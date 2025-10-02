# Waters Empower Toolkit - Comprehensive C# Library

A complete, production-ready C# library for automating Waters Empower chromatography systems using COM objects.

## Features

### Core Components
- **EmpowerProject**: Complete wrapper for MillenniumToolkit.Project COM object
- **EmpowerInstrument**: Full wrapper for MillenniumToolkit.Instrument COM object  
- **EmpowerSampleSetMethod**: Complete wrapper for MillenniumToolkit.SampleSetMethod COM object
- **EmpowerToolkit**: Unified high-level interface combining all functionality
- **ConnectionStatus**: Status and progress tracking
- **EmpowerHelper**: Static convenience methods for quick operations

### Functionality
- **Authentication**: Login/Logoff with project credentials
- **System Discovery**: Enumerate systems, nodes, and methods
- **Instrument Control**: Connect, disconnect, execute sample sets
- **Queue Management**: Start, stop, clear, and manage sample queues
- **Operation Control**: Pause, resume, stop running operations
- **Method Management**: Load, save, delete sample set methods
- **Status Monitoring**: Real-time status and progress tracking
- **Configuration**: Automatic config file loading with sensible defaults
- **Diagnostics**: Comprehensive health checks and validation

## Architecture

### Memory Management
- Implements IDisposable pattern for proper COM object cleanup
- Uses Marshal.ReleaseComObject to prevent memory leaks
- Automatic resource disposal in using statements
- Finalizers for emergency cleanup

### COM Integration
- Uses Type.GetTypeFromProgID for late-bound COM object creation
- Reflection-based method invocation for maximum compatibility
- Proper exception handling with COM error codes
- C# 2.0 compatible (no default parameters or var keywords)

### Error Handling
- Detailed exception messages with stack traces
- COM error code translation to readable descriptions
- Graceful degradation when features are unavailable
- Comprehensive diagnostics with pass/fail reporting

## Usage Examples

### Quick Start
```csharp
using WatersEmpowerToolkit;

// Quick diagnostics
string report = EmpowerHelper.RunDiagnostics();
Console.WriteLine(report);

// Quick system discovery
Dictionary<string, string[]> systems = EmpowerHelper.DiscoverSystems();

// Quick sample set execution
bool success = EmpowerHelper.ExecuteSampleSet("test cjs");
```

### Full Control
```csharp
using (EmpowerToolkit toolkit = new EmpowerToolkit())
{
    // Initialize and login
    toolkit.Initialize();
    
    // Connect to instrument
    toolkit.ConnectInstrument();
    
    // Execute sample set
    toolkit.Instrument.ReplaceSampleSet("test cjs");
    
    // Monitor progress
    while (toolkit.Instrument.Status != "Idle")
    {
        Console.WriteLine("Status: " + toolkit.Instrument.Status);
        Console.WriteLine("Progress: " + toolkit.Instrument.Progress);
        Thread.Sleep(5000);
    }
}
```

### Individual Components
```csharp
// Project operations
using (EmpowerProject project = new EmpowerProject())
{
    project.Create();
    project.Login("", "Waters GPC Training", "system", "manager");
    
    string[] projects = project.Projects;
    string[] services = project.Services;
    
    project.Logoff();
}

// Instrument operations
using (EmpowerInstrument instrument = new EmpowerInstrument())
{
    instrument.Create();
    
    string[] systems = instrument.Systems;
    string[] nodes = instrument.AcquisitionServers;
    
    instrument.Connect("Waters-h4q6k34", "Arc HPLC");
    
    string[] methods = instrument.SampleSetMethods;
    instrument.ReplaceSampleSet("test cjs");
    
    instrument.Disconnect();
}
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

## Compilation

### Library
```cmd
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /target:library /out:WatersEmpowerToolkit.dll WatersEmpowerToolkit.cs
```

### Test Applications
```cmd
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /reference:WatersEmpowerToolkit.dll /out:ComprehensiveTest.exe ComprehensiveTest.cs
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /reference:WatersEmpowerToolkit.dll /out:QuickTest.exe QuickTest.cs
```

## API Reference

### EmpowerProject Class
```csharp
// Core methods
bool Create()
bool Login(string database, string project, string username, string password)
bool Login() // Uses defaults
bool Logoff()

// Properties
string[] Projects { get; }
string[] Services { get; }
bool IsConnected { get; }

// Utilities
string GetErrorDescription(int errorCode)
```

### EmpowerInstrument Class
```csharp
// Core methods
bool Create()
bool Connect(string nodeName, string systemName, int timeout)
bool Connect(string nodeName, string systemName) // 30 second timeout
bool Disconnect()

// Properties
string[] Systems { get; }
string[] AcquisitionServers { get; }
string[] SampleSetMethods { get; }
bool IsConnected { get; }
string Status { get; }
string Progress { get; }

// Operations
bool ReplaceSampleSet(string methodName)
bool Stop()
bool Pause()
bool Resume()

// Queue management
bool QueueSampleSet(string sampleSetName)
bool StartQueue()
bool StopQueue()
bool ClearQueue()

// Status
ConnectionStatus GetConnectionStatus()
```

### EmpowerSampleSetMethod Class
```csharp
// Core methods
bool Create()
bool Load(string methodName)
bool Save(string methodName)
bool Delete(string methodName)

// Properties
string[] MethodNames { get; }
```

### EmpowerToolkit Class
```csharp
// Constructors
EmpowerToolkit() // Uses "secrets.ini"
EmpowerToolkit(string configFile)

// Core methods
bool Initialize()
bool ConnectInstrument()
Dictionary<string, string[]> DiscoverSystems()
string RunDiagnostics()
void Cleanup()

// Properties
EmpowerProject Project { get; }
EmpowerInstrument Instrument { get; }
EmpowerSampleSetMethod SampleSetMethod { get; }
Dictionary<string, string> Config { get; }
```

### EmpowerHelper Static Methods
```csharp
static Dictionary<string, string[]> DiscoverSystems()
static Dictionary<string, string[]> DiscoverSystems(string configFile)
static bool ExecuteSampleSet(string methodName)
static bool ExecuteSampleSet(string methodName, string configFile)
static string RunDiagnostics()
static string RunDiagnostics(string configFile)
```

## Requirements

- Windows with .NET Framework 3.5 or later
- Waters Empower installation with MillenniumToolkit COM objects
- Appropriate COM object registration (32-bit for older systems)
- Valid Empower project credentials

## Architecture Considerations

### 32-bit vs 64-bit
The library detects COM registration issues automatically. If you get error 80040154, you may need:
- 32-bit compilation for 32-bit COM objects
- Proper COM object registration
- Compatible .NET Framework version

### Thread Safety
COM objects are not thread-safe. Use one instance per thread or implement proper synchronization.

### Performance
Uses late-bound COM calls via reflection. For high-performance scenarios, consider early-bound interop assemblies.

## Testing

The library includes comprehensive test applications:

- **QuickTest.exe**: Basic functionality and diagnostics
- **ComprehensiveTest.exe**: Complete test suite with all methods
- **LibraryDemo.exe**: Feature overview and capabilities

All tests detect and report COM registration issues clearly.

## Integration

This library serves as the foundation for:
- Python wrappers using subprocess calls
- REST API services for web integration  
- PowerShell modules for system administration
- Custom applications with Empower automation

## Troubleshooting

### COM Registration Issues
```
Error: 80040154 - Class not registered
Solution: Ensure MillenniumToolkit COM objects are properly registered
```

### Authentication Failures  
```
Error: Login failed
Solution: Check credentials in secrets.ini, verify Empower is running
```

### Connection Timeouts
```
Error: Connection timeout
Solution: Verify system/node names, check network connectivity
```

## License

See LICENSE file in the project root.

## Contributing

See CONTRIBUTING.md for development guidelines and patterns.
