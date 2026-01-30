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
            astra_result = self.astra.prepare_experiment_for_collection(
                sample_info['astra_method_path'],
                sample_name
            )
            
            if not astra_result['success']:
                return {'success': False, 'error': 'ASTRA preparation failed', 'phase': 'astra_prep'}
            
            experiment_id = astra_result['experiment_id']
            self.log(f"✓ ASTRA experiment ready (ID: {experiment_id})")
            
            # Phase 2: Tray out (check if already out first)
            self.log("=== PHASE 2: TRAY EXTRACTION ===")
            tray = sample_info['tray']
            
            # Check current tray status
            status = self.automation_portal.get_status()
            if status.get('success', False):
                drawer_status = status.get('drawer_tray_status', '')
                self.log(f"Current tray status: {drawer_status}")
                
                if 'NoDrawer' in drawer_status or 'NoTray' in drawer_status:
                    if not self.automation_portal.extract_drawer(tray):
                        return {'success': False, 'error': f'Failed to extract tray {tray}', 'phase': 'tray_extract'}
                    self.log(f"✓ Tray {tray} extracted")
                else:
                    self.log(f"✓ Tray {tray} already extracted")
            else:
                self.log("⚠ Could not check tray status, attempting extraction anyway")
                if not self.automation_portal.extract_drawer(tray):
                    return {'success': False, 'error': f'Failed to extract tray {tray}', 'phase': 'tray_extract'}
                self.log(f"✓ Tray {tray} extracted")
            
            # Phase 3: Wait for sample loading
            self.log("=== PHASE 3: SAMPLE LOADING ===")
            self.log(f"Please load sample '{sample_name}' into tray {tray}, position {sample_info['vial_position']}")
            input("Press Enter when sample is loaded and ready...")
            self.log("✓ Sample loading confirmed")
            
            # Phase 4: Tray in
            self.log("=== PHASE 4: TRAY INSERTION ===")
            if not self.automation_portal.insert_drawer(tray):
                return {'success': False, 'error': f'Failed to insert tray {tray}', 'phase': 'tray_insert'}
            self.log(f"✓ Tray {tray} inserted")
            
            # Phase 5: ASTRA wait for injection (this passes automatically according to user)
            self.log("=== PHASE 5: ASTRA INJECTION WAIT ===")
            if not self.astra.wait_for_injection_signal(experiment_id):
                return {'success': False, 'error': 'ASTRA injection wait failed', 'phase': 'injection_wait'}
            self.log("✓ ASTRA injection wait completed")
            
            # Phase 6: ASTRA wait for collection to start (this hangs until empower starts)
            self.log("=== PHASE 6: ASTRA DATA COLLECTION PREPARATION ===")
            
            # Create Empower sample set first
            self.log("Creating Empower sample set...")
            sample_set_name = f"{sample_name}_{astra_result['timestamp']}"
            # Build empower vial string from tray and vial position
            vial_pos = sample_info['vial_position']  # e.g., "A1"
            vial_string = f"{tray}:{vial_pos},1"  # Format: "tray:position,vial_number"
            
            create_params = {
                'template': sample_info.get('empower_template', '20251002_KC'),
                'injection_volume': sample_info.get('injection_volume', 10.0),
                'vials': vial_string,
                'sample_names': sample_name,
                'runtime': sample_info.get('runtime'),
                'sample_weight': sample_info.get('sample_weight'),
                'dilution': sample_info.get('dilution_factor')
            }
            # Remove None values
            create_params = {k: v for k, v in create_params.items() if v is not None}
            
            if not self.empower.create_sample_set(sample_set_name, **create_params):
                return {'success': False, 'error': 'Empower sample set creation failed', 'phase': 'empower_create'}
            self.log(f"✓ Sample set '{sample_set_name}' created")
            
            # Phase 7: Execute Empower while starting ASTRA collection
            self.log("=== PHASE 7: COORDINATED EMPOWER & ASTRA EXECUTION ===")
            self.log("Starting Empower execution to trigger ASTRA collection...")
            
            execution_result = self.empower.execute_sample_set(sample_set_name)
            if not execution_result.get('execution_started', False):
                return {'success': False, 'error': 'Empower sample set execution failed', 'phase': 'empower_execute'}
            self.log("✓ Empower execution started")
            
            # Phase 8: ASTRA data collection and processing  
            self.log("=== PHASE 8: ASTRA DATA COLLECTION & PROCESSING ===")
            collection_result = self.astra.collect_and_process_data(
                experiment_id,
                astra_result['results_folder'],
                astra_result['timestamp']
            )
            
            if not collection_result['success']:
                return {'success': False, 'error': 'ASTRA data collection failed', 'phase': 'data_collection'}
            
            self.log(f"✓ Data collection completed ({collection_result['collection_duration_minutes']:.2f} min)")
            
            # Phase 9: Cleanup everything
            self.log("=== PHASE 9: CLEANUP ===")
            self.astra.close_experiment(experiment_id)
            self.log("✓ ASTRA experiment closed")
            
            # Compile final results
            workflow_result = {
                'success': True,
                'sample_name': sample_name,
                'experiment_id': experiment_id,
                'sample_set_name': sample_set_name,
                'astra_results': collection_result,
                'results_folder': astra_result['results_folder'],
                'timestamp': astra_result['timestamp']
            }
            

            self.log(f"\\n🎉 SAMPLE {sample_name} WORKFLOW COMPLETED SUCCESSFULLY")
            return workflow_result
            
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
            'method_set': 'Default_Methods'
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
    
    # Create orchestrator
    orchestrator = GPCOrchestrator(config)
    
    try:
        # Initialize all systems
        if not orchestrator.initialize_all_systems():
            print("Failed to initialize systems")
            return
        
        # Example single sample
        sample_info = {
            'sample_name': 'BSA_Standard_1',
            'tray': 1,              # Tray number for both systems
            'vial_position': 'A1',  # Grid position within tray (A1, B2, etc.)
            'astra_method_path': r'//dbf/Method Builder/Owen/test_method_3',
            'empower_template': '20251002_KC',  # Actual default template from Empower
            'injection_volume': 10.0,
            # Optional Empower parameters:
            # 'runtime': 30,          # Runtime in minutes
            # 'sample_weight': 1.5,   # Sample weight 
            # 'dilution_factor': 2.0  # Dilution factor
        }
        
        # Run single sample workflow
        result = orchestrator.run_sample_workflow(sample_info)
        
        if result.get('success', False):
            print("\\n🎉 SAMPLE WORKFLOW COMPLETED SUCCESSFULLY")
            print(f"Results saved to: {result['results_folder']}")
        else:
            print(f"\\n❌ SAMPLE WORKFLOW FAILED: {result.get('error', 'Unknown error')}")
    
    finally:
        orchestrator.cleanup_all_systems()


if __name__ == "__main__":
    main()