# Waters Empower Python Automation

**Complete Python interface for Waters Empower chromatography automation**

Automate sample set creation, execution, and monitoring using a simple Python wrapper around the Waters Empower COM interface.

## Quick Start

```python
from waters_empower import WatersEmpower

# Initialize connection
empower = WatersEmpower()

# Check if instrument is ready
if empower.is_ready():
    # Create a new sample set
    empower.create_sample_set("my_experiment_20260123")
    
    # Execute the sample set
    status = empower.execute_sample_set("my_experiment_20260123")
    
    if status.get('execution_started'):
        print("Experiment running!")
```

## Installation

1. **Clone or download** this repository
2. **Configure credentials** in `secrets.ini`:
   ```ini
   [empower]
   username = your_username
   password = your_password
   project = your_project
   system = your_system
   node = your_node
   default_template = your_default_template
   ```
3. **Test the API**:
   ```bash
   # Check instrument status
   python usage_examples.py status
   
   # List available sample sets
   python usage_examples.py list
   
   # Run a test
   python usage_examples.py create "test_run"
   python usage_examples.py run "test_run"
   ```

## Python Wrapper API

### Core Methods

#### `is_ready() -> bool`
Check if instrument is ready for new experiments.
```python
ready = empower.is_ready()
# Returns: True if ready, False if busy/error
```

#### `get_detailed_status() -> dict`
Get comprehensive instrument status information.
```python
status = empower.get_detailed_status()
# Returns: {
#   "ready": True/False,
#   "state": "System Idle - Instrument Failure",
#   "current_vial": "1", 
#   "active_sample_set": "test_sample",
#   "raw_output": "...",
#   "error_output": "..."
# }

#### `list_sample_sets() -> List[str]`
Get all available sample sets in the current project.
```python
sample_sets = empower.list_sample_sets()
# Returns: ['sample1', 'sample2', 'sample3', ...]
```

#### `read_sample_set(name) -> dict`
Read details of a specific sample set.
```python
details = empower.read_sample_set("Python_API_Test")
# Returns: {"name": "...", "lines": [{"injection_volume": "20", ...}]}
```

#### `create_sample_set(name, template=None, **kwargs) -> bool`
Create a new sample set by copying an existing sample set template.

The method creates a new sample set by copying all parameters from an existing sample set (template) and optionally overriding specific parameters. If no template is specified, it uses the `default_template` from `secrets.ini`.

```python
success = empower.create_sample_set(
    "my_experiment",
    injection_volume="25.0",     # Override injection volume (µL)
    runtime="15"                 # Override runtime (minutes)
)
# Returns: True if created, False if failed
```

**Optional parameters** (all inherited from template if not specified):
- `template` - Specific sample set to copy from (a `default_template` from `secrets.ini` can be set)
- `injection_volume` - Injection volume in microliters (µL)
- `runtime` - Analysis runtime in minutes  
- `sample_name` - Custom sample name
- `vial_position` - Plate/vial position (e.g., "1:A,1")

**Template behavior**:
- Uses existing sample set as template (copies all settings)
- Override only the parameters you need to change
- All other settings (method, column, mobile phases, etc.) copied from template
- If `template=None`, uses `default_template` from `secrets.ini`

#### `execute_sample_set(name) -> dict`
Execute a sample set and return detailed status.
```python
status = empower.execute_sample_set("my_experiment")
# Returns: {
#   "return_code": 0,
#   "execution_started": True,
#   "status": "idle|busy|not_found",
#   "stdout": "...",
#   "stderr": "..."
# }
```

## Usage Patterns

### Simple Execution
```python
empower = WatersEmpower()

# Quick status check and run
if empower.is_ready():
    status = empower.execute_sample_set("Python_API_Test")
    if status.get('execution_started'):
        print("Running!")
```

### Create and Execute Workflow
```python
import datetime

# Generate unique name (no dashes!)
timestamp = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
name = f"experiment_{timestamp}"

# Create sample set
if empower.create_sample_set(name):
    # Execute immediately
    status = empower.execute_sample_set(name)
    print(f"Started: {status.get('execution_started', False)}")
```

## Important Notes

### Error Handling
The wrapper handles common error states:
- **Instrument busy**: `is_ready()` returns `False`
- **Sample not found**: `execute_sample_set()` returns `{"status": "not_found"}`
- **Instrument failure**: Detected in status output
- **Connection issues**: Handled gracefully with error codes

## Command Line Usage

The `usage_examples.py` script provides a command-line interface for all functionality:

### Check Instrument Status
```bash
# Basic status check
python usage_examples.py status
# Output: Instrument ready: True

# Detailed status with full diagnostics
python usage_examples.py status -v
# Output: 
# Instrument ready: True
# Detailed status:
#   state: System Idle - Ready  
#   current_vial: 0
#   active_sample_set: default_template
# Raw output: [full debug information]
```

### List All Sample Sets
```bash
python usage_examples.py list
# Output:
# Found 77 sample sets:
#   1. 20250819_NKG_HCW_samples
#   2. Python_API_Test
#   ...
```

### Inspect Sample Set Configuration
```bash
python usage_examples.py inspect "Python_API_Test"
# Output:
# Sample set 'Python_API_Test':
#   Sample lines: 1
#   Sample 1:
#     SampleName: KC_Test_A4
#     Vial: 1:A,4
#     Runtime: 10.00
#     Function: Inject Samples
#     InjVol: 25.0
#     SampleWeight: 1.0000
#     Dilution: 1.0000
```

### Create New Sample Set
```bash
python usage_examples.py create "my_test_run"
# Output: Created sample set 'my_test_run': True
```

### Execute Sample Set
```bash
python usage_examples.py run "my_test_run"
# Output: Sample set 'my_test_run' execution started: True
```

### Import Functions for Custom Scripts
```python
from usage_examples import test_instrument_status, create_test_sample_set, run_sample_set

# Use individual functions in your scripts
if test_instrument_status():
    create_test_sample_set("batch_001")
    run_sample_set("batch_001")
```

## Testing

Run the test suite to verify functionality:
```bash
python test_empower.py
```

Tests include:
- Instrument status checking
- Sample set listing and reading
- Sample set creation with unique names
- Execution with status monitoring
- Post-execution status validation

## Architecture

### Components
- **`waters_empower.py`** - Main Python wrapper class
- **`SampleSetExtractor.exe`** - C# executable for sample set execution and status (pre-compiled)
- **`SampleSetCreator.exe`** - C# executable for sample set creation (pre-compiled)
- **`SampleSetReader.exe`** - C# executable for sample set enumeration and reading (pre-compiled)
- **`secrets.ini`** - Configuration file with credentials


### How It Works
The Python wrapper uses subprocess calls to communicate with compiled C# executables that interface directly with the Waters Empower COM objects. This approach provides:
- **Reliability** - Stable COM interface through native C#
- **Simplicity** - Easy Python API for automation
- **Error handling** - Graceful failure modes
- **Cross-process isolation** - COM cleanup handled per call

## Requirements

- **Windows** with .NET Framework 4.0+ (32-bit or 64-bit)
- **Waters Empower Personal 7.0+**
- **Python 3.6+** (for wrapper)
- **Valid Empower credentials** in `secrets.ini`

**Hardware Compatibility**: The pre-compiled executables are 32-bit and should work on most Windows systems. If you encounter compatibility issues with different hardware or Waters instrument configurations, you may need to recompile the C# source files on your target system.

## C# Compilation Instructions

**CRITICAL**: For COM compatibility with Waters Empower, you MUST compile with the `/platform:x86` flag to ensure 32-bit executables:

```cmd
# Compile all three C# executables with x86 platform targeting
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /platform:x86 /out:SampleSetCreator.exe SampleSetCreator.cs
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /platform:x86 /out:SampleSetReader.exe SampleSetReader.cs  
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /platform:x86 /out:SampleSetExtractor.exe SampleSetExtractor.cs
```

**What Works**: 
- .NET Framework v4.0.30319 with `/platform:x86` flag ✅
- Empower COM objects accessible through MillenniumToolkit.Project ✅
- Single vial position format "1:A,2" correctly parsed ✅

**What Doesn't Work**:
- Default compilation without platform flag (results in COM registration errors)
- 64-bit compilation with `/platform:x64` 
- Older .NET Framework versions (v2.0, v3.5) due to `var` keyword usage


---

**Last Updated:** January 23, 2026  
