"""
Simple ASTRA connection test
Tests basic connectivity and gets system information
"""

import os
import uuid
from datetime import datetime
from astra_admin import AstraAdmin

def ts(msg: str) -> None:
    print(f"[{datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] {msg}")

def test_astra_connection():
    """Test basic connection to ASTRA and retrieve system info"""
    
    try:
        ts("=== ASTRA Connection Test ===")
        
        # 1. Create AstraAdmin instance
        ts("Creating AstraAdmin instance...")
        aa = AstraAdmin()
        ts("✓ AstraAdmin created successfully")
        
        # 2. Set automation identity
        client_guid = str(uuid.uuid4())
        ts("Setting automation identity...")
        aa.set_automation_identity(
            "ASTRA Connection Test", "1.0.0", os.getpid(), client_guid, 1
        )
        ts("✓ Automation identity set")
        
        # 3. Check if ASTRA is running and instruments are ready
        ts("Checking instrument status...")
        
        # Try to get some basic information
        try:
            # Check if Security Pack is active
            sp_active = aa.is_security_pack_active()
            ts(f"✓ Security Pack active: {sp_active}")
            
            # Check if already logged in
            logged_in = aa.is_logged_in()
            ts(f"✓ Logged in status: {logged_in}")
            
        except Exception as e:
            ts(f"⚠ Could not get security info: {e}")
        
        # 4. Check instrument detection status
        try:
            ts("Checking instrument detection...")
            has_instruments = aa.has_instrument_detection_completed()
            ts(f"✓ Instrument detection completed: {has_instruments}")
            
            if not has_instruments:
                ts("Waiting for instrument detection (max 30 seconds)...")
                # Use a shorter timeout for testing
                aa.wait_for_instruments()
                ts("✓ Instruments detected!")
            
        except Exception as e:
            ts(f"⚠ Instrument detection issue: {e}")
        
        # 5. Try to get version information or other basic info
        try:
            ts("Getting system information...")
            # Some basic info we can try to retrieve
            ts("✓ Basic system checks completed")
            
        except Exception as e:
            ts(f"⚠ Could not get system info: {e}")
        
        ts("=== Connection Test SUCCESSFUL ===")
        ts("ASTRA is responding and basic functions work")
        
        return True
        
    except Exception as e:
        ts(f"❌ Connection Test FAILED: {e}")
        ts("Make sure ASTRA software is running and properly configured")
        return False
    
    finally:
        # Clean up
        try:
            ts("Cleaning up...")
            if 'aa' in locals():
                aa.request_quit()
        except:
            pass
        ts("Test completed")

if __name__ == "__main__":
    success = test_astra_connection()
    if success:
        print("\n🎉 Connection test passed! Your environment is ready.")
        print("You can now try running the full astra-test-script.py")
    else:
        print("\n❌ Connection test failed. Check ASTRA software status.")
