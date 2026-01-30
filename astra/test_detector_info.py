"""
Test to probe ASTRA for detector and instrument information
This explores what information we can get without running experiments
"""

import os
import uuid
from datetime import datetime
from astra_admin import AstraAdmin

def ts(msg: str) -> None:
    print(f"[{datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] {msg}")

def test_detector_info():
    """Test what detector/instrument information we can retrieve"""
    
    try:
        ts("=== ASTRA Detector/Instrument Information Test ===")
        
        # Connect to ASTRA
        aa = AstraAdmin()
        client_guid = str(uuid.uuid4())
        aa.set_automation_identity("Detector Info Test", "1.0.0", os.getpid(), client_guid, 1)
        
        # Wait for instruments
        if not aa.has_instrument_detection_completed():
            ts("Waiting for instrument detection...")
            aa.wait_for_instruments()
        
        ts("✓ Connected to ASTRA")
        
        # Try to get instrument/detector information
        ts("\n--- Checking Available Information ---")
        
        # Check what methods are available on the AstraAdmin object
        ts("Available AstraAdmin methods:")
        aa_methods = [method for method in dir(aa) if not method.startswith('_')]
        instrument_related = []
        detector_related = []
        info_related = []
        
        for method in aa_methods:
            method_lower = method.lower()
            if any(word in method_lower for word in ['instrument', 'device', 'detector']):
                instrument_related.append(method)
            elif any(word in method_lower for word in ['info', 'get', 'status', 'version', 'name']):
                info_related.append(method)
        
        if instrument_related:
            ts("Instrument/Detector related methods:")
            for method in sorted(instrument_related):
                ts(f"  - {method}")
        
        if info_related:
            ts("Information/Status related methods:")
            for method in sorted(info_related):
                ts(f"  - {method}")
        
        # Try some specific information gathering
        ts("\n--- Attempting to Get System Information ---")
        
        # Try specific information gathering methods that exist
        ts("\n--- System Information Available ---")
        
        # Test information methods we know exist
        info_tests = [
            ('astra_version', 'ASTRA Version'),
            ('is_security_pack_active', 'Security Pack Status'),
            ('is_logged_in', 'Login Status'),
            ('has_instrument_detection_completed', 'Instrument Detection'),
            ('get_active_user', 'Active User Info'),
            ('get_experiment_templates', 'Available Templates'),
        ]
        
        for method_name, description in info_tests:
            try:
                if hasattr(aa, method_name):
                    result = getattr(aa, method_name)()
                    ts(f"✓ {description}: {result}")
                else:
                    ts(f"⚠ {method_name} not available")
            except Exception as e:
                ts(f"⚠ Could not get {description}: {e}")
        
        # Try to get data database directory information
        try:
            ts("\n--- Data Directory Information ---")
            # Try common ASTRA data paths
            test_paths = [
                r"C:\ASTRA\Data",
                r"C:\Users\Public\Documents\ASTRA\Data",
                r"C:\ProgramData\Wyatt Technology\ASTRA\Data"
            ]
            
            for test_path in test_paths:
                if os.path.exists(test_path):
                    try:
                        data_dirs = aa.get_data_database_directory(test_path)
                        ts(f"✓ Data directory {test_path}: {len(data_dirs) if data_dirs else 0} items")
                        if data_dirs and len(data_dirs) < 10:  # Don't spam if too many
                            for item in data_dirs[:5]:
                                ts(f"    - {item}")
                    except Exception as e:
                        ts(f"⚠ Could not read {test_path}: {e}")
        except Exception as e:
            ts(f"⚠ Data directory check failed: {e}")
        
        # Try to get more detailed information if methods exist
        ts("\n--- Attempting Advanced Information Queries ---")
        
        # Look for methods that might give us detector/instrument details
        advanced_methods = [
            'get_instrument_info',
            'get_detector_info', 
            'get_device_info',
            'get_system_info',
            'get_version',
            'get_instrument_list',
            'get_detector_list'
        ]
        
        for method_name in advanced_methods:
            try:
                if hasattr(aa, method_name):
                    ts(f"Found method: {method_name}")
                    # Try to call it (carefully)
                    try:
                        result = getattr(aa, method_name)()
                        ts(f"  Result: {result}")
                    except Exception as e:
                        ts(f"  Could not call: {e}")
                else:
                    ts(f"Method not found: {method_name}")
            except Exception as e:
                ts(f"Error checking {method_name}: {e}")
        
        # Check if we can get information about available experiments or templates
        ts("\n--- Template and Method Information ---")
        
        try:
            templates = aa.get_experiment_templates()
            ts(f"✓ Found {len(templates) if templates else 0} experiment templates")
            if templates:
                for i, template in enumerate(templates[:5]):  # Show first 5
                    ts(f"    Template {i+1}: {template}")
        except Exception as e:
            ts(f"⚠ Could not get templates: {e}")
        
        # Try to get system capabilities without opening an experiment
        ts("\n--- System Capabilities (No Experiment Needed) ---")
        
        # These are methods we can call without an active experiment
        system_methods = [
            'get_active_user',
            'get_experiment_templates', 
        ]
        
        for method_name in system_methods:
            try:
                if hasattr(aa, method_name):
                    result = getattr(aa, method_name)()
                    ts(f"✓ {method_name}: {result}")
            except Exception as e:
                ts(f"⚠ {method_name} failed: {e}")
        
        ts("\n=== Information Gathering Complete ===")
        return True
        
    except Exception as e:
        ts(f"❌ Detector info test failed: {e}")
        return False
    
    finally:
        try:
            if 'aa' in locals():
                aa.request_quit()
        except:
            pass

def test_com_object_info():
    """Try to get information directly from the COM object"""
    
    try:
        ts("\n=== COM Object Information Test ===")
        
        aa = AstraAdmin()
        client_guid = str(uuid.uuid4())
        aa.set_automation_identity("COM Info Test", "1.0.0", os.getpid(), client_guid, 1)
        
        # Try to access the underlying COM object
        if hasattr(aa, 'astra_com'):
            ts("Found astra_com object")
            try:
                # Try to get COM object methods/properties
                com_obj = aa.astra_com
                ts(f"COM object type: {type(com_obj)}")
                
                # Some common COM properties we might try
                com_properties = ['Version', 'Name', 'Application', 'Status']
                for prop in com_properties:
                    try:
                        if hasattr(com_obj, prop):
                            value = getattr(com_obj, prop)
                            ts(f"  {prop}: {value}")
                    except Exception as e:
                        ts(f"  Could not get {prop}: {e}")
                        
            except Exception as e:
                ts(f"Could not access COM object: {e}")
        else:
            ts("No direct COM object access found")
            
    except Exception as e:
        ts(f"COM object test failed: {e}")
    
    finally:
        try:
            if 'aa' in locals():
                aa.request_quit()
        except:
            pass

if __name__ == "__main__":
    success1 = test_detector_info()
    test_com_object_info()
    
    print("\n" + "="*50)
    if success1:
        print("✓ Successfully connected and explored available information")
        print("The tests show what kind of data ASTRA exposes via the API")
    else:
        print("❌ Could not gather detector information")
    print("="*50)
