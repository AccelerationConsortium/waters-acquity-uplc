#!/usr/bin/env python3
"""
GPC Automation Class - Object-oriented ASTRA Workflow

Provides a class-based interface for ASTRA GPC automation with methods
that can be called separately for integration with external systems.
"""

import os
import uuid
import re
import shutil
from datetime import datetime
from astra_admin import AstraAdmin


class GPCAutomation:
    """
    Object-oriented interface for ASTRA GPC automation workflow.
    
    Separates the workflow into distinct phases to allow integration
    with external HPLC/Empower systems.
    """
    
    def __init__(self, base_results_dir, 
                 app_name="GPC Automation", app_version="2.0.0"):
        """
        Initialize GPC automation admin connection.
        
        Args:
            base_results_dir: Directory for saving results
            app_name: Application name for ASTRA identity
            app_version: Application version for ASTRA identity
        """
        self.base_results_dir = base_results_dir
        self.app_name = app_name
        self.app_version = app_version
        
        # Admin connection state (shared across experiments)
        self.admin = None
        self.client_id = None
        self.is_initialized = False
        
        # Export configuration (less frequently changed)
        self.export_xml_results = True
        self.export_csv_datasets = True
        self.export_experiment_file = True
        self.create_summary_file = True
        self.show_molecular_weights = True
        
        # Dataset exports to perform
        self.dataset_exports = [
            ("masses vs volume", "Mass chromatogram data"),
            ("rms radius vs volume", "RMS radius chromatogram data")
        ]
        
        # Display precision
        self.pdi_decimal_places = 3
        self.mw_decimal_places = 1
    
    def log(self, message: str):
        """Log with timestamp"""
        timestamp = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
        print(f"[{timestamp}] {message}")
    
    def initialize_admin(self):
        """
        Initialize ASTRA admin connection once.
        This should be called before any experiment operations.
        
        Returns:
            bool: True if initialization successful, False otherwise
        """
        if self.is_initialized:
            self.log("✓ Admin already initialized")
            return True
            
        try:
            self.log("=== Initializing ASTRA Admin Connection ===")
            self.client_id = uuid.uuid4().hex
            
            self.admin = AstraAdmin()
            self.admin.set_automation_identity(
                self.app_name,
                self.app_version,
                os.getpid(),
                self.client_id,
                1
            )
            self.log("✓ Automation identity set")
            
            self.admin.wait_for_instruments()
            self.log("✓ Instruments detected")
            
            self.is_initialized = True
            return True
            
        except Exception as e:
            self.log(f"❌ Admin initialization failed: {e}")
            return False
    
    def create_results_folder(self, experiment_name=None):
        """
        Create a timestamped results folder for an experiment.
        
        Args:
            experiment_name: Optional name to include in folder name
            
        Returns:
            tuple: (timestamp, results_folder_path) or (None, None) if failed
        """
        try:
            timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
            folder_name = f"gpc_run_{timestamp}"
            if experiment_name:
                folder_name = f"gpc_run_{experiment_name}_{timestamp}"
                
            results_folder = os.path.join(self.base_results_dir, folder_name)
            os.makedirs(results_folder, exist_ok=True)
            
            self.log(f"📁 Created results folder: {os.path.basename(results_folder)}")
            return timestamp, results_folder
            
        except Exception as e:
            self.log(f"❌ Failed to create results folder: {e}")
            return None, None
    
    def prepare_experiment_for_collection(self, astra_method_path, experiment_name=None):
        """
        Phase 1: Create new experiment and prepare for data collection.
        
        Args:
            astra_method_path: Full path to ASTRA method template
            experiment_name: Optional name for the experiment
        
        Returns:
            dict: {
                'success': bool,
                'experiment_id': str or None,
                'timestamp': str or None, 
                'results_folder': str or None
            }
        """
        if not self.is_initialized:
            if not self.initialize_admin():
                return {'success': False, 'experiment_id': None, 'timestamp': None, 'results_folder': None}
        
        try:
            self.log("=== Phase 1: Preparing New Experiment for Collection ===")
            
            # Create results folder for this experiment
            timestamp, results_folder = self.create_results_folder(experiment_name)
            if not results_folder:
                return {'success': False, 'experiment_id': None, 'timestamp': None, 'results_folder': None}
            
            # Create experiment from template
            experiment_id = self.admin.new_experiment_from_template(astra_method_path)
            self.log(f"✓ Experiment created - ID: {experiment_id}")
            
            # Start collection (but won't actually collect until inject signal)
            self.admin.start_collection(experiment_id)
            self.log("✓ Collection started - waiting for inject signal")
            
            return {
                'success': True,
                'experiment_id': experiment_id,
                'timestamp': timestamp,
                'results_folder': results_folder
            }
            
        except Exception as e:
            self.log(f"❌ Experiment preparation failed: {e}")
            return {'success': False, 'experiment_id': None, 'timestamp': None, 'results_folder': None}
    
    def wait_for_injection_signal(self, experiment_id):
        """
        Phase 2: Wait for external injection signal from HPLC/Empower.
        
        Args:
            experiment_id: ID of the experiment waiting for injection
        
        Returns:
            bool: True if signal received, False if error/timeout
        """
        try:
            self.log(f"=== Phase 2: Waiting for External Injection Signal (Exp: {experiment_id}) ===")
            self.log("🔄 READY FOR HPLC INJECTION - Waiting for auto-inject signal...")
            
            # This blocks until external inject signal received
            self.admin.wait_waiting_for_auto_inject()
            self.log("✓ Injection signal received from external system!")
            
            return True
            
        except Exception as e:
            self.log(f"❌ Failed waiting for injection signal: {e}")
            return False
    
    def collect_and_process_data(self, experiment_id, results_folder, timestamp):
        """
        Phase 3: Collect data and process results.
        
        Args:
            experiment_id: ID of the experiment to collect data for
            results_folder: Path to save results files
            timestamp: Timestamp string for file naming
        
        Returns:
            dict: {
                'success': bool,
                'collection_duration_minutes': float or None,
                'exported_files': list of file paths,
                'molecular_weights': dict or None
            }
        """
        try:
            self.log(f"=== Phase 3: Data Collection and Processing (Exp: {experiment_id}) ===")
            
            exported_files = []
            
            # Step 6: Wait for collection to start
            self.admin.wait_collection_started()
            self.log("✓ Data collection started")
            
            # Step 7: Wait for collection to finish
            collection_start_time = datetime.now()
            self.admin.wait_collection_finished()
            collection_end_time = datetime.now()
            collection_duration = (collection_end_time - collection_start_time).total_seconds() / 60
            self.log(f"✓ Data collection completed ({collection_duration:.2f} minutes)")
            
            # Step 9: Run experiment to generate final results
            self.log("Processing data and calculating molecular weights...")
            self.admin.run_experiment(experiment_id)
            self.log("✓ Data processing completed")
            
            # Step 10: Save experiment file
            if self.export_experiment_file:
                experiment_filename = f"experiment_{timestamp}.aex"
                experiment_path = os.path.join(results_folder, experiment_filename)
                self.admin.save_experiment(experiment_id, experiment_path)
                
                if os.path.exists(experiment_path):
                    exp_size = os.path.getsize(experiment_path)
                    self.log(f"✓ Experiment saved: {exp_size:,} bytes")
                    exported_files.append(experiment_path)
            
            # Step 11: Export XML results and analyze molecular weights
            molecular_weights = None
            if self.export_xml_results:
                results_filename = f"results_{timestamp}.xml"
                results_path = os.path.join(results_folder, results_filename)
                
                self.admin.save_results(experiment_id, results_path)
                
                if os.path.exists(results_path):
                    results_size = os.path.getsize(results_path)
                    self.log(f"✓ XML results exported: {results_size:,} bytes")
                    exported_files.append(results_path)
                    
                    # Extract and display molecular weight data
                    if self.show_molecular_weights:
                        molecular_weights = self._extract_and_display_molecular_weights(results_path, results_folder, timestamp)
            
            # Step 12: Export CSV datasets  
            if self.export_csv_datasets:
                csv_files = self._export_csv_datasets(experiment_id, results_folder, timestamp)
                exported_files.extend(csv_files)
                self.log(f"✓ Exported {len(csv_files)} CSV dataset files")
            
            self.log("✅ All data collection and processing completed successfully")
            return {
                'success': True,
                'collection_duration_minutes': collection_duration,
                'exported_files': exported_files,
                'molecular_weights': molecular_weights
            }
            
        except Exception as e:
            self.log(f"❌ Data collection/processing failed: {e}")
            return {
                'success': False,
                'collection_duration_minutes': None,
                'exported_files': [],
                'molecular_weights': None
            }
    
    def close_experiment(self, experiment_id):
        """
        Close a specific experiment.
        
        Args:
            experiment_id: ID of the experiment to close
            
        Returns:
            bool: True if successful, False otherwise
        """
        try:
            if self.admin is not None:
                self.admin.close_experiment(experiment_id)
                self.log(f"✓ Experiment {experiment_id} closed")
                return True
            else:
                self.log("⚠ No admin connection available")
                return False
                
        except Exception as e:
            self.log(f"⚠ Warning closing experiment {experiment_id}: {e}")
            return False
    
    def cleanup_admin(self):
        """
        Clean up ASTRA admin connection and dispose resources.
        This should be called when done with all experiments.
        """
        try:
            self.log("=== Admin Cleanup ===")
            
            if self.admin is not None:
                try:
                    self.admin.dispose()
                    self.log("✓ ASTRA connection disposed")
                except Exception as e:
                    self.log(f"⚠ Warning disposing ASTRA: {e}")
            
            # Reset state
            self.admin = None
            self.client_id = None
            self.is_initialized = False
            
        except Exception as e:
            self.log(f"⚠ Warning during admin cleanup: {e}")
    
    def run_complete_workflow(self, astra_method_path, experiment_name=None):
        """
        Run the complete automation workflow in one call.
        
        This combines all phases for backwards compatibility with
        the original script behavior.
        
        Args:
            astra_method_path: Full path to ASTRA method template
            experiment_name: Optional name for the experiment
            
        Returns:
            dict: {
                'success': bool,
                'experiment_id': str or None,
                'results': dict or None  # Results from collect_and_process_data
            }
        """
        experiment_id = None
        try:
            # Phase 1: Prepare
            prep_result = self.prepare_experiment_for_collection(astra_method_path, experiment_name)
            if not prep_result['success']:
                return {'success': False, 'experiment_id': None, 'results': None}
            
            experiment_id = prep_result['experiment_id']
            
            # Phase 2: Wait for injection
            if not self.wait_for_injection_signal(experiment_id):
                return {'success': False, 'experiment_id': experiment_id, 'results': None}
            
            # Phase 3: Collect and process
            results = self.collect_and_process_data(
                experiment_id, 
                prep_result['results_folder'], 
                prep_result['timestamp']
            )
            if not results['success']:
                return {'success': False, 'experiment_id': experiment_id, 'results': results}
            
            self._print_final_summary(prep_result['results_folder'], prep_result['timestamp'])
            
            return {
                'success': True,
                'experiment_id': experiment_id,
                'results': results
            }
            
        except Exception as e:
            self.log(f"❌ Complete workflow failed: {e}")
            return {'success': False, 'experiment_id': experiment_id, 'results': None}
        
        finally:
            if experiment_id:
                self.close_experiment(experiment_id)
    
    def _extract_and_display_molecular_weights(self, results_path, results_folder, timestamp):
        """Extract molecular weight data from XML and display/save results"""
        try:
            with open(results_path, 'r', encoding='utf-8') as f:
                xml_content = f.read()
            
            self.log("🔬 Extracting molecular weight data...")
            peak_data = self._extract_peak_results(xml_content)
            
            if peak_data:
                self._display_and_save_results(peak_data, results_folder, timestamp)
                return peak_data
            else:
                self.log("⚠ No molecular weight data found in XML")
                return None
                
        except Exception as e:
            self.log(f"⚠ Warning: Could not extract molecular weights: {e}")
            return None
    
    def _export_csv_datasets(self, experiment_id, results_folder, timestamp):
        """Export CSV dataset files"""
        exported_files = []
        
        for dataset_name, description in self.dataset_exports:
            try:
                csv_filename = f"chromatogram_{dataset_name.replace(' ', '_')}_{timestamp}.csv"
                csv_path = os.path.join(results_folder, csv_filename)
                
                success = self.admin.save_data_set(experiment_id, dataset_name, csv_path)
                
                if success and os.path.exists(csv_path):
                    csv_size = os.path.getsize(csv_path)
                    self.log(f"  ✓ {description}: {csv_size:,} bytes")
                    exported_files.append(csv_path)
                else:
                    self.log(f"  ⚠ Failed to export '{dataset_name}'")
                    
            except Exception as e:
                self.log(f"  ✗ Error exporting '{dataset_name}': {e}")
        
        return exported_files
    
    def _extract_peak_results(self, xml_content):
        """Extract peak molecular weight results from ASTRA XML"""
        # [Copy the extract_peak_results function from original script]
        # This is a lengthy function - keeping the same implementation
        try:
            lines = xml_content.split('\n')
            peak_data = {}
            
            for i, line in enumerate(lines):
                line = line.strip()
                
                # Look for molar mass results
                if '<result type="molar mass">' in line:
                    if i + 1 < len(lines) and i + 2 < len(lines):
                        name_line = lines[i + 1].strip()
                        scalar_line = lines[i + 2].strip()
                        
                        # Extract name
                        name_match = re.search(r'<name>(.+?)</name>', name_line)
                        if name_match:
                            name = name_match.group(1)
                            
                            # Handle different XML attribute orders
                            scalar_match = re.search(r'<scalar.*?units="([^"]*)".*?uncertainty="([^"]*)".*?peak="(\d+)".*?>([^<]*)</scalar>', scalar_line)
                            
                            if not scalar_match:
                                scalar_match = re.search(r'<scalar.*?units="([^"]*)".*?peak="(\d+)".*?uncertainty="([^"]*)".*?>([^<]*)</scalar>', scalar_line)
                                if scalar_match:
                                    units = scalar_match.group(1)
                                    peak_num = int(scalar_match.group(2))
                                    uncertainty = float(scalar_match.group(3))
                                    value_str = scalar_match.group(4).strip()
                            else:
                                units = scalar_match.group(1)
                                uncertainty = float(scalar_match.group(2))
                                peak_num = int(scalar_match.group(3))
                                value_str = scalar_match.group(4).strip()
                            
                            if scalar_match and value_str != 'n/a':
                                value = float(value_str)
                                pct_uncertainty = (uncertainty / value) * 100
                                
                                if peak_num not in peak_data:
                                    peak_data[peak_num] = {}
                                
                                peak_data[peak_num][name] = {
                                    'value': value,
                                    'units': units,
                                    'uncertainty_pct': pct_uncertainty
                                }
                
                # Look for polydispersity (PDI)
                elif '<result type="polydispersity">' in line:
                    if i + 1 < len(lines) and i + 2 < len(lines):
                        name_line = lines[i + 1].strip()
                        if '<name>Mw/Mn</name>' in name_line:
                            scalar_line = lines[i + 2].strip()
                            scalar_match = re.search(r'<scalar.*?uncertainty="([^"]*)".*?peak="(\d+)".*?>([^<]*)</scalar>', scalar_line)
                            
                            if not scalar_match:
                                scalar_match = re.search(r'<scalar.*?peak="(\d+)".*?uncertainty="([^"]*)".*?>([^<]*)</scalar>', scalar_line)
                                if scalar_match:
                                    peak_num = int(scalar_match.group(1))
                                    uncertainty = float(scalar_match.group(2))
                                    value = float(scalar_match.group(3))
                            else:
                                uncertainty = float(scalar_match.group(1))
                                peak_num = int(scalar_match.group(2))
                                value = float(scalar_match.group(3))
                            
                            if scalar_match:
                                pct_uncertainty = (uncertainty / value) * 100
                                
                                if peak_num not in peak_data:
                                    peak_data[peak_num] = {}
                                
                                peak_data[peak_num]['Mw/Mn'] = {
                                    'value': value,
                                    'units': '',
                                    'uncertainty_pct': pct_uncertainty
                                }
                
                # Look for rms radius results  
                elif '<result type="rms radius">' in line:
                    if i + 1 < len(lines) and i + 2 < len(lines):
                        name_line = lines[i + 1].strip()
                        scalar_line = lines[i + 2].strip()
                        
                        name_match = re.search(r'<name>(.+?)</name>', name_line)
                        if name_match and name_match.group(1) == 'rz':
                            scalar_match = re.search(r'<scalar.*?units="([^"]*)".*?uncertainty="([^"]*)".*?peak="(\d+)".*?>([^<]*)</scalar>', scalar_line)
                            
                            if not scalar_match:
                                scalar_match = re.search(r'<scalar.*?units="([^"]*)".*?peak="(\d+)".*?uncertainty="([^"]*)".*?>([^<]*)</scalar>', scalar_line)
                                if scalar_match:
                                    units = scalar_match.group(1)
                                    peak_num = int(scalar_match.group(2))
                                    uncertainty = float(scalar_match.group(3))
                                    value_str = scalar_match.group(4).strip()
                            else:
                                units = scalar_match.group(1)
                                uncertainty = float(scalar_match.group(2))
                                peak_num = int(scalar_match.group(3))
                                value_str = scalar_match.group(4).strip()
                            
                            if scalar_match and value_str != 'n/a':
                                value = float(value_str)
                                pct_uncertainty = (uncertainty / value) * 100
                                
                                if peak_num not in peak_data:
                                    peak_data[peak_num] = {}
                                
                                peak_data[peak_num]['rz'] = {
                                    'value': value,
                                    'units': units,
                                    'uncertainty_pct': pct_uncertainty
                                }
            
            return peak_data
            
        except Exception as e:
            self.log(f"Error parsing XML: {e}")
            return {}
    
    def _format_value_with_uncertainty(self, value, units, uncertainty_pct, extra_precision=False):
        """Format value with uncertainty like ASTRA GUI"""
        if value >= 1000:
            formatted_value = f"{value:.3e}"
        else:
            formatted_value = f"{value:.1f}"
        
        # Use configurable precision
        precision = f".{self.pdi_decimal_places}f" if extra_precision else f".{self.mw_decimal_places}f"
        return f"{formatted_value} (±{uncertainty_pct:{precision}}%)"
    
    def _display_and_save_results(self, peak_data, results_folder, timestamp):
        """Display results in terminal and save summary to file"""
        summary_lines = []
        
        print("\n" + "="*50)
        print("🎯 MOLECULAR WEIGHT ANALYSIS RESULTS")
        print("="*50)
        
        for peak_num in sorted(peak_data.keys()):
            data = peak_data[peak_num]
            
            peak_header = f"🧪 Peak {peak_num} Results"
            print(peak_header)
            print()
            
            summary_lines.append(peak_header)
            summary_lines.append("")
            
            # Molecular weights  
            mw_header = "⚖️ Molecular Weights (g/mol)"
            print(mw_header)
            print()
            
            summary_lines.append(mw_header)
            summary_lines.append("")
            
            mw_fields = ['Mn', 'Mw', 'Mz']
            for field in mw_fields:
                if field in data:
                    mw = data[field]
                    formatted = self._format_value_with_uncertainty(mw['value'], mw['units'], mw['uncertainty_pct'])
                    line = f"  {field}: {formatted}"
                    print(line)
                    summary_lines.append(line)
            
            print()
            summary_lines.append("")
            
            # Polydispersity with extra precision
            if 'Mw/Mn' in data:
                pdi_header = "📈 Polydispersity"
                print(pdi_header)
                print()
                
                summary_lines.append(pdi_header)
                summary_lines.append("")
                
                pdi = data['Mw/Mn']
                formatted = self._format_value_with_uncertainty(pdi['value'], '', pdi['uncertainty_pct'], extra_precision=True)
                line = f"  Mw/Mn: {formatted}"
                print(line)
                print()
                
                summary_lines.append(line)
                summary_lines.append("")
            
            # RMS radius
            if 'rz' in data:
                radius_header = "🔵 RMS radius moments (nm)"
                print(radius_header)
                print()
                
                summary_lines.append(radius_header)
                summary_lines.append("")
                
                rz = data['rz']
                formatted = self._format_value_with_uncertainty(rz['value'], rz['units'], rz['uncertainty_pct'])
                line = f"  rz: {formatted}"
                print(line)
                print()
                
                summary_lines.append(line)
                summary_lines.append("")
        
        # Save summary to file
        if self.create_summary_file:
            summary_file = os.path.join(results_folder, "molecular_weight_summary.txt")
            try:
                with open(summary_file, 'w', encoding='utf-8') as f:
                    f.write('\n'.join(summary_lines))
                self.log(f"✓ Molecular weight summary saved to: {os.path.basename(summary_file)}")
            except Exception as e:
                self.log(f"⚠ Warning: Could not save summary file: {e}")
        
        print("="*50)
        print("✅ SUCCESS: Complete molecular weight analysis!")
        print("💾 All data saved to timestamped results folder")
        print("="*50)
    
    def _print_final_summary(self, results_folder, timestamp):
        """Print final success summary"""
        self.log("📁 All data saved to timestamped results folder:")
        self.log(f"   📂 Folder: {os.path.basename(results_folder)}")
        
        if self.export_xml_results:
            self.log(f"   📄 XML Results: results_{timestamp}.xml")
        if self.export_csv_datasets:
            self.log(f"   📊 CSV Datasets: chromatogram files")
        if self.create_summary_file and self.show_molecular_weights:
            self.log(f"   📝 Summary: molecular_weight_summary.txt")
        if self.export_experiment_file:
            self.log(f"   💾 Experiment: experiment_{timestamp}.aex")


# Example usage and backwards compatibility
def main():
    """Example usage of the GPCAutomation class"""
    
    # Configuration
    ASTRA_METHOD_PATH = r"//dbf/Method Builder/Owen/test_method_3"
    BASE_RESULTS_DIR = r"C:\Users\Administrator.WS\Desktop\wyatt-api\gpc-automation\results"
    
    # Create automation instance
    gpc = GPCAutomation(base_results_dir=BASE_RESULTS_DIR)
    
    try:
        # Run complete workflow (backwards compatible)
        result = gpc.run_complete_workflow(ASTRA_METHOD_PATH)
        
        if result['success']:
            print("\n" + "="*60)
            print("🎉 GPC AUTOMATION COMPLETE!")
            print("✅ Full workflow executed successfully")
            print("💾 All data saved in timestamped results folder")
            print("📊 Molecular weight analysis completed and displayed")
            print("="*60)
        else:
            print("\n" + "="*60)
            print("❌ AUTOMATION FAILED")
            print("Check the log messages above for details")
            print("="*60)
            
    finally:
        # Clean up admin connection
        gpc.cleanup_admin()


if __name__ == "__main__":
    main()