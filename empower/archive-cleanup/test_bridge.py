"""
Test script for the Empower Bridge - Uses working C# executables
"""

try:
    from empower_bridge import EmpowerToolkitBridge, run_empower_diagnostics
    
    print("Waters Empower Toolkit - Bridge Test")
    print("=" * 50)
    
    # Run comprehensive diagnostics
    print("Running diagnostics...")
    print()
    
    diagnostics = run_empower_diagnostics()
    print(diagnostics)
    
    print()
    print("Bridge test completed!")
    
except Exception as e:
    print(f"Bridge test failed: {e}")
    import traceback
    traceback.print_exc()
