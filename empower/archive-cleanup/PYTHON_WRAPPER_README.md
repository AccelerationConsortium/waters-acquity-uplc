# Waters Empower Toolkit Python Wrapper

A comprehensive Python interface for Waters Empower Toolkit COM objects, providing Pythonic access to Empower automation capabilities.

## Features

- **Project Management**: Login/logout, project discovery, error handling
- **Instrument Control**: System discovery, connection management, sample set execution
- **Sample Set Operations**: Method execution, queue management, status monitoring
- **Context Management**: Automatic cleanup and resource management
- **Configuration Support**: Load settings from `secrets.ini` file
- **Error Handling**: Comprehensive exception handling with detailed error messages

## Quick Start

### Basic Usage

```python
from empower_toolkit import EmpowerToolkit

# Using context manager (recommended)
with EmpowerToolkit() as toolkit:
    # Discover available systems
    discovery = toolkit.discover_systems()
    print(f"Found {len(discovery['systems'])} systems")
    
    # Connect to instrument
    if toolkit.connect_instrument():
        print("Connected successfully!")
        
        # Execute sample set
        methods = toolkit.instrument.sample_set_methods
        if methods:
            toolkit.instrument.replace_sample_set(methods[0])
```

### Quick Discovery

```python
from empower_toolkit import discover_empower_systems

# Quick system discovery
systems = discover_empower_systems()
print(f"Systems: {systems['systems']}")
print(f"Nodes: {systems['nodes']}")
```

## Configuration

Create a `secrets.ini` file:

```ini
username=system
password=manager
database=
project=Waters GPC Training
system=Arc HPLC
node=Waters-h4q6k34
```

## Classes and Methods

### EmpowerToolkit (Main Class)

- `initialize()` - Initialize COM objects and login
- `connect_instrument()` - Connect to instrument using config
- `discover_systems()` - Get available systems, nodes, and methods
- `cleanup()` - Cleanup all connections

### EmpowerProject

- `create()` - Create Project COM object
- `login(database, project, username, password)` - Login to Empower
- `logoff()` - Logout from Empower
- `get_error_description(error_code)` - Get detailed error messages
- Properties: `projects`, `services`, `is_connected`

### EmpowerInstrument

- `create()` - Create Instrument COM object
- `connect(node_name, system_name, timeout=30)` - Connect to instrument
- `disconnect()` - Disconnect from instrument
- `replace_sample_set(method_name)` - Execute sample set method
- `stop()`, `pause()`, `resume()` - Control operations
- Queue operations: `queue_sample_set()`, `start_queue()`, `stop_queue()`, `clear_queue()`
- Properties: `systems`, `acquisition_servers`, `sample_set_methods`, `status`, `progress`

### EmpowerSampleSetMethod

- `create()` - Create SampleSetMethod COM object
- `load(method_name)`, `save(method_name)`, `delete(method_name)` - Method management
- Property: `method_names` - Get available method names

## Examples

### System Discovery

```python
with EmpowerToolkit() as toolkit:
    discovery = toolkit.discover_systems()
    
    for system in discovery['systems']:
        print(f"System: {system}")
    
    for node in discovery['nodes']:
        print(f"Node: {node}")
```

### Manual Connection

```python
from empower_toolkit import EmpowerProject, EmpowerInstrument

project = EmpowerProject()
instrument = EmpowerInstrument()

try:
    project.create()
    instrument.create()
    
    project.login("", "Waters GPC Training", "system", "manager")
    success = instrument.connect("Waters-h4q6k34", "Arc HPLC")
    
    if success:
        print("Connected!")
        methods = instrument.sample_set_methods
        print(f"Available methods: {methods}")
        
finally:
    instrument.disconnect()
    project.logoff()
```

### Queue Management

```python
with EmpowerToolkit() as toolkit:
    if toolkit.connect_instrument():
        # Clear existing queue
        toolkit.instrument.clear_queue()
        
        # Add methods to queue
        for method in ["Method1", "Method2"]:
            toolkit.instrument.queue_sample_set(method)
        
        # Start processing
        toolkit.instrument.start_queue()
```

### Error Handling

```python
try:
    with EmpowerToolkit() as toolkit:
        toolkit.connect_instrument()
        
except RuntimeError as e:
    print(f"Connection failed: {e}")
    
    # Get detailed error description
    if "error code" in str(e):
        description = toolkit.project.get_error_description(-1)
        print(f"Details: {description}")
```

## Requirements

- Python 3.6+
- pywin32 (`pip install pywin32`)
- Waters Empower software installed and registered
- Valid Empower credentials and system access

## Installation Notes

1. **COM Registration**: Requires Waters Empower Toolkit to be installed and COM objects registered
2. **Permissions**: May require administrator privileges for COM object access
3. **Hardware**: Some operations require actual Empower hardware connections

## Error Codes

Common COM error codes:
- `-2147221164`: Class not registered (Empower not installed/registered)
- `-2147221021`: Operation unavailable (Service not running)
- Custom error codes can be resolved using `project.get_error_description()`

## Based on Official Documentation

This wrapper implements patterns from:
- Waters ToolkitHelp.chm documentation
- Official Waters instrument control examples
- Proven SystemDiscoveryExtractor.cs patterns

## Files

- `empower_toolkit.py` - Main wrapper classes
- `example_usage.py` - Comprehensive usage examples
- `simple_test.py` - Basic functionality test
- `secrets.ini` - Configuration file (create manually)

## Testing

Run basic tests:

```bash
python simple_test.py
```

Run comprehensive examples:

```bash
python example_usage.py
```

Note: Tests will show "Class not registered" errors if Empower software is not installed. This is expected behavior.
