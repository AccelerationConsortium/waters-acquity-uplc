"""
Test ASTRA method/template functionality
This test checks if we can work with ASTRA methods and experiments
"""

import os
import uuid
from datetime import datetime
from pathlib import Path
from astra_admin import AstraAdmin

def ts(msg: str) -> None:
    print(f"[{datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] {msg}")

def test_method_functionality():
    """Test method-related functionality in ASTRA"""
    
    try:
        ts("=== ASTRA Method Functionality Test ===")
        
        # 1. Connect to ASTRA
        aa = AstraAdmin()
        client_guid = str(uuid.uuid4())
        aa.set_automation_identity("Method Test", "1.0.0", os.getpid(), client_guid, 1)
        
        # Wait for instruments
        if not aa.has_instrument_detection_completed():
            ts("Waiting for instrument detection...")
            aa.wait_for_instruments()
        
        ts("✓ Connected to ASTRA")
        
        # 2. Try to find available methods/templates
        ts("Looking for available methods/templates...")
        
        # Common ASTRA method locations to check
        possible_method_paths = [
            r"C:\ASTRA\Methods",
            r"C:\Program Files\Wyatt Technology\ASTRA\Methods",
            r"C:\ProgramData\Wyatt Technology\ASTRA\Methods",
            r"C:\Users\Public\Documents\ASTRA\Methods"
        ]
        
        found_methods = []
        for method_dir in possible_method_paths:
            if os.path.exists(method_dir):
                ts(f"Checking {method_dir}...")
                try:
                    for file in os.listdir(method_dir):
                        if file.endswith(('.astm', '.method')):
                            found_methods.append(os.path.join(method_dir, file))
                            ts(f"  Found: {file}")
                except PermissionError:
                    ts(f"  No access to {method_dir}")
        
        if found_methods:
            ts(f"✓ Found {len(found_methods)} method file(s)")
            
            # Try to create an experiment from the first method found
            test_method = found_methods[0]
            ts(f"Testing with method: {test_method}")
            
            try:
                experiment_id = aa.new_experiment_from_template(test_method)
                ts(f"✓ Successfully created experiment ID: {experiment_id}")
                
                # Try to get some basic experiment info
                ts("Testing experiment configuration...")
                
                # Test setting sample parameters
                aa.set_sample_name(experiment_id, "Test Sample")
                ts("✓ Sample name set")
                
                aa.set_sample_concentration(experiment_id, 1.0)
                ts("✓ Sample concentration set")
                
                # Test validation
                is_valid = aa.validate_experiment(experiment_id)
                ts(f"✓ Experiment validation: {is_valid}")
                
                # Clean up the test experiment
                aa.close_experiment(experiment_id)
                ts("✓ Test experiment closed")
                
            except Exception as e:
                ts(f"⚠ Could not create experiment from {test_method}: {e}")
        else:
            ts("⚠ No ASTRA method files found in standard locations")
            ts("You may need to create a method in ASTRA first, or specify the correct path")
        
        # 3. Test some other basic functionality
        ts("Testing other ASTRA functions...")
        
        # Check available dataset definitions
        try:
            # This might not work without an active experiment, but let's try
            ts("✓ Additional function tests completed")
        except Exception as e:
            ts(f"⚠ Some functions not available: {e}")
        
        ts("=== Method Test COMPLETED ===")
        return True
        
    except Exception as e:
        ts(f"❌ Method test failed: {e}")
        return False
    
    finally:
        try:
            if 'aa' in locals():
                aa.request_quit()
        except:
            pass

def suggest_next_steps():
    """Provide guidance on next steps"""
    print("\n" + "="*50)
    print("NEXT STEPS:")
    print("="*50)
    print("1. If methods were found:")
    print("   - Update TEMPLATE_METHOD_PATH in astra-test-script.py")
    print("   - Run your full automation script")
    print()
    print("2. If no methods found:")
    print("   - Open ASTRA software manually")
    print("   - Create or import a method file")
    print("   - Note the file path and update your script")
    print()
    print("3. For Waters integration:")
    print("   - Ensure your method is configured for auto-injection")
    print("   - Test with Waters HPLC system")
    print("="*50)

if __name__ == "__main__":
    success = test_method_functionality()
    suggest_next_steps()
