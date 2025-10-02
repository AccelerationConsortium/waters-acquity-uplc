"""
Simple test for Waters Empower Toolkit Python wrapper
"""

try:
    from empower_toolkit import EmpowerToolkit, discover_empower_systems
    print("✅ Import successful")
    
    print("\nTesting toolkit creation...")
    toolkit = EmpowerToolkit()
    print(f"✅ Toolkit created with {len(toolkit.config)} config settings")
    
    print("\nTesting system discovery...")
    discovery = discover_empower_systems()
    print(f"✅ Discovery completed:")
    print(f"  - Systems: {len(discovery.get('systems', []))}")
    print(f"  - Nodes: {len(discovery.get('nodes', []))}")
    print(f"  - Methods: {len(discovery.get('methods', []))}")
    
    if discovery.get('systems'):
        print("\nAvailable systems:")
        for system in discovery['systems']:
            print(f"  - {system}")
    
    if discovery.get('nodes'):
        print("\nAvailable nodes:")
        for node in discovery['nodes']:
            print(f"  - {node}")
    
    print("\n🎉 All basic tests passed!")
    
except Exception as e:
    print(f"❌ Test failed: {e}")
    import traceback
    traceback.print_exc()
