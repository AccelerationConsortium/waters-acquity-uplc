"""
Quick test script for Waters Empower Toolkit Python wrapper
"""

# Global variables for imported modules
EmpowerToolkit = None
discover_empower_systems = None

def test_basic_import():
    """Test basic import functionality"""
    try:
        from empower_toolkit import EmpowerToolkit, discover_empower_systems
        print("✅ Import successful")
        # Store globally for other tests
        global EmpowerToolkit, discover_empower_systems
        return True
    except ImportError as e:
        print(f"❌ Import failed: {e}")
        return False
    except Exception as e:
        print(f"❌ Unexpected error during import: {e}")
        return False


def test_discovery():
    """Test system discovery"""
    try:
        print("Testing system discovery...")
        discovery = discover_empower_systems()
        
        print(f"Systems found: {len(discovery.get('systems', []))}")
        print(f"Nodes found: {len(discovery.get('nodes', []))}")
        print(f"Methods found: {len(discovery.get('methods', []))}")
        
        if discovery['systems']:
            print("Available systems:")
            for system in discovery['systems']:
                print(f"  - {system}")
        
        return True
        
    except Exception as e:
        print(f"❌ Discovery test failed: {e}")
        return False


def test_toolkit_creation():
    """Test toolkit object creation"""
    try:
        print("Testing toolkit creation...")
        toolkit = EmpowerToolkit()
        
        # Test config loading
        config = toolkit.config
        print(f"Config loaded with {len(config)} settings")
        
        return True
        
    except Exception as e:
        print(f"❌ Toolkit creation failed: {e}")
        return False


if __name__ == "__main__":
    print("Waters Empower Toolkit - Quick Test")
    print("=" * 40)
    
    tests = [
        ("Import Test", test_basic_import),
        ("Toolkit Creation", test_toolkit_creation),
        ("System Discovery", test_discovery)
    ]
    
    results = []
    
    for test_name, test_func in tests:
        print(f"\n{test_name}:")
        success = test_func()
        results.append((test_name, success))
    
    print("\n" + "=" * 40)
    print("Test Results:")
    
    for test_name, success in results:
        status = "✅ PASS" if success else "❌ FAIL"
        print(f"{test_name}: {status}")
    
    total_tests = len(results)
    passed_tests = sum(1 for _, success in results if success)
    
    print(f"\nSummary: {passed_tests}/{total_tests} tests passed")
    
    if passed_tests == total_tests:
        print("🎉 All tests passed! Wrapper is ready to use.")
    else:
        print("⚠ Some tests failed. Check error messages above.")
