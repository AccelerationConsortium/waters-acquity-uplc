#!/usr/bin/env python3
"""
GPC Orchestrator - Complete Workflow Management

Coordinates all three systems for complete GPC automation:
- Automation Portal (sample handling)
- Waters Empower (HPLC control)  
- ASTRA (light scattering analysis)

This orchestrator manages the logical sequence across all systems.
"""

import sys
import os
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Optional, Any

# Add component paths
sys.path.append(str(Path(__file__).parent / "automation-portal"))
sys.path.append(str(Path(__file__).parent / "empower"))
sys.path.append(str(Path(__file__).parent / "astra"))

# Import component classes
from automation_portal_driver import AutomationPortalDriver
from waters_empower import WatersEmpower
from gpc_automation_class import GPCAutomation


class GPCOrchestrator:
    """
    Complete GPC workflow orchestrator that coordinates:
    - Automation Portal for sample handling
    - Waters Empower for HPLC operations
    - ASTRA for light scattering data collection and analysis
    """
    
    def __init__(self, config: Dict[str, Any]):
        """
        Initialize the GPC orchestrator with configuration.
        
        Args:
            config: Configuration dictionary containing:
                - automation_portal: AP config
                - empower: Empower config  
                - astra: ASTRA config
                - workflow: General workflow settings
        """
        self.config = config
        self.log_entries = []
        
        # Component instances
        self.automation_portal = None
        self.empower = None
        self.astra = None
        
        # Current workflow state
        self.current_sample = None
        self.current_experiment_id = None
        self.is_initialized = False
    
    def log(self, message: str, level: str = "INFO"):
        """Log message with timestamp and level"""
        timestamp = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
        log_entry = f"[{timestamp}] [{level}] {message}"
        self.log_entries.append(log_entry)
        print(log_entry)
    
    def initialize_all_systems(self) -> bool:
        """
        Initialize all three component systems.
        
        Returns:
            bool: True if all systems initialized successfully
        """
        try:
            self.log("=== INITIALIZING ALL GPC SYSTEMS ===")
            
            # Initialize Automation Portal
            self.log("Initializing Automation Portal...")
            self.automation_portal = AutomationPortalDriver()
            if not self.automation_portal.connect():
                self.log("Failed to connect to Automation Portal", "ERROR")
                return False
            self.log("✓ Automation Portal connected")
            
            # Initialize Waters Empower
            self.log("Initializing Waters Empower...")
            self.empower = WatersEmpower()
            # Note: WatersEmpower doesn't have a connect() method - it uses subprocess calls
            self.log("✓ Waters Empower initialized")
            
            # Check if Empower instrument is ready (following usage_examples pattern)
            self.log("Checking Empower instrument readiness...")
            if not self.empower.is_ready():
                self.log("Empower instrument not ready for operation", "ERROR")
                return False
            self.log("✓ Empower instrument ready")
            
            # Initialize ASTRA
            self.log("Initializing ASTRA...")
            self.astra = GPCAutomation(
                base_results_dir=self.config['astra']['results_dir']
            )
            if not self.astra.initialize_admin():
                self.log("Failed to initialize ASTRA admin", "ERROR")
                return False
            self.log("✓ ASTRA admin initialized")
            
            self.is_initialized = True
            self.log("🎉 ALL SYSTEMS INITIALIZED SUCCESSFULLY")
            return True
            
        except Exception as e:
            self.log(f"System initialization failed: {e}", "ERROR")
            return False
    
    def cleanup_all_systems(self):
        """Clean up all system connections"""
        try:
            self.log("=== CLEANING UP ALL SYSTEMS ===")
            
            if self.automation_portal:
                self.automation_portal.disconnect()
                self.log("✓ Automation Portal disconnected")
            
            if self.empower:
                # WatersEmpower doesn't require explicit disconnection
                self.log("✓ Empower connection cleaned up")
                
            if self.astra:
                self.astra.cleanup_admin()
                self.log("✓ ASTRA admin cleaned up")
                
            self.is_initialized = False
            
        except Exception as e:
            self.log(f"Cleanup warning: {e}", "WARN")
    
    def run_sample_workflow(self, sample_info: Dict[str, Any]) -> Dict[str, Any]:
        """
        Run complete workflow for a single sample.
        
        Args:
            sample_info: Dictionary containing:
                - sample_name: str
                - tray: int (tray number for both automation portal and empower)
                - vial_position: str (grid position within tray like 'A1', 'B2')
                - astra_method_path: str
                - empower_template: str (optional, defaults to '20251002_KC')
                - injection_volume: float (optional, µL)
                - initial_tray_open: bool (optional, whether to extract tray for loading, default False)
                - send_out_after: bool (optional, whether to extract tray after completion, default True)
                - astra_ready_delay: float (optional, seconds to wait before Empower execution, default 5.0)
                - runtime: float (optional, minutes)
                - sample_weight: float (optional)
                - dilution_factor: float (optional)
                
        Returns:
            dict: Results of the complete workflow
        """
        if not self.is_initialized:
            return {'success': False, 'error': 'Systems not initialized'}
        
        sample_name = sample_info['sample_name']
        self.current_sample = sample_name
        
        self.log(f"\\n{'='*60}")
        self.log(f"STARTING WORKFLOW FOR SAMPLE: {sample_name}")
        self.log(f"{'='*60}")
        
        try:
            # Phase 1: Setup ASTRA experiment
            self.log("=== PHASE 1: ASTRA EXPERIMENT SETUP ===")
            
            # Allow per-run override of ASTRA results directory via sample_info
            results_override = sample_info.get('results_dir')
            if results_override:
                try:
                    self.astra.base_results_dir = results_override
                    self.log(f"Using per-run ASTRA results_dir: {results_override}")
                except Exception as e:
                    self.log(f"⚠ Failed to set per-run results_dir '{results_override}': {e}", "WARN")
            
            astra_result = self.astra.prepare_experiment_for_collection(
                sample_info['astra_method_path'],
                experiment_name=sample_name
            )
            
            if not astra_result['success']:
                return {'success': False, 'error': 'ASTRA preparation failed', 'phase': 'astra_prep'}
            
            experiment_id = astra_result['experiment_id']
            self.log(f"✓ ASTRA experiment ready (ID: {experiment_id})")
            self.log(f"ASTRA results folder: {astra_result['results_folder']}")
            
            # Phase 2A: Optional tray extraction and sample loading
            initial_tray_open = sample_info.get('initial_tray_open', False)
            tray = sample_info['tray']
            
            if initial_tray_open:
                self.log("=== PHASE 2A: TRAY EXTRACTION FOR SAMPLE LOADING ===")
                
                # Use improved multi-tray logic
                if not self.ensure_tray_available(tray):
                    return {'success': False, 'error': f'Failed to make tray {tray} available', 'phase': 'tray_extract'}
                
                # Phase 3: Sample loading
                self.log("=== PHASE 3: SAMPLE LOADING ===")
                self.log(f"Please load sample '{sample_name}' into tray {tray}, position {sample_info['vial_position']}")
                input("Press Enter when sample is loaded and ready...")
                self.log("✓ Sample loading confirmed")
            else:
                self.log("=== PHASE 2A: SKIPPING TRAY EXTRACTION (initial_tray_open=False) ===")
                self.log(f"Assuming samples are already loaded in tray {tray}")
            
            # Phase 2B: Always ensure tray is inserted (regardless of initial state)
            self.log("=== PHASE 2B: ENSURING TRAY IS INSERTED ===")
            
            tray_was_inserted = False
            
            # Check current tray status
            status = self.automation_portal.get_status()
            if status.get('success', False):
                drawer_status = status.get('drawer_tray_status', '')
                mode = status.get('mode', '')
                self.log(f"Current tray status: {drawer_status}, Mode: {mode}")
                
                # Check if the target tray is currently extracted
                if self._is_tray_extracted(drawer_status, mode, tray):
                    self.log(f"Tray {tray} is currently out - inserting it now")
                    if not self.automation_portal.insert_drawer(tray):
                        return {'success': False, 'error': f'Failed to insert tray {tray}', 'phase': 'tray_insert'}
                    self.log(f"✓ Tray {tray} insertion command sent")
                    tray_was_inserted = True
                    
                elif drawer_status == 'NoDrawerNoTray':
                    self.log("✓ All trays are already in position")
                else:
                    # Some other tray might be out - insert it
                    current_tray_out = self._parse_current_tray_from_mode(mode)
                    if current_tray_out is not None:
                        self.log(f"Tray {current_tray_out} is currently out - inserting it")
                        if not self.automation_portal.insert_drawer(current_tray_out):
                            return {'success': False, 'error': f'Failed to insert tray {current_tray_out}', 'phase': 'tray_insert'}
                        self.log(f"✓ Tray {current_tray_out} insertion command sent")
                        tray_was_inserted = True
                        
                    self.log("✓ All trays are now in position")
            else:
                self.log("⚠ Could not check tray status - assuming trays are in position", "WARN")
            
            # Additional stabilization delay only if we actually inserted a tray
            if tray_was_inserted:
                self.log("Allowing automation portal and detectors to stabilize after tray insertion...")
                import time
                time.sleep(30.0)  # 30-second stabilization period for detector/instrument settling
                self.log("✓ System stabilization complete")
            else:
                self.log("✓ No tray insertion required - proceeding immediately")
            
            # Phase 4: ASTRA WAIT FOR INJECTION SIGNAL (MAIN THREAD)
            self.log("=== PHASE 4: ASTRA WAITING FOR GPC AUTO-INJECT SIGNAL ===")
            
            # Start ASTRA waiting for injection signal (in main thread for COM compatibility)
            self.log("Starting ASTRA wait for auto-inject signal...")
            self.astra.admin.wait_waiting_for_auto_inject()
            self.log("✓ ASTRA is now waiting for GPC auto-inject signal")
            
            # Phase 5: START EMPOWER THREAD WITH DELAY
            self.log("=== PHASE 5: STARTING EMPOWER THREAD ===")
            
            import threading
            import time
            
            # Container to store Empower thread results
            empower_execution_result = {}
            # Use shorter, simpler sample set name to avoid COM issues
            simple_timestamp = astra_result['timestamp'].replace('_', '').replace('-', '')[:12]  # YYYYMMDDHHmm
            sample_set_name = f"GPC_{simple_timestamp}"
            
            # Check if manual execution mode is enabled
            manual_mode = self.config.get('empower', {}).get('manual_execution', False)
            
            def empower_execution_thread():
                """Background thread for Empower execution with delay"""
                try:
                    # Configurable delay to give ASTRA time to start collecting
                    empower_delay = sample_info.get('astra_ready_delay', 120.0)  # Default 120 seconds as suggested
                    self.log(f"Empower thread: Waiting {empower_delay} seconds to give ASTRA time to start collecting...")
                    time.sleep(empower_delay)
                    
                    if manual_mode:
                        # MANUAL MODE: Skip automated execution, just provide instructions
                        self.log("🔧 MANUAL MODE ENABLED - Skipping automated Empower execution")
                        
                        # Still create sample set parameters for manual reference
                        vial_pos = sample_info['vial_position']  # e.g., "A2"
                        vial_row = vial_pos[0]      # "A"  
                        vial_col = vial_pos[1:]     # "2"
                        vial_string = f"{tray}:{vial_row},{vial_col}"  # Format: "1:A,2" (correct GPC format)
                        template_name = sample_info.get('empower_template', '20251002_KC')

                        print("Vial String:", vial_string)

                        create_params = {
                            'template': template_name,
                            'injection_volume': sample_info.get('injection_volume', 10.0)
                        }
                        if vial_string:
                            create_params['vials'] = vial_string
                        if sample_name:
                            create_params['sample_names'] = sample_name
                        if sample_info.get('runtime') is not None:
                            create_params['runtime'] = sample_info.get('runtime')
                        if sample_info.get('sample_weight') is not None:
                            create_params['sample_weight'] = sample_info.get('sample_weight')
                        if sample_info.get('dilution_factor') is not None:
                            create_params['dilution'] = sample_info.get('dilution_factor')
                        
                        self.log("🎯 MANUAL MODE - Please create and execute sample set manually in Empower:")
                        self.log(f"   Sample set name: {sample_set_name}")
                        self.log(f"   Parameters: {create_params}")
                        self.log("   ⚠ IMPORTANT: Execute the sample set now to send injection signal to ASTRA!")
                        
                        # Wait for manual execution confirmation
                        input("   Press Enter after you have started the Empower execution...")
                        self.log("✓ Manual execution confirmed - assuming injection signal sent")
                        
                        # Mock successful execution for manual mode
                        empower_execution_result.update({
                            'success': True,
                            'sample_set_name': sample_set_name,
                            'execution_result': {'return_code': 0, 'execution_started': True, 'status': 'manual_execution'}
                        })
                        return
                    
                    # AUTOMATED MODE: Full automated execution
                    self.log("🔄 AUTOMATED MODE - Running full Empower automation")
                    
                    # Create Empower sample set
                    self.log("Empower thread: Creating sample set...")
                    vial_pos = sample_info['vial_position']  # e.g., "A2"
                    # Parse vial position correctly: "A2" -> tray:A,2 (correct GPC format)
                    vial_row = vial_pos[0]      # "A"  
                    vial_col = vial_pos[1:]     # "2"
                    vial_string = f"{tray}:{vial_row},{vial_col}"  # Format: "1:A,2"
                    print("Vials", vial_string)
                    
                    create_params = {
                        'template': sample_info.get('empower_template', '20251002_KC'),
                        'injection_volume': sample_info.get('injection_volume', 10.0)
                    }
                    # Only add vial positions and sample names if provided
                    if vial_string:
                        create_params['vials'] = vial_string
                    if sample_name:
                        create_params['sample_names'] = sample_name
                    # Only add optional parameters if they're explicitly provided
                    if sample_info.get('runtime') is not None:
                        create_params['runtime'] = sample_info.get('runtime')
                    if sample_info.get('sample_weight') is not None:
                        create_params['sample_weight'] = sample_info.get('sample_weight')
                    if sample_info.get('dilution_factor') is not None:
                        create_params['dilution'] = sample_info.get('dilution_factor')  # Note: C# exe expects 'dilution', not 'dilution_factor'
                    
                    self.log(f"Empower thread: Attempting to create sample set with params: {create_params}")
                    create_result = self.empower.create_sample_set(sample_set_name, **create_params)
                    self.log(f"Empower thread: create_sample_set returned: {create_result}")
                    
                    if not create_result:
                        # Get more detailed error information
                        self.log(f"Empower thread: ❌ Sample set creation failed for '{sample_set_name}'", "ERROR")
                        self.log(f"Empower thread: Used parameters: {create_params}", "ERROR")
                        empower_execution_result.update({
                            'success': False, 
                            'error': f'Empower sample set creation failed for {sample_set_name} with params {create_params}'
                        })
                        return
                    
                    self.log(f"Empower thread: ✓ Sample set '{sample_set_name}' created successfully")
                    
                    # Execute the newly created sample set (this will trigger ASTRA injection signal)
                    self.log("Empower thread: Executing newly created sample set (sending injection signal to ASTRA)...")
                    self.log(f"🎯 TIMING: About to send injection signal to ASTRA via Empower execution of sample set '{sample_set_name}'")
                    execution_result = self.empower.execute_sample_set(sample_set_name)
                    self.log(f"🎯 TIMING: Empower execution command completed")
                    self.log(f"Empower thread: Full execution result: {execution_result}")
                    
                    # Check if execution started successfully
                    return_code = execution_result.get('return_code', -999)
                    self.log(f"Empower thread: Execution return code: {return_code}")
                    
                    if return_code != 0:
                        stderr_msg = execution_result.get('stderr', 'No stderr available')
                        stdout_msg = execution_result.get('stdout', 'No stdout available')
                        self.log(f"Empower thread: ❌ Execution failed with return code {return_code}", "ERROR")
                        self.log(f"Empower thread: STDERR: {stderr_msg}", "ERROR")
                        self.log(f"Empower thread: STDOUT: {stdout_msg}", "ERROR")
                        empower_execution_result.update({
                            'success': False,
                            'error': f'Empower execution failed: return_code={return_code}, stderr={stderr_msg}, stdout={stdout_msg}'
                        })
                        return
                    
                    execution_started = execution_result.get('execution_started', None)
                    status = execution_result.get('status', 'unknown')
                    self.log(f"Empower thread: execution_started={execution_started}, status='{status}'")
                    
                    if not execution_started:
                        # Check if it's already running or ready
                        if status == 'busy':
                            self.log("Empower thread: ✓ Empower is busy - execution likely started")
                        elif execution_result.get('ready', False):
                            self.log("Empower thread: ✓ Empower execution command sent successfully")
                        else:
                            self.log(f"Empower thread: ❌ Execution may have failed - status: '{status}'", "ERROR")
                            self.log(f"Empower thread: Full execution_result: {execution_result}", "ERROR")
                            empower_execution_result.update({
                                'success': False,
                                'error': f'Empower execution may have failed: status={status}, full_result={execution_result}'
                            })
                            return
                    else:
                        self.log("Empower thread: ✓ Empower execution started - injection signal sent to ASTRA")
                    
                    empower_execution_result.update({
                        'success': True,
                        'sample_set_name': sample_set_name,
                        'execution_result': execution_result
                    })
                    
                except Exception as e:
                    self.log(f"❌ Empower thread error: {e}", "ERROR")
                    empower_execution_result.update({
                        'success': False,
                        'error': str(e)
                    })
            
            # Start Empower execution thread
            empower_thread = threading.Thread(target=empower_execution_thread, name="EmpowerExecution")
            empower_thread.start()
            self.log("✓ Empower execution thread started with delay")
            
            # Phase 6: CONTINUE ASTRA COLLECTION IN MAIN THREAD
            self.log("=== PHASE 6: ASTRA DATA COLLECTION ===")
            
            # Wait for collection to start (injection signal from Empower)
            self.log("Waiting for collection to start (injection signal from Empower)...")
            self.astra.admin.wait_collection_started()
            self.log("✓ Data collection started")
            
            # Wait for collection to finish
            self.log("Waiting for collection to finish...")
            collection_start_time = datetime.now()
            self.astra.admin.wait_collection_finished()
            collection_end_time = datetime.now()
            collection_duration = (collection_end_time - collection_start_time).total_seconds() / 60
            self.log(f"✅ ASTRA data collection completed ({collection_duration:.2f} minutes)")
            
            # Wait for Empower thread to complete
            self.log("Waiting for Empower thread to complete...")
            empower_thread.join()
            
            # Check Empower execution results
            if not empower_execution_result.get('success', False):
                return {'success': False, 'error': f'Empower execution failed: {empower_execution_result.get("error", "Unknown error")}', 'phase': 'empower_execution'}
            
            self.log("✓ Empower execution thread completed successfully")
            
            # Phase 7: Process and save ASTRA results
            self.log("=== PHASE 7: ASTRA DATA PROCESSING ===")
            processing_result = self.astra.process_and_save_results(
                experiment_id,
                astra_result['results_folder'],
                astra_result['timestamp'],
                collection_duration
            )
            
            if not processing_result['success']:
                return {'success': False, 'error': 'ASTRA data processing failed', 'phase': 'data_processing'}
            
            # Phase 8: Cleanup everything
            self.log("=== PHASE 8: CLEANUP ===")
            self.astra.close_experiment(experiment_id)
            self.log("✓ ASTRA experiment closed")
            
            # Phase 9: Optional tray extraction after completion
            send_out_after = sample_info.get('send_out_after', True)
            if send_out_after:
                self.log("=== PHASE 9: POST-COMPLETION TRAY EXTRACTION ===")
                if not self.automation_portal.extract_drawer(tray):
                    self.log(f"Warning: Failed to extract tray {tray} after completion", "WARN")
                else:
                    self.log(f"✓ Tray {tray} extracted for sample collection")
            else:
                self.log("=== PHASE 9: SKIPPING POST-COMPLETION TRAY EXTRACTION (send_out_after=False) ===")
            
            # Create ASTRA collection results object for compatibility
            astra_collection_result = {
                'success': True,
                'collection_duration_minutes': collection_duration,
                'collection_start_time': collection_start_time,
                'collection_end_time': collection_end_time
            }
            
            # Compile final results
            workflow_result = {
                'success': True,
                'sample_name': sample_name,
                'experiment_id': experiment_id,
                'sample_set_name': sample_set_name,
                'template_name': sample_info.get('empower_template', '20251002_KC'),
                'astra_collection_results': astra_collection_result,
                'empower_execution_results': empower_execution_result,
                'astra_processing_results': processing_result,
                'results_folder': astra_result['results_folder'],
                'timestamp': astra_result['timestamp']
            }
            

            self.log(f"\n🎉 SAMPLE {sample_name} WORKFLOW COMPLETED SUCCESSFULLY")
            return workflow_result
            
        except KeyboardInterrupt:
            # Gracefully stop ASTRA collection on interrupt
            try:
                self.log("⚠ KeyboardInterrupt received - attempting to stop ASTRA collection", "WARN")
                if 'experiment_id' in locals() and experiment_id is not None and getattr(self.astra, 'admin', None):
                    stopped = self.astra.admin.stop_collection(experiment_id)
                    if stopped:
                        self.log("✓ ASTRA collection stopped")
                    else:
                        self.log("⚠ Failed to stop ASTRA collection via API", "WARN")
                    # Try to close experiment to release resources
                    self.astra.close_experiment(experiment_id)
                else:
                    self.log("⚠ No active experiment to stop", "WARN")
            except Exception as ie:
                self.log(f"⚠ Interrupt handling error: {ie}", "WARN")
            return {'success': False, 'error': 'Interrupted by user', 'phase': 'interrupt'}
        except Exception as e:
            self.log(f"Sample workflow failed: {e}", "ERROR")
            return {'success': False, 'error': str(e), 'phase': 'unknown'}
        
        finally:
            self.current_sample = None
    
    def get_workflow_status(self) -> Dict[str, Any]:
        """Get current workflow status"""
        return {
            'is_initialized': self.is_initialized,
            'current_sample': self.current_sample,
            'current_experiment_id': self.current_experiment_id,
            'recent_logs': self.log_entries[-5:]  # Last 5 log entries
        }

    def _parse_current_tray_from_mode(self, mode: str) -> Optional[int]:
        """
        Parse which tray is currently out from the Mode field.
        
        Args:
            mode: Mode string like 'Extract(1)', 'Insert(0)', etc.
            
        Returns:
            Tray number (1 or 2) if a tray is out, None if all trays are in
        """
        import re
        
        # Extract the number from Extract(N) or Insert(N)
        match = re.search(r'(Extract|Insert)\((\d+)\)', mode)
        if not match:
            return None
            
        tray_mode, tray_num_str = match.groups()
        tray_num = int(tray_num_str)
        
        # Convert 0-based to 1-based tray numbering if needed
        # Based on your data: Extract(1) = Tray 1, Extract(0) = Tray 2
        if tray_num == 1:
            return 1  # Tray 1
        elif tray_num == 0:
            return 2  # Tray 2
        else:
            return None
    
    def _is_tray_extracted(self, drawer_status: str, mode: str, target_tray: int) -> bool:
        """
        Check if the specific target tray is currently extracted.
        
        Args:
            drawer_status: The drawer_tray_status value
            mode: The mode string
            target_tray: The tray we want to check (1 or 2)
            
        Returns:
            True if target_tray is currently extracted
        """
        # Check if any tray is out
        if drawer_status == 'NoDrawerNoTray':
            return False  # All trays are in
            
        # Parse which tray is currently out
        current_tray_out = self._parse_current_tray_from_mode(mode)
        return current_tray_out == target_tray
    
    def ensure_tray_available(self, target_tray: int) -> bool:
        """
        Ensure the specified tray is available (extracted) for sample loading.
        Handles the case where the wrong tray might be currently extracted.
        
        Args:
            target_tray: The tray number (1 or 2) that needs to be extracted
            
        Returns:
            bool: True if target tray is now available, False on failure
        """
        try:
            # Get current status
            status = self.automation_portal.get_status()
            if not status.get('success', False):
                self.log("⚠ Could not check tray status, attempting extraction anyway")
                return self.automation_portal.extract_drawer(target_tray)
            
            drawer_status = status.get('drawer_tray_status', '')
            mode = status.get('mode', '')
            self.log(f"Current system status: {drawer_status}, Mode: {mode}")
            
            # Check if target tray is already out
            if self._is_tray_extracted(drawer_status, mode, target_tray):
                self.log(f"✓ Tray {target_tray} is already extracted")
                return True
            
            # Check if a different tray is currently out
            current_tray_out = self._parse_current_tray_from_mode(mode)
            
            if current_tray_out is not None and current_tray_out != target_tray:
                # Wrong tray is out - insert it first
                self.log(f"Tray {current_tray_out} is currently out, but we need Tray {target_tray}")
                self.log(f"Inserting Tray {current_tray_out} first...")
                
                if not self.automation_portal.insert_drawer(current_tray_out):
                    self.log(f"Failed to insert Tray {current_tray_out}", "ERROR")
                    return False
                self.log(f"✓ Tray {current_tray_out} inserted")
                
                # Brief delay to let system update
                import time
                time.sleep(2)
            
            # Now extract the target tray
            self.log(f"Extracting Tray {target_tray}...")
            if not self.automation_portal.extract_drawer(target_tray):
                self.log(f"Failed to extract Tray {target_tray}", "ERROR")
                return False
                
            self.log(f"✓ Tray {target_tray} extracted successfully")
            return True
            
        except Exception as e:
            self.log(f"Tray management failed: {e}", "ERROR")
            return False

def create_default_config() -> Dict[str, Any]:
    """Create a default configuration for the orchestrator"""
    return {
        'automation_portal': {
            'connection_timeout': 30,
            'operation_timeout': 60
        },
        'empower': {
            'server_name': 'localhost',
            'project_name': 'GPC_Automation',
            'method_set': 'Default_Methods',
            'manual_execution': False  # Set to True to skip automated Empower execution (dev mode)
        },
        'astra': {
            'results_dir': r'C:\\GPC_Results',
            'app_name': 'GPC Orchestrator',
            'app_version': '1.0.0'
        },
        'workflow': {
            'default_injection_volume': 10.0,
            'inter_sample_delay': 30  # seconds between samples
        }
    }


def main():
    """Example usage of the GPC orchestrator"""
    
    # Create configuration
    config = create_default_config()
    
    # DEVELOPMENT MODE: Toggle between manual and automated Empower execution
    # Set to True to skip automated Empower calls (manual testing)
    # Set to False for full automation
    MANUAL_EMPOWER_MODE = False  # <-- Change this to switch modes easily
    config['empower']['manual_execution'] = MANUAL_EMPOWER_MODE
    
    if MANUAL_EMPOWER_MODE:
        print("🔧 DEVELOPMENT MODE: Manual Empower execution enabled")
    else:
        print("🔄 PRODUCTION MODE: Automated Empower execution enabled")
    
    # Create orchestrator
    orchestrator = GPCOrchestrator(config)
    
    try:
        # Initialize all systems
        if not orchestrator.initialize_all_systems():
            print("Failed to initialize systems")
            return
        
        # Example single sample
        sample_info = {
            'sample_name': 'OAM_Testing_PS',
            'tray': 1,              # Tray number for both systems
            'vial_position': 'A2',  # Grid position within tray (A1, B2, etc.)
            'astra_method_path': r'//dbf/Method Builder/Owen/test_method_3',
            'empower_template': '20251002_KC',  # Actual default template from Empower
            'injection_volume': 15.0,
            'initial_tray_open': False,    # Whether to extract tray for sample loading
            'send_out_after': True,        # Whether to extract tray after completion
            'astra_ready_delay': 45.0,      # Seconds to wait before Empower execution (ensure ASTRA is ready)
            'results_dir': r'C:\\GPC_Results\\Runs',  # Optional per-run ASTRA results directory
            # Optional Empower parameters:
            'runtime': 10,          # Runtime in minutes
            # 'sample_weight': 1.5,   # Sample weight
            # 'dilution_factor': 2.0  # Dilution factor
        }
        
        try:
            # Run single sample workflow
            result = orchestrator.run_sample_workflow(sample_info)
        except KeyboardInterrupt:
            print("\n⚠ Interrupted by user - attempting to stop ASTRA collection")
            # Best-effort stop: we don't have experiment_id here, rely on internal handler
            # Additional cleanup will happen in finally
            return
        
        if result.get('success', False):
            print("\n🎉 SAMPLE WORKFLOW COMPLETED SUCCESSFULLY")
            print(f"Results saved to: {result['results_folder']}")
        else:
            print(f"\n❌ SAMPLE WORKFLOW FAILED: {result.get('error', 'Unknown error')}")
    
    finally:
        orchestrator.cleanup_all_systems()


if __name__ == "__main__":
    main()