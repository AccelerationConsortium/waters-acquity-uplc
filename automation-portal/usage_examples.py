#!/usr/bin/env python3
"""
Waters Automation Portal Python API - Usage Examples
===================================================

Demonstrates how to use the Waters Automation Portal Python API for sample handling automation.
"""

from automation_portal_driver import AutomationPortalDriver
import sys
import time

def test_connection():
    """Test connection to automation portal"""
    portal = AutomationPortalDriver()
    
    connected = portal.connect()
    print(f"Connection successful: {connected}")
    
    if connected:
        portal.disconnect()
    
    return connected

def check_status(verbose=False):
    """Check current automation portal status"""
    portal = AutomationPortalDriver()
    
    if not portal.connect():
        print("Failed to connect to automation portal")
        return None
    
    try:
        status = portal.get_status()
        
        if verbose:
            print("Portal Status:")
            for key, value in status.items():
                if key != 'success':
                    print(f"  {key}: {value}")
        else:
            print(f"System State: {status.get('system_state')}")
            print(f"Door Status: {status.get('door_status')}")
            print(f"Current Mode: {status.get('mode')}")
            print(f"Movement: {status.get('status')}")
        
        return status
        
    finally:
        portal.disconnect()

def initialize_portal():
    """Initialize the automation portal system"""
    portal = AutomationPortalDriver()
    
    if not portal.connect():
        print("Failed to connect to automation portal")
        return False
    
    try:
        print("Initializing portal... (may take up to 2 minutes)")
        success = portal.initialize()
        print(f"Initialization successful: {success}")
        return success
        
    finally:
        portal.disconnect()

def extract_sample(position=2):
    """Extract sample from specified position (1 or 2)"""
    portal = AutomationPortalDriver()
    
    if not portal.connect():
        print("Failed to connect to automation portal")
        return False
    
    try:
        # Check if system is operational first
        status = portal.get_status()
        if status.get('system_state') != 'OPERATIONAL':
            print("Portal not operational - initialize first")
            return False
        
        # Ensure system is ready before extract
        if not wait_for_ready(portal, timeout=30):
            print("❌ System not ready for extraction")
            return False
        
        # Map user position to driver position (1->1, 2->0)
        driver_position = 1 if position == 1 else 0
        
        print(f"Extracting sample from tray {position}...")
        success = portal.extract_drawer(driver_position)
        
        if success:
            print("✅ Extract command sent")
            # Wait for operation to complete
            if wait_for_ready(portal, timeout=90):
                print("✅ Extract operation completed")
            else:
                print("⚠️  Extract operation may still be in progress")
        else:
            print("❌ Extract command failed")
        
        return success
        
    finally:
        portal.disconnect()

def insert_sample(position=2):
    """Insert sample to specified position (1 or 2)"""
    portal = AutomationPortalDriver()
    
    if not portal.connect():
        print("Failed to connect to automation portal")
        return False
    
    try:
        # Check if system is operational first
        status = portal.get_status()
        if status.get('system_state') != 'OPERATIONAL':
            print("Portal not operational - initialize first")
            return False
        
        # Ensure system is ready before insert
        if not wait_for_ready(portal, timeout=30):
            print("❌ System not ready for insertion")
            return False
        
        # Map user position to driver position (1->1, 2->0)
        driver_position = 1 if position == 1 else 0
        
        print(f"Inserting sample to tray {position}...")
        success = portal.insert_drawer(driver_position)
        
        if success:
            print("✅ Insert command sent")
            # Wait for operation to complete
            if wait_for_ready(portal, timeout=90):
                print("✅ Insert operation completed")
            else:
                print("⚠️  Insert operation may still be in progress")
        else:
            print("❌ Insert command failed")
        
        return success
        
    finally:
        portal.disconnect()

def get_version():
    """Get portal version information"""
    portal = AutomationPortalDriver()
    
    if not portal.connect():
        print("Failed to connect to automation portal")
        return None
    
    try:
        version = portal.report_version()
        print(f"Portal Version: {version}")
        return version
        
    finally:
        portal.disconnect()

def wait_for_ready(portal, timeout=60, poll_interval=2):
    """
    Wait for the portal to be ready (Idle, DoorClosed, OPERATIONAL)
    
    Args:
        portal: Connected AutomationPortalDriver instance
        timeout: Maximum time to wait in seconds
        poll_interval: Time between status checks in seconds
    
    Returns:
        bool: True if ready, False if timeout
    """
    print("Waiting for system to be ready...", end="", flush=True)
    start_time = time.time()
    
    while time.time() - start_time < timeout:
        try:
            status = portal.get_status()
            
            # Check if system is ready
            if (status.get('system_state') == 'OPERATIONAL' and 
                status.get('status') == 'Idle' and
                status.get('door_status') == 'DoorClosed'):
                print(" Ready!")
                return True
            
            print(".", end="", flush=True)
            time.sleep(poll_interval)
            
        except Exception as e:
            print(f" Error checking status: {e}")
            time.sleep(poll_interval)
    
    print(f" Timeout after {timeout}s")
    return False

def sample_transfer_workflow():
    """Example workflow: extract sample, wait, insert back"""
    print("=== Sample Transfer Workflow ===")
    
    portal = AutomationPortalDriver()
    
    if not portal.connect():
        print("Failed to connect to automation portal")
        return False
    
    try:
        # Check initial status
        status = portal.get_status()
        print(f"Initial status: {status.get('system_state')}")
        
        if status.get('system_state') != 'OPERATIONAL':
            print("Initializing portal...")
            if not portal.initialize():
                print("Initialization failed")
                return False
            
            # Wait for initialization to complete
            if not wait_for_ready(portal, timeout=120):  # 2 minutes for init
                print("❌ System not ready after initialization")
                return False
        
        # Ensure system is ready before extract
        if not wait_for_ready(portal, timeout=30):
            print("❌ System not ready for extraction")
            return False
        
        # Extract from tray 2 (driver position 0)
        print("Extracting sample from tray 2...")
        if portal.extract_drawer(0):
            print("✅ Extract command sent")
            
            # Wait for extract operation to complete
            if not wait_for_ready(portal, timeout=90):  # Extract can take ~60s
                print("⚠️  Extract operation may still be in progress")
            else:
                print("✅ Extract operation completed")
            
            # Wait for user action (in real use, this would be sample processing)
            print("\nSample extracted. Process your sample, then press Enter to continue...")
            input()
            
            # Ensure system is ready before insert
            if not wait_for_ready(portal, timeout=30):
                print("❌ System not ready for insertion")
                return False
            
            # Insert back to tray 2 (driver position 0)
            print("Inserting sample back to tray 2...")
            if portal.insert_drawer(0):
                print("✅ Insert command sent")
                
                # Wait for insert operation to complete
                if not wait_for_ready(portal, timeout=90):  # Insert can take ~60s
                    print("⚠️  Insert operation may still be in progress")
                    return False
                else:
                    print("✅ Insert operation completed")
                    print("✅ Workflow completed successfully")
                    return True
            else:
                print("❌ Insert failed")
                return False
        else:
            print("❌ Extract failed")
            return False
            
    except Exception as e:
        print(f"Workflow error: {e}")
        return False
        
    finally:
        portal.disconnect()

def context_manager_example():
    """Example using context manager for automatic cleanup"""
    print("=== Context Manager Example ===")
    
    try:
        with AutomationPortalDriver() as portal:
            if not portal.connect():
                print("Connection failed")
                return False
            
            # Get status
            status = portal.get_status()
            print(f"Portal ready: {status.get('system_state') == 'OPERATIONAL'}")
            
            return True
            
    except Exception as e:
        print(f"Error: {e}")
        return False

def show_help():
    """Display help information for all commands"""
    print("Waters Automation Portal Python API - Command Line Interface")
    print("=" * 65)
    print()
    print("USAGE:")
    print("  python usage_examples.py <command> [options]")
    print()
    print("COMMANDS:")
    print()
    print("  connect")
    print("    Test connection to automation portal")
    print("    Example:")
    print("      python usage_examples.py connect")
    print()
    print("  status [-v]")
    print("    Check current portal status")
    print("    -v : Show verbose output with all status details")
    print("    Examples:")
    print("      python usage_examples.py status")
    print("      python usage_examples.py status -v")
    print()
    print("  initialize")
    print("    Initialize the automation portal system")
    print("    Example:")
    print("      python usage_examples.py initialize")
    print()
    print("  extract <position>")
    print("    Extract sample from tray (1 or 2)")
    print("    Tray 1 = Position 1, Tray 2 = Position 2")
    print("    Examples:")
    print("      python usage_examples.py extract 1")
    print("      python usage_examples.py extract 2")
    print()
    print("  insert <position>")
    print("    Insert sample to tray (1 or 2)")
    print("    Examples:")
    print("      python usage_examples.py insert 1")
    print("      python usage_examples.py insert 2")
    print()
    print("  version")
    print("    Get portal firmware version information")
    print("    Example:")
    print("      python usage_examples.py version")
    print()
    print("  workflow")
    print("    Run complete sample transfer workflow")
    print("    Example:")
    print("      python usage_examples.py workflow")
    print()
    print("  help")
    print("    Show this help message")
    print()
    print("TYPICAL WORKFLOW:")
    print("  1. python usage_examples.py connect     # Test connection")
    print("  2. python usage_examples.py status      # Check if ready")
    print("  3. python usage_examples.py initialize  # Initialize if needed")
    print("  4. python usage_examples.py extract 2   # Extract from tray 2")
    print("  5. # Process your sample manually")
    print("  6. python usage_examples.py insert 2    # Insert back to tray 2")
    print()
    print("For Python API usage, import functions:")
    print("  from usage_examples import extract_sample, insert_sample, check_status")


if __name__ == "__main__":
    if len(sys.argv) > 1:
        command = sys.argv[1]
        
        if command == "connect":
            test_connection()
        elif command == "status":
            verbose = len(sys.argv) > 2 and sys.argv[2] == "-v"
            check_status(verbose)
        elif command == "initialize":
            initialize_portal()
        elif command == "extract" and len(sys.argv) > 2:
            try:
                pos = int(sys.argv[2])
                if pos in [1, 2]:
                    extract_sample(pos)
                else:
                    print("Position must be 1 or 2")
            except ValueError:
                print("Position must be a number (1 or 2)")
        elif command == "insert" and len(sys.argv) > 2:
            try:
                pos = int(sys.argv[2])
                if pos in [1, 2]:
                    insert_sample(pos)
                else:
                    print("Position must be 1 or 2")
            except ValueError:
                print("Position must be a number (1 or 2)")
        elif command == "version":
            get_version()
        elif command == "workflow":
            sample_transfer_workflow()
        elif command == "help":
            show_help()
        else:
            print("Usage:")
            print("  python usage_examples.py connect")
            print("  python usage_examples.py status [-v]")
            print("  python usage_examples.py initialize")
            print("  python usage_examples.py extract <position>  # 1 or 2")
            print("  python usage_examples.py insert <position>   # 1 or 2")
            print("  python usage_examples.py version")
            print("  python usage_examples.py workflow")
            print("  python usage_examples.py help")
    else:
        show_help()
