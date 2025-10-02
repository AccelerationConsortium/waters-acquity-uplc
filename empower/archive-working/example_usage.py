"""
Waters Empower Toolkit Python Usage Examples
============================================

This file demonstrates how to use the empower_toolkit.py wrapper
for interacting with Waters Empower systems via Python.
"""

from empower_toolkit import EmpowerToolkit, EmpowerProject, EmpowerInstrument


def basic_discovery_example():
    """Basic example: Discover available systems and methods"""
    print("=== Basic Discovery Example ===")
    
    try:
        # Using context manager for automatic cleanup
        with EmpowerToolkit() as toolkit:
            
            # Discover available systems
            discovery = toolkit.discover_systems()
            
            print("Available Systems:")
            for system in discovery['systems']:
                print(f"  - {system}")
            
            print("\nAvailable Nodes:")
            for node in discovery['nodes']:
                print(f"  - {node}")
            
            print("\nAvailable Sample Set Methods:")
            for method in discovery['methods']:
                print(f"  - {method}")
                
    except Exception as e:
        print(f"Discovery failed: {e}")


def connection_example():
    """Example: Connect to instrument and check status"""
    print("\n=== Connection Example ===")
    
    try:
        with EmpowerToolkit() as toolkit:
            
            # Try to connect to instrument
            print("Attempting instrument connection...")
            success = toolkit.connect_instrument()
            
            if success:
                print("✅ Connected successfully!")
                
                # Check instrument status
                print(f"Instrument Status: {toolkit.instrument.status}")
                print(f"Connection Status: {toolkit.instrument.is_connected}")
                
                # Get available methods when connected
                methods = toolkit.instrument.sample_set_methods
                print(f"Sample Set Methods: {methods}")
                
            else:
                print("❌ Connection failed")
                
    except Exception as e:
        print(f"Connection example failed: {e}")


def manual_object_example():
    """Example: Using individual objects manually"""
    print("\n=== Manual Object Example ===")
    
    project = EmpowerProject()
    instrument = EmpowerInstrument()
    
    try:
        # Manual setup
        project.create()
        instrument.create()
        
        # Login
        print("Logging in...")
        project.login(
            database="",
            project="Waters GPC Training",
            username="system",
            password="manager"
        )
        print("✅ Login successful")
        
        # Discover systems
        systems = instrument.systems
        nodes = instrument.acquisition_servers
        
        print(f"Found {len(systems)} systems and {len(nodes)} nodes")
        
        # Try connection if systems available
        if systems and nodes:
            print(f"Attempting connection: {nodes[0]} -> {systems[0]}")
            success = instrument.connect(nodes[0], systems[0])
            
            if success:
                print("✅ Manual connection successful!")
                print(f"Status: {instrument.status}")
            else:
                print("❌ Manual connection failed")
        
    except Exception as e:
        print(f"Manual example failed: {e}")
    
    finally:
        # Manual cleanup
        instrument.disconnect()
        project.logoff()


def sample_set_execution_example():
    """Example: Execute a sample set method"""
    print("\n=== Sample Set Execution Example ===")
    
    try:
        with EmpowerToolkit() as toolkit:
            
            # Connect first
            if toolkit.connect_instrument():
                print("Connected to instrument")
                
                # Get available methods
                methods = toolkit.instrument.sample_set_methods
                
                if methods:
                    method_name = methods[0]  # Use first available method
                    print(f"Executing method: {method_name}")
                    
                    try:
                        success = toolkit.instrument.replace_sample_set(method_name)
                        if success:
                            print("✅ Sample set execution started")
                            
                            # Monitor progress (basic example)
                            print("Monitoring progress...")
                            for i in range(5):  # Check status 5 times
                                status = toolkit.instrument.status
                                progress = toolkit.instrument.progress
                                print(f"  Status: {status}, Progress: {progress}")
                                
                                import time
                                time.sleep(2)  # Wait 2 seconds between checks
                                
                        else:
                            print("❌ Sample set execution failed")
                            
                    except Exception as e:
                        print(f"Execution error: {e}")
                        
                else:
                    print("No sample set methods available")
            else:
                print("Could not connect to instrument")
                
    except Exception as e:
        print(f"Sample set execution example failed: {e}")


def queue_operations_example():
    """Example: Queue operations"""
    print("\n=== Queue Operations Example ===")
    
    try:
        with EmpowerToolkit() as toolkit:
            
            if toolkit.connect_instrument():
                print("Connected to instrument")
                
                # Clear existing queue
                toolkit.instrument.clear_queue()
                print("Queue cleared")
                
                # Add sample sets to queue (example names)
                sample_sets = ["Test Method 1", "Test Method 2"]
                
                for sample_set in sample_sets:
                    try:
                        toolkit.instrument.queue_sample_set(sample_set)
                        print(f"Added to queue: {sample_set}")
                    except Exception as e:
                        print(f"Failed to add {sample_set}: {e}")
                
                # Start queue processing
                try:
                    toolkit.instrument.start_queue()
                    print("✅ Queue processing started")
                except Exception as e:
                    print(f"Failed to start queue: {e}")
                    
            else:
                print("Could not connect to instrument")
                
    except Exception as e:
        print(f"Queue operations example failed: {e}")


def error_handling_example():
    """Example: Proper error handling"""
    print("\n=== Error Handling Example ===")
    
    try:
        with EmpowerToolkit() as toolkit:
            
            # This might fail - demonstrate error handling
            try:
                # Try invalid connection
                toolkit.instrument.connect("InvalidNode", "InvalidSystem")
                
            except RuntimeError as e:
                print(f"Expected connection error: {e}")
                
                # Get detailed error description if available
                if hasattr(e, 'args') and len(e.args) > 0:
                    error_msg = str(e.args[0])
                    if "error code" in error_msg.lower():
                        # Extract error code and get description
                        try:
                            # This is just an example - actual implementation may vary
                            error_desc = toolkit.project.get_error_description(-1)
                            print(f"Error description: {error_desc}")
                        except Exception:
                            print("Could not get detailed error description")
            
            # Show proper connection attempt
            print("\nAttempting proper connection...")
            success = toolkit.connect_instrument()
            
            if success:
                print("✅ Proper connection successful")
            else:
                print("❌ Even proper connection failed (may need hardware)")
                
    except Exception as e:
        print(f"Error handling example failed: {e}")


if __name__ == "__main__":
    """Run all examples"""
    
    print("Waters Empower Toolkit Python Examples")
    print("=" * 50)
    
    # Run examples
    basic_discovery_example()
    connection_example()
    manual_object_example()
    sample_set_execution_example()
    queue_operations_example()
    error_handling_example()
    
    print("\n" + "=" * 50)
    print("Examples completed!")
    print("\nNote: Some operations may fail without actual Empower hardware.")
    print("The toolkit wrapper handles errors gracefully and provides detailed feedback.")
