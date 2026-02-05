#!/usr/bin/env python3
"""
Process Existing Experiment - Extract Data from Saved Experiments

This script:
1. Takes a folder path containing an experiment file (.aex or .aex.afe8)
2. Opens the experiment and re-exports XML and CSV data
3. Extracts and displays molecular weight analysis
4. Saves summary to the same folder

Usage: python process_existing_experiment.py <folder_path>
"""

import os
import sys
import re
from datetime import datetime
from astra_admin import AstraAdmin

# Configuration from original script
DATASET_EXPORTS = [
    ("masses vs volume", "Mass chromatogram data"),
    ("rms radius vs volume", "RMS radius chromatogram data")
]

SHOW_MOLECULAR_WEIGHTS_IN_TERMINAL = True
PDI_DECIMAL_PLACES = 3
MW_DECIMAL_PLACES = 1

def log(message: str):
    """Log with timestamp"""
    timestamp = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
    print(f"[{timestamp}] {message}")

def find_experiment_file(folder_path):
    """Find the .aex or .aex.afe8 experiment file in the given folder"""
    if not os.path.exists(folder_path):
        raise ValueError(f"Folder does not exist: {folder_path}")
    
    # Look for both .aex and .aex.afe8 files
    aex_files = [f for f in os.listdir(folder_path) if f.endswith('.aex') or f.endswith('.aex.afe8')]
    
    if not aex_files:
        raise ValueError(f"No .aex or .aex.afe8 experiment files found in: {folder_path}")
    
    if len(aex_files) > 1:
        log(f"Found multiple experiment files, using: {aex_files[0]}")
    
    return os.path.join(folder_path, aex_files[0])

def extract_peak_results(xml_content):
    """Extract peak molecular weight results from ASTRA XML"""
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
        log(f"Error parsing XML: {e}")
        return {}

def format_value_with_uncertainty(value, units, uncertainty_pct, extra_precision=False):
    """Format value with uncertainty like ASTRA GUI"""
    if value >= 1000:
        formatted_value = f"{value:.3e}"
    else:
        formatted_value = f"{value:.1f}"
    
    # Use configurable precision
    precision = f".{PDI_DECIMAL_PLACES}f" if extra_precision else f".{MW_DECIMAL_PLACES}f"
    return f"{formatted_value} (±{uncertainty_pct:{precision}}%)"

def display_and_save_results(peak_data, results_folder):
    """Display results in terminal and save summary to file"""
    summary_lines = []
    
    print("\n" + "="*50)
    print("🎯 MOLECULAR WEIGHT ANALYSIS RESULTS")
    print("="*50)
    
    summary_lines.append("="*50)
    summary_lines.append("🎯 MOLECULAR WEIGHT ANALYSIS RESULTS")
    summary_lines.append("="*50)
    
    for peak_num in sorted(peak_data.keys()):
        data = peak_data[peak_num]
        
        peak_header = f"🔬 Peak {peak_num}"
        peak_divider = "-" * 30
        
        print(f"\n{peak_header}")
        print(peak_divider)
        
        summary_lines.append(f"\n{peak_header}")
        summary_lines.append(peak_divider)
        
        # Molar mass moments
        if any(key in data for key in ['Mn', 'Mw', 'Mp', 'Mz']):
            mass_header = "📊 Molar mass moments (g/mol)"
            print(mass_header)
            print()
            
            summary_lines.append(mass_header)
            summary_lines.append("")
            
            if 'Mn' in data:
                mn = data['Mn']
                formatted = format_value_with_uncertainty(mn['value'], mn['units'], mn['uncertainty_pct'])
                line = f"  Mn: {formatted}"
                print(line)
                summary_lines.append(line)
            
            if 'Mw' in data:
                mw = data['Mw']
                formatted = format_value_with_uncertainty(mw['value'], mw['units'], mw['uncertainty_pct'])
                line = f"  Mw: {formatted}"
                print(line)
                summary_lines.append(line)
                
            if 'Mp' in data:
                mp = data['Mp'] 
                formatted = format_value_with_uncertainty(mp['value'], mp['units'], mp['uncertainty_pct'])
                line = f"  Mp: {formatted}"
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
            formatted = format_value_with_uncertainty(pdi['value'], '', pdi['uncertainty_pct'], extra_precision=True)
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
            formatted = format_value_with_uncertainty(rz['value'], rz['units'], rz['uncertainty_pct'])
            line = f"  rz: {formatted}"
            print(line)
            print()
            
            summary_lines.append(line)
            summary_lines.append("")
    
    # Save summary to file
    summary_file = os.path.join(results_folder, "molecular_weight_summary.txt")
    try:
        with open(summary_file, 'w', encoding='utf-8') as f:
            f.write('\n'.join(summary_lines))
        log(f"✓ Molecular weight summary saved to: {os.path.basename(summary_file)}")
    except Exception as e:
        log(f"⚠ Warning: Could not save summary file: {e}")
    
    print("="*50)
    print("✅ SUCCESS: Complete molecular weight analysis!")
    print("💾 All data saved to folder")
    print("="*50)

def process_experiment(folder_path):
    """Process an existing experiment file in the given folder"""
    log(f"🔍 Processing experiment in folder: {folder_path}")
    
    # Find experiment file
    try:
        experiment_file = find_experiment_file(folder_path)
        log(f"📁 Found experiment file: {os.path.basename(experiment_file)}")
    except ValueError as e:
        log(f"❌ {e}")
        return False
    
    admin = None
    experiment_id = None
    timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
    
    try:
        # Initialize ASTRA
        log("🔧 Initializing ASTRA connection...")
        admin = AstraAdmin()
        
        # Wait for instruments (might be needed for ASTRA to be ready)
        log("⏳ Waiting for ASTRA to be ready...")
        try:
            admin.wait_for_instruments()
            log("✓ ASTRA is ready")
        except Exception as wait_error:
            log(f"⚠ Warning: Could not wait for instruments: {wait_error}")
            log("   Continuing anyway...")
        
        # Open the experiment
        log("📂 Opening experiment...")
        experiment_id = admin.open_experiment(experiment_file)
        
        if experiment_id <= 0:
            log(f"❌ Failed to open experiment file - ID: {experiment_id}")
            log("   This could mean the file is corrupted or ASTRA can't read it")
            return False
            
        log(f"✓ Experiment opened - ID: {experiment_id}")
        
        # Run the experiment to ensure all data is processed
        log("🔄 Running experiment to process molecular weight calculations...")
        run_success = admin.run_experiment(experiment_id)
        if not run_success:
            log("⚠ Warning: Experiment run may have failed, but continuing...")
        else:
            log("✓ Experiment processing completed")
        
        # Export NEW XML results ONLY
        log("📄 Exporting NEW XML results...")
        results_filename = f"results_xml_{timestamp}.xml"
        results_path = os.path.join(folder_path, results_filename)
        
        admin.save_results(experiment_id, results_path)
        
        if not os.path.exists(results_path):
            log("❌ Failed to create NEW XML results file")
            return False
        
        results_size = os.path.getsize(results_path)
        log(f"✓ NEW XML results exported: {results_size:,} bytes")
        
        # Extract and display molecular weight data from NEW XML only
        if SHOW_MOLECULAR_WEIGHTS_IN_TERMINAL:
            try:
                with open(results_path, 'r', encoding='utf-8') as f:
                    xml_content = f.read()
                
                log("🔬 Extracting molecular weight data from NEW XML...")
                peak_data = extract_peak_results(xml_content)
                
                if peak_data:
                    # Display results in terminal and save summary
                    display_and_save_results(peak_data, folder_path)
                else:
                    log("⚠ No molecular weight data found in NEW XML")
                    
            except Exception as extract_error:
                log(f"⚠ Warning: Could not extract molecular weights: {extract_error}")
        
        # Export NEW CSV datasets only
        log("📊 Exporting NEW CSV datasets...")
        exported_csv_count = 0
        
        for dataset_name, description in DATASET_EXPORTS:
            log(f"  Exporting NEW dataset: '{dataset_name}'")
            
            csv_filename = f"chromatogram_{dataset_name.replace(' ', '_')}_{timestamp}.csv"
            csv_path = os.path.join(folder_path, csv_filename)
            
            success = admin.save_data_set(experiment_id, dataset_name, csv_path)
            
            if success and os.path.exists(csv_path):
                csv_size = os.path.getsize(csv_path)
                log(f"    ✓ NEW {description}: {csv_size:,} bytes")
                exported_csv_count += 1
            else:
                log(f"    ❌ Failed to create NEW '{dataset_name}' - ABORTING")
                return False
        
        log(f"✓ Exported {exported_csv_count} NEW CSV dataset files")
        
        # Summary
        log("=== Processing Complete ===")
        log(f"📁 NEW results saved to: {folder_path}")
        log(f"📄 NEW XML Results: {results_filename}")
        log(f"📊 NEW CSV Datasets: {exported_csv_count} files")
        log(f"📝 NEW Summary: molecular_weight_summary.txt")
        
        return True
        
    except Exception as error:
        log(f"❌ Processing error: {error}")
        return False
    
    finally:
        # Clean up
        if experiment_id is not None and admin is not None:
            try:
                admin.close_experiment(experiment_id)
                log("✓ Experiment closed")
            except Exception as e:
                log(f"⚠ Warning closing experiment: {e}")
        
        if admin is not None:
            try:
                admin.dispose()
                log("✓ ASTRA connection disposed")
            except Exception as e:
                log(f"⚠ Warning disposing ASTRA: {e}")

def main():
    """Main entry point"""
    if len(sys.argv) != 2:
        print("Usage: python process_existing_experiment.py <folder_path>")
        print("Example: python process_existing_experiment.py C:\\path\\to\\experiment\\folder")
        return False
    
    folder_path = sys.argv[1]
    
    log("🚀 Process Existing Experiment - Data Extraction Tool")
    log(f"📁 Target folder: {folder_path}")
    
    success = process_experiment(folder_path)
    
    if success:
        print("\n" + "="*50)
        print("🎉 EXPERIMENT PROCESSING COMPLETE!")
        print("✅ XML and CSV data exported successfully")
        print("📊 Molecular weight analysis completed")
        print("📁 Check the folder for all exported files")
        print("="*50)
    else:
        print("\n" + "="*50)
        print("❌ PROCESSING FAILED")
        print("Check the log messages above for details")
        print("="*50)
    
    return success

if __name__ == "__main__":
    main()
