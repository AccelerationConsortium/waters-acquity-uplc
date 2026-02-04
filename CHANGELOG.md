# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased] - 2026-02-02

### DEVELOPMENT MODE TOGGLE - Manual vs Automated Empower Execution
- **🔧 DEVELOPMENT TOGGLE**: Added configuration flag to easily switch between manual and automated Empower execution
  - Added `empower.manual_execution` configuration option
  - Added `MANUAL_EMPOWER_MODE` toggle in main() function for easy development switching
  - Manual mode displays sample set parameters and waits for user confirmation
  - Automated mode runs full programmatic Empower execution as before
  - Enhanced logging to clearly indicate which mode is active

### EMPOWER PARAMETER FIXES - COM Execution Issue Resolution
- **🔧 SIMPLIFIED SAMPLE SET PARAMETERS**: Fixed Empower sample set creation parameters to match actual C# API requirements
  - Cleaned up parameter passing to only include explicitly provided values (no None values)
  - Simplified sample set naming: `GPC_{timestamp}` instead of `{sample_name}_{full_timestamp}`
  - Fixed parameter validation to prevent COM invocation errors during sample set execution

## [Previous] - 2026-01-30

### FLEXIBLE TRAY MANAGEMENT - Sample Handling Options
- **⚙️ CONFIGURABLE TRAY EXTRACTION**: Added optional `initial_tray_open` parameter:
  - `initial_tray_open=True` (default): Extract tray, wait for loading, insert tray
  - `initial_tray_open=False`: Skip extraction, assume samples already loaded
- **📤 CONFIGURABLE POST-COMPLETION TRAY**: Added optional `send_out_after` parameter:
  - `send_out_after=True` (default): Extract tray after completion for sample collection
  - `send_out_after=False`: Leave tray in position after completion
- **⏱️ INJECTION TIMING CONTROL**: Added optional `astra_ready_delay` parameter:
  - Configurable delay (default 5.0 seconds) between ASTRA thread start and Empower execution
  - Ensures ASTRA is fully ready to receive injection signal from Empower
  - Example set to 8.0 seconds for safer timing margin
- **🔍 ENHANCED TIMING LOGS**: Added detailed timing logs around injection signal transmission
- **🎯 USE CASES SUPPORTED**:
  - Pre-loaded batch processing (`initial_tray_open=False`)
  - Automated sample collection (`send_out_after=True`)
  - Minimal tray movement for throughput (`initial_tray_open=False`, `send_out_after=False`)
- **📋 EXAMPLE CONFIGURATION**: Updated sample_info with new parameters and defaults

### ASTRA THREADING REDESIGN - Parallel Data Collection
- **🔄 PARALLEL WORKFLOW DESIGN**: Restructured GPC automation for proper parallel execution:
  - Phase 1: `prepare_experiment_for_collection()` - Setup experiment, start collection
  - Phase 2: `wait_and_collect_data()` - **THREADED** wait for injection + data collection  
  - Phase 3: `process_and_save_results()` - Process data and save files (after collection)
- **⚡ THREADING-READY**: Phase 2 designed to run in separate thread while HPLC injection occurs
- **🎯 PROPER TIMING**: Data collection now happens concurrently with HPLC, not sequentially after
- **🧵 ORCHESTRATOR THREADING**: Implemented threaded execution in orchestrator:
  - ASTRA `wait_and_collect_data()` runs in background thread
  - Empower execution runs in main thread (triggers ASTRA injection signal)
  - Main thread waits for ASTRA thread completion before processing results
- **📊 COLLECTION DURATION LOGGING**: Added collection duration to processing phase logs
- **🔧 METHOD SIGNATURES FIXED**:
  - ASTRA: `prepare_experiment_for_collection(method_path, experiment_name=sample_name)`
  - Empower: `execute_sample_set()` returns status dict, not boolean
  - Added proper error handling for Empower execution status
- **🔧 METHOD CONSOLIDATION**:
  - Removed: `wait_for_injection_signal()` and `collect_and_process_data()`
  - Added: `wait_and_collect_data()` (combines injection wait + collection)
  - Added: `process_and_save_results()` (handles all post-collection processing)
- **✅ BACKWARDS COMPATIBILITY**: Updated `run_complete_workflow()` to use new structure

### CRITICAL FIX - Multi-Tray Status Logic
- **🔧 COMPLETE TRAY STATUS MAPPING**: Discovered full status patterns for both trays:
  - Tray 1 OUT: `drawer_tray_status="DrawerAndTray"` + `Mode: Extract(1)`
  - Tray 1 IN: `drawer_tray_status="NoDrawerNoTray"` + `Mode: Insert(1)`  
  - Tray 2 OUT: `drawer_tray_status="DrawerNoTray"` + `Mode: Extract(0)`
  - Tray 2 IN: `drawer_tray_status="NoDrawerNoTray"` + `Mode: Insert(0)`
- **✅ SMART MULTI-TRAY LOGIC**: Implemented `ensure_tray_available()` method that:
  - Parses current tray state from Mode field
  - Handles wrong tray being out (inserts it first)
  - Extracts correct target tray
  - Prevents conflicts between Tray 1 and Tray 2
- **🛠️ NEW HELPER FUNCTIONS**: 
  - `_parse_current_tray_from_mode()`: Determines which tray is currently out
  - `_is_tray_extracted()`: Checks if specific tray is extracted
- **🧪 ENHANCED TEST SCRIPT**: Updated test script to show complete mode analysis and tray-specific interpretations
- **⚠️ BACKWARDS COMPATIBILITY**: Replaces simple tray logic with comprehensive multi-tray management

### MULTI-EXPERIMENT REFACTORING
- **✅ DECOUPLED EXPERIMENT STATE**: Refactored `GPCAutomation` class to support multiple experiments by removing experiment-specific state from `__init__`
- **✅ STATELESS METHODS**: Methods now return experiment metadata (experiment_id, timestamp, results_folder) instead of storing as instance variables
- **✅ ADMIN PERSISTENCE**: Admin connection now managed separately from individual experiments via `initialize_admin()` and `cleanup_admin()`
- **✅ EXPERIMENT LIFECYCLE**: New methods `prepare_experiment_for_collection()`, `close_experiment()`, and `create_results_folder()` for per-experiment management
- **✅ MULTI-EXPERIMENT SUPPORT**: Added `run_multiple_experiments()` method demonstrating how to run several experiments sequentially
- **✅ BACKWARDS COMPATIBILITY**: Updated `run_complete_workflow()` to work with new architecture while maintaining same interface
- **✅ RUNTIME STATE MANAGEMENT**: Admin stores runtime state, methods return data structures for external orchestration

### ASTRA Integration - Class-Based Automation
- **✅ CLASS-BASED WORKFLOW**: Created `GPCAutomation` class in `gpc_automation_class.py` for object-oriented ASTRA control
- **✅ PHASE SEPARATION**: Split workflow into distinct phases: `prepare_for_collection()`, `wait_for_injection_signal()`, `collect_and_process_data()`
- **✅ CONFIGURABLE PARAMETERS**: Constructor accepts `astra_method_path` and `base_results_dir` as main configurable parameters
- **✅ EXTERNAL INTEGRATION**: Designed for integration with HPLC/Empower systems via separated injection waiting phase
- **✅ BACKWARDS COMPATIBILITY**: Includes `run_complete_workflow()` method that maintains original script behavior
- **✅ MODULAR CLEANUP**: Separate `cleanup()` method for integration with multi-system orchestrator
- **✅ DIRECTORY CLEANUP**: Removed 16 development/test files from astra folder, keeping only essential production files

### File Organization
- **NEW**: `astra/gpc_automation_class.py` - Object-oriented automation interface
- **REMOVED**: Development test files, outdated documentation, alternative versions
- **RETAINED**: `enhanced_gpc_automation.py` (original script), `astra_admin.py`, `sdk_helper.py`, `extract_molecular_weights.py`

## [Unreleased] - 2026-01-29
### Automation Portal Workflow Integration
- **✅ INTEGRATED STATUS CHECKING**: Added `_check_ready()` and `_wait_for_ready()` methods directly to `AutomationPortalDriver` class
- **✅ AUTOMATED READINESS**: Modified `extract_drawer()` and `insert_drawer()` to automatically check system status before operations
- **✅ SIMPLIFIED WORKFLOW**: Updated `automation_portal_workflow.py` to use simple one-liner commands (connect, insert, extract)
- **✅ ENHANCED LOGGING**: Improved logging messages for better operation tracking
- **✅ STABILIZATION DELAY**: Added 5-second delay after reaching "Idle" state to ensure system stabilization before next operation
- **✅ POSITION RENUMBERING**: Changed position numbering from 0/1 to 1/2 (Position 1=Tray 1, Position 2=Tray 2)
- **✅ CONSISTENT TERMINOLOGY**: Updated all references from "drawer" to "tray" for consistency
- **✅ TIMEOUT HANDLING**: Implemented 120-second timeout with 2-second polling intervals for operations
- **✅ OPERATIONAL STATE**: Ensures system is in OPERATIONAL state with Idle status before any movement commands

## [Unreleased] - 2026-01-15
### Code Quality Improvements
- **✅ FLAKE8 CLEANUP**: Fixed Python code style issues across automation-portal and empower modules
- **✅ WHITESPACE**: Removed trailing whitespace and blank line formatting issues
- **✅ IMPORTS**: Removed unused imports (datetime, Optional, Union) 
- **✅ LINE LENGTH**: Fixed long lines exceeding 100 characters
- **✅ DOCSTRINGS**: Improved docstring formatting and consistency
- **✅ AUTOPEP8**: Applied automatic Python formatting for consistency

### Directory Cleanup
- **✅ AUTOMATION-PORTAL**: Cleaned up directory structure, removed test files and empty scripts
- **✅ REMOVED FILES**: test_*.py, clear_error.py, sample_transfer_check.py, transfer_to_position_1.py
- **✅ CACHE CLEANUP**: Removed __pycache__ directories
- **✅ GITIGNORE**: Updated to prevent test files from being tracked

## [0.6.0] - 2025-10-02
### Sample Set Data Reading and Project Cleanup
- **✅ SAMPLE SET READER**: Created clean, working SampleSetReader.cs for non-invasive data access
- **✅ FIELD DISCOVERY**: Successfully identified key Waters COM field names using official patterns
- **✅ DATA EXTRACTION**: Confirmed working fields: SampleName, Vial, Runtime, Function, InjVol, SampleWeight, Dilution
- **✅ INJECTION VOLUME**: Found actual injection volume data (InjVol field) showing 10.0-30.0 µL values
- **✅ PROJECT CLEANUP**: Archived sample-management/ folder containing experimental scripts
- **✅ LEAN IMPLEMENTATION**: Simplified to essential, proven functionality following official Waters examples

### Technical Achievements
- **Official COM patterns**: Followed ssl.Set("Runtime", "1") pattern from Waters documentation
- **0-based indexing**: Confirmed Waters COM collections use 0-based indexing (not 1-based)
- **Working field names**: Discovered official field names: InjVol (not InjectionVolume), Function, etc.
- **Clean data access**: Read-only sample set data retrieval without execution risk
- **Archive strategy**: Moved experimental code to archive while preserving working implementations

### Data Successfully Retrieved
- Sample names and vial positions for multiple sample sets
- Runtime information (10.00-14.00 minutes)
- Injection volumes (10.0-30.0 µL per sample)  
- Sample functions ("Inject Samples")
- Sample weights and dilution factors

## [0.5.0] - 2025-10-02
### Successful Empower Execution and Directory Cleanup
- **✅ WORKING EXECUTION**: Successfully implemented actual sample set execution using official `Run()` method
- **✅ BUSY STATE DETECTION**: Added intelligent checking for instrument status before execution attempts
- **✅ REAL-TIME MONITORING**: Proper InstrumentStatus monitoring showing active execution progress
- **✅ ERROR ELIMINATION**: Removed all "Unknown name" COM errors by using only working methods
- **✅ DIRECTORY CLEANUP**: Organized working scripts and archived non-essential files

### Key Achievements
- **Execution vs. Altering resolved**: Distinguished between `Replace()` (loading) and `Run()` (executing)
- **Official method patterns**: Followed Waters documentation examples for proper execution
- **Instrument conflict prevention**: Pre-execution status checks prevent interference with running operations
- **Clean monitoring**: 5-check monitoring cycles instead of excessive polling
- **Production-ready structure**: Kept only functional, tested scripts

### Working Files Retained
- `SampleSetExtractor.cs` - Main execution script with Run() method
- `SampleSetExtractor.exe` - Compiled 32-bit executable  
- `CleanStatusMonitor.exe` - Real-time status monitoring
- `WatersEmpowerToolkit.cs` - Complete C# library
- `secrets.ini` - Configuration file
- `CSHARP_LIBRARY_README.md` - Library documentation

### Technical Implementation
- Uses `_instrument.Run(sampleSetMethodName, newName)` from official examples
- Checks `SystemStateDescription` for "Sample Set" + "Running/Waiting/Injection" states
- InstrumentStatus object provides: State, Vial, Injection, RunTime, SampleSetMethodName
- 32-bit compilation required: `csc.exe /platform:x86`
- Proper COM cleanup with Marshal.ReleaseComObject

### Status Monitoring Improvements
- ConnectionStatus: Shows COM connection establishment only
- InstrumentStatus: Shows actual analytical run status
- Clear differentiation between IDLE/READY vs ACTIVELY EXECUTING
- Graceful handling of busy instruments with user guidance

## [0.4.0] - 2025-09-25
### Comprehensive C# Library for Empower Toolkit
- **Complete C# library**: WatersEmpowerToolkit.cs with full COM object wrappers
- **Production-ready**: EmpowerProject, EmpowerInstrument, EmpowerSampleSetMethod classes
- **Memory management**: Proper COM object lifetime using IDisposable pattern
- **C# 2.0 compatible**: Works with .NET Framework 3.5 compiler
- **Unified interface**: EmpowerToolkit class combining all functionality
- **Comprehensive diagnostics**: Built-in health checks and system validation
- **Configuration management**: Automatic config file loading with defaults
- **Error handling**: Detailed exception messages with COM error codes
- **Static helpers**: EmpowerHelper class for quick operations
- **All documented methods**: Replace, Stop, Pause, Resume, Queue operations

### Added
- WatersEmpowerToolkit.cs: Complete C# library (1500+ lines)
- ComprehensiveTest.cs: Full test suite exercising all functionality
- QuickTest.cs: Simple diagnostic test application
- LibraryDemo.cs: Library feature demonstration

### Technical Features
- ConnectionStatus class for status management
- Proper COM object creation using Type.GetTypeFromProgID
- Method overloading for compatibility (no default parameters)
- Marshal.ReleaseComObject for memory cleanup
- Reflection-based COM method invocation
- Configuration file parsing with fallback defaults
- Comprehensive error descriptions and stack traces

## [0.3.0] - 2025-09-25
### Python Wrapper for Empower Toolkit
- **Complete Python wrapper**: Created comprehensive Python interface for Waters Empower Toolkit COM objects
- **Object-oriented design**: EmpowerProject, EmpowerInstrument, and EmpowerSampleSetMethod classes
- **Context management**: Automatic resource cleanup using Python context managers
- **Configuration support**: Load settings from secrets.ini file
- **Error handling**: Comprehensive exception handling with detailed error messages
- **Usage examples**: Complete example scripts demonstrating all functionality
- **Official patterns**: Based on Waters ToolkitHelp.chm and instrument control examples

### Added
- empower_toolkit.py: Main Python wrapper with all COM object interfaces
- example_usage.py: Comprehensive usage examples for all features
- simple_test.py: Basic functionality testing script
- PYTHON_WRAPPER_README.md: Complete documentation for Python wrapper

## [0.2.0] - 2025-09-18
### Major Refactoring
- **Production-ready automation portal driver**: Fixed critical bugs in error detection and command formatting
- **Interactive menu system**: Added `automation_menu.py` for easy command-line operation
- **Clean directory structure**: Removed all debugging and test files
- **Updated documentation**: Comprehensive README.md with usage guide and API reference
- **Bug fixes in automation_portal_driver.py**:
  - Fixed extract_drawer() and insert_drawer() methods to properly detect errors
  - Corrected command format (removed sequence numbers from Extract/Insert commands)
  - Fixed initialize() method to handle unknown command responses
  - Improved error state detection and timeout handling

### Added
- automation_menu.py: Interactive command-line interface for automation portal operations
- Comprehensive usage guide in README.md
- Error recovery procedures and troubleshooting guide

### Removed
- All debugging files: clear_error.py, connection_test.py, debug_test.py, demo.py, etc.
- Test scripts: test_*.py files
- Cache directories: __pycache__, archive/
- Duplicate sample transfer check functionality

## [0.1.1] - 2025-09-02
### Changed
- automation-portal/automation_portal_driver.py: Align with org Copilot instructions.
  - Replace `WatersAcquityError` uses with `AutomationPortalError`.
  - Add context manager methods `__enter__` and `__exit__`.
  - Make `_parse_portal_response` parameters optional to match internal calls.
  - Update examples to use `AutomationPortalDriver`.
  - Remove `if __name__ == "__main__"` block.

## [0.1.0] - 2025-09-02
### Added
- Initial cleaned repository structure with Automation Portal driver and sample-management components.

## [Unreleased] - 2025-09-19

### Added
- Comprehensive Waters technical support debug package (`debug_for_waters.ps1`)
- Complete COM registration breakthrough analysis (`BREAKTHROUGH_REPORT.md`)
- Advanced ProgID testing utilities (`TestProgID.cs`)

### Fixed
- COM DLL registration using correct 32-bit regsvr32 path
- Identified successful `DllRegisterServer` operation for MilTk.dll
- Confirmed ProgID to CLSID mapping functionality

### Investigation
- Discovered MilTk.dll successfully registers with `C:\Windows\SysWOW64\regsvr32.exe`
- Confirmed ProgID resolution works but CLSID implementation missing
- Identified root cause: CLSID `{19F37CF2-C7C2-11D0-8714-0020AFEE2C2A}` not in registry
- Verified all Empower services running (Oracle, Waters components)
- Created comprehensive technical support documentation package

### Technical Details
- Bitness alignment: 32-bit registration required for Waters COM components
- Service dependencies: Multiple Oracle/Waters services confirmed running
- Registration status: DLL registers successfully but target CLSID missing
- Error code: 0x80040154 (REGDB_E_CLASSNOTREG) - Class not registered

## [2025-09-22] - Empower COM Authentication Breakthrough

### Added
- **EmpowerLoginVerification.cs**: Comprehensive authentication testing with wrong/correct credentials
- **EmpowerWorkflow.cs**: Complete workflow implementation following VBScript pattern
- **EmpowerExplorer.cs**: COM object method discovery and exploration
- **EmpowerSampleManager.cs**: Sample set operation testing
- **EmpowerCOMDiscovery.cs**: Available COM object enumeration

### Verified
- ✅ Real authentication validation (wrong passwords properly rejected)
- ✅ Login syntax: `Login("", "Waters GPC Training", "system", "manager")`
- ✅ Project enumeration: Successfully retrieves available projects
- ✅ COM object creation: Both MillenniumToolkit.Project and MillenniumToolkit.Instrument
- ✅ Session management: Proper login/logoff functionality
- ✅ Data access control: Only correct credentials allow project data retrieval

### Technical Implementation
- 32-bit COM registration with `C:\Windows\SysWOW64\regsvr32.exe MilTk.dll`
- Platform targeting: `/platform:x86` compilation mandatory
- Late-bound COM pattern: `InvokeMember` for all method calls
- Workflow pattern: Project → Login → Instrument → Connect → Sample Operations

### Breakthrough Discovery
- Sample set operations require **MillenniumToolkit.Instrument** object, not Project
- Correct workflow: Create Project → Login → Create Instrument → Connect → Execute
- Authentication is genuine - wrong credentials consistently fail, correct ones succeed

### Next Steps
- Implement instrument connection with actual system names
- Add sample set method discovery and execution
- Build production automation workflow on verified foundation

## [2025-01-25] - Major Directory Cleanup
### Added
- **SystemDiscoveryExtractor.cs** - Primary working tool based on official Waters patterns
- Clean README.md with only working functionality documented

### Changed
- Updated secrets.ini with correct system name case ("Arc HPLC")
- Corrected connection parameter order (node first, system second)

### Removed
- EmpowerSampleSetExtractor.cs/.exe (guessed method names)
- SimpleSampleSetExtractor.cs/.exe (guessed method names)
- SampleSetExtractor.cs/.exe (guessed method names)
- SampleSetLister.cs/.exe (guessed method names)
- DocumentedSampleSetExtractor.cs/.exe (guessed method names)
- DiagnosticSampleSetExtractor.cs/.exe (debugging version)
- ConfigurableSampleSetExtractor.cs/.exe (redundant)
- WorkingSampleSetExtractor.cs/.exe (partial working)
- OfficialSampleSetExtractor.cs/.exe (replaced by SystemDiscoveryExtractor)
- EmpowerLoginVerification.cs (non-functional)
- EmpowerWorkflow.cs (non-functional)
- analyze_toolkit_help.py (debugging tool)
- test_documentation_examples.py (debugging tool)
- extract_pdf_docs.py (redundant)

### Fixed
- Directory now contains only proven, working code
- Eliminated all failed attempts with guessed Waters method names
- Based on official Waters instrument control example patterns

## [Unreleased] - 2026-02-02

### THREADING ARCHITECTURE REDESIGN - COM Compatibility Fix
- **🏗️ REVERSE THREADING APPROACH**: Fixed critical threading deadlock by redesigning execution flow:
  - **OLD**: ASTRA waits in background thread (fails due to COM threading constraints)  
  - **NEW**: ASTRA waits in main thread, Empower executes in background thread
- **🔧 THREADING SEQUENCE**:
  - Phase 5: ASTRA `wait_waiting_for_auto_inject()` in main thread (COM compatible)
  - Phase 6: Start Empower thread with configurable delay (default 120s)
  - Phase 7: ASTRA continues `wait_collection_started()` → `wait_collection_finished()` in main thread
  - Empower thread triggers injection signal after delay
- **⏱️ IMPROVED TIMING CONTROL**: Updated `astra_ready_delay` to default 120 seconds (user suggested)
- **🧵 COM INTERFACE COMPATIBILITY**: Keeps ASTRA COM calls in main thread where they work reliably
- **🎯 FULLY AUTOMATED**: Achieves parallel execution without manual intervention
- **📊 ENHANCED LOGGING**: Added thread-specific log messages for debugging timing issues
- **✅ PROVEN APPROACH**: Based on working sequential logic from `enhanced_gpc_automation.py`
