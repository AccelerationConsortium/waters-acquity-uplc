#!/usr/bin/env python3
"""
Waters Empower Python API - Usage Examples
==========================================

Demonstrates how to use the Waters Empower Python API for chromatography automation.
"""

from waters_empower import WatersEmpower
import datetime
import sys

def test_instrument_status(verbose=False):
    """Check if the instrument is ready for operation"""
    empower = WatersEmpower()
    
    if verbose:
        status = empower.get_detailed_status()
        print(f"Instrument ready: {status['ready']}")
        print("Detailed status:")
        for key, value in status.items():
            if key not in ['ready', 'raw_output', 'error_output']:
                print(f"  {key}: {value}")
        
        if status.get('raw_output'):
            print("\nRaw output:")
            print(status['raw_output'])
            
        return status['ready']
    else:
        ready = empower.is_ready()
        print(f"Instrument ready: {ready}")
        return ready

def list_available_sample_sets():
    """Get all sample sets available in Empower"""
    empower = WatersEmpower()
    sample_sets = empower.list_sample_sets()
    print(f"Found {len(sample_sets)} sample sets:")
    for i, name in enumerate(sample_sets, 1):
        print(f"  {i}. {name}")
    return sample_sets

def inspect_sample_set(sample_set_name):
    """Read and display configuration of a specific sample set"""
    empower = WatersEmpower()
    try:
        details = empower.read_sample_set(sample_set_name)
        if not details:
            print(f"Sample set '{sample_set_name}' not found or error reading")
            return None
        
        # Filter out debug/status lines - only show actual sample data
        sample_lines = []
        for line in details['lines']:
            # Skip lines that are just debug output (contain status messages)
            if any(key in line for key in ['? EXACT MATCH', '?? READING', 'Setting sample set', 'Total Sample Lines']):
                continue
            # Only include lines with actual sample data
            if 'SampleName' in line or 'Vial' in line:
                sample_lines.append(line)
        
        print(f"Sample set '{sample_set_name}':")
        print(f"  Sample lines: {len(sample_lines)}")
        
        for i, line in enumerate(sample_lines, 1):
            print(f"\n  Sample {i}:")
            for key, value in line.items():
                print(f"    {key}: {value}")
                
        return details
    except Exception as e:
        print(f"Error reading sample set: {e}")
        return None

def create_test_sample_set(name=None, volume=None, template=None, runtime=None, 
                          vials=None, sample_names=None, sample_weight=None, dilution=None):
    """Create a new sample set for testing"""
    empower = WatersEmpower()
    if name is None:
        timestamp = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
        name = f"test_{timestamp}"
    
    # Build parameters - only include injection_volume if explicitly provided
    params = {}
    if volume:
        params['injection_volume'] = float(volume)
    if template:
        params['template'] = template
    if runtime:
        params['runtime'] = float(runtime)
    if vials:
        params['vials'] = vials
    if sample_names:
        params['sample_names'] = sample_names
    if sample_weight:
        params['sample_weight'] = float(sample_weight)
    if dilution:
        params['dilution'] = float(dilution)
    
    print(f"Creating sample set '{name}' with parameters:")
    for key, value in params.items():
        print(f"  {key}: {value}")
    if not volume:
        print("  injection_volume: (using template default)")
    
    created = empower.create_sample_set(name, **params)
    print(f"Created sample set '{name}': {created}")
    return created

def run_sample_set(sample_set_name):
    """Execute a sample set if instrument is ready"""
    empower = WatersEmpower()
    
    # Check if ready first
    if not empower.is_ready():
        print("Instrument not ready - cannot execute sample set")
        return False
    
    try:
        status = empower.execute_sample_set(sample_set_name)
        started = status.get('execution_started', False)
        print(f"Sample set '{sample_set_name}' execution started: {started}")
        return started
    except Exception as e:
        print(f"Error executing sample set: {e}")
        return False



def show_help():
    """Display detailed help information for all commands"""
    print("Waters Empower Python API - Command Line Interface")
    print("=" * 55)
    print()
    print("USAGE:")
    print("  python usage_examples.py <command> [options]")
    print()
    print("COMMANDS:")
    print()
    print("  status [-v]")
    print("    Check if instrument is ready for operation")
    print("    -v : Show verbose output with detailed diagnostics")
    print("    Examples:")
    print("      python usage_examples.py status")
    print("      python usage_examples.py status -v")
    print()
    print("  list")
    print("    Display all available sample sets in the current project")
    print("    Example:")
    print("      python usage_examples.py list")
    print()
    print("  inspect <sample_set_name>")
    print("    Show detailed configuration of a specific sample set")
    print("    Displays: sample lines, vial positions, injection volumes, runtimes")
    print("    Example:")
    print("      python usage_examples.py inspect \"Python_API_Test\"")
    print()
    print("  create <sample_set_name> [options]")
    print("    Create a new sample set with optional parameters")
    print("    Options:")
    print("      --volume=<µL>           Injection volume (uses template default if not specified)")
    print("      --template=<name>       Template to use (e.g., 20251002_KC)")
    print("      --runtime=<minutes>     Runtime in minutes")
    print("      --vials=<position>      Vial position (e.g., \"1:A,2\" for tray 1, row A, column 2)")
    print("      --sample-names=<name>   Sample name(s)")
    print("      --sample-weight=<mg>    Sample weight in milligrams")
    print("      --dilution=<factor>     Dilution factor")
    print("    Examples:")
    print("      python usage_examples.py create \"my_experiment\"")
    print("      python usage_examples.py create \"2026_02_02_KC\" --volume=10.0 --template=20251002_KC --runtime=10.0")
    print()
    print("  run <sample_set_name>")
    print("    Execute a sample set (only if instrument is ready)")
    print("    Automatically checks instrument status before execution")
    print("    Example:")
    print("      python usage_examples.py run \"my_experiment\"")
    print()
    print("  help")
    print("    Show this help message")
    print()
    print("TYPICAL WORKFLOW:")
    print("  1. python usage_examples.py status        # Check if ready")
    print("  2. python usage_examples.py list          # See available samples")
    print("  3. python usage_examples.py create \"test\" # Create new sample set")
    print("  4. python usage_examples.py run \"test\"    # Execute sample set")
    print("  5. python usage_examples.py status -v     # Monitor progress")
    print()
    print("For Python API usage, import functions:")
    print("  from usage_examples import test_instrument_status, create_test_sample_set")


if __name__ == "__main__":
    if len(sys.argv) > 1:
        command = sys.argv[1]
        if command == "status":
            verbose = len(sys.argv) > 2 and sys.argv[2] == "-v"
            test_instrument_status(verbose)
        elif command == "list":
            list_available_sample_sets()
        elif command == "inspect" and len(sys.argv) > 2:
            inspect_sample_set(sys.argv[2])
        elif command == "create" and len(sys.argv) > 2:
            name = sys.argv[2]
            # Parse optional parameters
            volume = None
            template = None
            runtime = None
            vials = None
            sample_names = None
            sample_weight = None
            dilution = None
            
            # Check for additional arguments
            for i in range(3, len(sys.argv)):
                arg = sys.argv[i]
                if arg.startswith("--volume="):
                    volume = arg.split("=", 1)[1]
                elif arg.startswith("--template="):
                    template = arg.split("=", 1)[1]
                elif arg.startswith("--runtime="):
                    runtime = float(arg.split("=", 1)[1])
                elif arg.startswith("--vials="):
                    vials = arg.split("=", 1)[1]
                elif arg.startswith("--sample-names="):
                    sample_names = arg.split("=", 1)[1]
                elif arg.startswith("--sample-weight="):
                    sample_weight = float(arg.split("=", 1)[1])
                elif arg.startswith("--dilution="):
                    dilution = float(arg.split("=", 1)[1])
            
            create_test_sample_set(name, volume=volume, template=template, runtime=runtime, 
                                 vials=vials, sample_names=sample_names, 
                                 sample_weight=sample_weight, dilution=dilution)
        elif command == "run" and len(sys.argv) > 2:
            run_sample_set(sys.argv[2])
        elif command == "help":
            show_help()
        else:
            print("Usage:")
            print("  python usage_examples.py status [-v]")
            print("  python usage_examples.py list")
            print("  python usage_examples.py inspect <sample_set_name>")
            print("  python usage_examples.py create <sample_set_name> [--volume=X] [--template=X] [--runtime=X] [--vials=X] [--sample-names=X] [--sample-weight=X] [--dilution=X]")
            print("  python usage_examples.py run <sample_set_name>")
            print("  python usage_examples.py help")
    else:
        show_help()

