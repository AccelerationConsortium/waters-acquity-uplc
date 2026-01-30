#!/usr/bin/env python3
"""
Waters Automation Portal Workflow
"""

import sys
from pathlib import Path

sys.path.append(str(Path(__file__).parent / "automation-portal"))

from automation_portal_driver import AutomationPortalDriver

def main():
    """Main workflow function"""
    print("Waters Automation Portal Workflow")
    print("Simple insert/extract operations with automated status checking")
    
    driver = AutomationPortalDriver()
    
    try:
        # Connect to the system
        print("\nConnecting to automation portal...")
        if not driver.connect():
            print("❌ Failed to connect to automation portal")
            return False
        print("✅ Connected successfully")
        
        # Simple one-liner operations
        print("\nInserting to tray 1...")
        if not driver.insert_drawer(1):
            print("❌ Insert operation failed")
            return False
        print("✅ Insert completed successfully")
        
        print("\nExtracting from tray 1...")
        if not driver.extract_drawer(1):
            print("❌ Extract operation failed")
            return False
        print("✅ Extract completed successfully")
        
        print("\n🎉 WORKFLOW COMPLETED SUCCESSFULLY")
        return True
        
    except Exception as e:
        print(f"❌ Workflow error: {e}")
        return False
    
    finally:
        driver.disconnect()
        print("\nDisconnected from automation portal")

if __name__ == "__main__":
    success = main()
    sys.exit(0 if success else 1)
