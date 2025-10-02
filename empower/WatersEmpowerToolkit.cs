using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace WatersEmpowerToolkit
{
    /// <summary>
    /// Represents connection status information
    /// </summary>
    public class ConnectionStatus
    {
        public bool Done { get; set; }
        public string Text { get; set; }
        public int ErrorCode { get; set; }
        
        public ConnectionStatus()
        {
            Done = false;
            Text = string.Empty;
            ErrorCode = 0;
        }
    }

    /// <summary>
    /// Wrapper for MillenniumToolkit.Project COM object
    /// </summary>
    public class EmpowerProject : IDisposable
    {
        private object _project;
        private bool _connected;
        private bool _disposed;

        public EmpowerProject()
        {
            _project = null;
            _connected = false;
            _disposed = false;
        }

        /// <summary>
        /// Create the Project COM object
        /// </summary>
        public bool Create()
        {
            try
            {
                Type projectType = Type.GetTypeFromProgID("MillenniumToolkit.Project");
                _project = Activator.CreateInstance(projectType);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create Project object: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Login to Empower project
        /// </summary>
        public bool Login(string database, string project, string username, string password)
        {
            return LoginInternal(database, project, username, password);
        }

        /// <summary>
        /// Login to Empower project with default parameters
        /// </summary>
        public bool Login()
        {
            return LoginInternal("", "Waters GPC Training", "system", "manager");
        }

        private bool LoginInternal(string database, string project, string username, string password)
        {
            if (_project == null)
                throw new InvalidOperationException("Project object not created. Call Create() first.");

            try
            {
                _project.GetType().InvokeMember(
                    "Login",
                    BindingFlags.InvokeMethod,
                    null,
                    _project,
                    new object[] { database, project, username, password }
                );
                _connected = true;
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Login failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Logoff from Empower project
        /// </summary>
        public bool Logoff()
        {
            if (_project != null && _connected)
            {
                try
                {
                    _project.GetType().InvokeMember(
                        "Logoff",
                        BindingFlags.InvokeMethod,
                        null,
                        _project,
                        null
                    );
                    _connected = false;
                    return true;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Logoff failed: " + ex.Message, ex);
                }
            }
            return false;
        }

        /// <summary>
        /// Get detailed error description for error code
        /// </summary>
        public string GetErrorDescription(int errorCode)
        {
            if (_project == null)
                return "Project object not available";

            try
            {
                object result = _project.GetType().InvokeMember(
                    "TkErrorDescription",
                    BindingFlags.InvokeMethod,
                    null,
                    _project,
                    new object[] { errorCode }
                );
                return result.ToString();
            }
            catch (Exception)
            {
                return "Unknown error code: " + errorCode;
            }
        }

        /// <summary>
        /// Get list of available projects
        /// </summary>
        public string[] Projects
        {
            get
            {
                if (_project == null)
                    return new string[0];

                try
                {
                    object projectsObj = _project.GetType().InvokeMember(
                        "Projects",
                        BindingFlags.GetProperty,
                        null,
                        _project,
                        null
                    );

                    if (projectsObj != null && projectsObj != System.DBNull.Value)
                    {
                        return (string[])projectsObj;
                    }
                    return new string[0];
                }
                catch (Exception)
                {
                    return new string[0];
                }
            }
        }

        /// <summary>
        /// Get list of available services
        /// </summary>
        public string[] Services
        {
            get
            {
                if (_project == null)
                    return new string[0];

                try
                {
                    object servicesObj = _project.GetType().InvokeMember(
                        "Services",
                        BindingFlags.GetProperty,
                        null,
                        _project,
                        null
                    );

                    if (servicesObj != null && servicesObj != System.DBNull.Value)
                    {
                        return (string[])servicesObj;
                    }
                    return new string[0];
                }
                catch (Exception)
                {
                    return new string[0];
                }
            }
        }

        /// <summary>
        /// Check if connected to project
        /// </summary>
        public bool IsConnected
        {
            get { return _connected; }
        }

        /// <summary>
        /// Get the underlying COM object (advanced users only)
        /// </summary>
        public object ComObject
        {
            get { return _project; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        Logoff();
                    }
                    catch { }
                }

                if (_project != null)
                {
                    try
                    {
                        Marshal.ReleaseComObject(_project);
                    }
                    catch { }
                    _project = null;
                }

                _disposed = true;
            }
        }

        ~EmpowerProject()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Wrapper for MillenniumToolkit.Instrument COM object
    /// </summary>
    public class EmpowerInstrument : IDisposable
    {
        private object _instrument;
        private bool _connected;
        private bool _disposed;

        public EmpowerInstrument()
        {
            _instrument = null;
            _connected = false;
            _disposed = false;
        }

        /// <summary>
        /// Create the Instrument COM object
        /// </summary>
        public bool Create()
        {
            try
            {
                Type instrumentType = Type.GetTypeFromProgID("MillenniumToolkit.Instrument");
                _instrument = Activator.CreateInstance(instrumentType);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create Instrument object: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Get available systems
        /// </summary>
        public string[] Systems
        {
            get
            {
                if (_instrument == null)
                    return new string[0];

                try
                {
                    object systemsObj = _instrument.GetType().InvokeMember(
                        "Systems",
                        BindingFlags.GetProperty,
                        null,
                        _instrument,
                        null
                    );

                    if (systemsObj != null && systemsObj != System.DBNull.Value)
                    {
                        return (string[])systemsObj;
                    }
                    return new string[0];
                }
                catch (Exception)
                {
                    return new string[0];
                }
            }
        }

        /// <summary>
        /// Get available acquisition servers (nodes)
        /// </summary>
        public string[] AcquisitionServers
        {
            get
            {
                if (_instrument == null)
                    return new string[0];

                try
                {
                    object serversObj = _instrument.GetType().InvokeMember(
                        "AcqServers",
                        BindingFlags.GetProperty,
                        null,
                        _instrument,
                        null
                    );

                    if (serversObj != null && serversObj != System.DBNull.Value)
                    {
                        return (string[])serversObj;
                    }
                    return new string[0];
                }
                catch (Exception)
                {
                    return new string[0];
                }
            }
        }

        /// <summary>
        /// Connect to instrument system
        /// </summary>
        /// <param name="nodeName">Name of acquisition server/node</param>
        /// <param name="systemName">Name of instrument system</param>
        /// <param name="timeout">Connection timeout in seconds</param>
        public bool Connect(string nodeName, string systemName, int timeout)
        {
            return ConnectInternal(nodeName, systemName, timeout);
        }

        /// <summary>
        /// Connect to instrument system with default timeout
        /// </summary>
        public bool Connect(string nodeName, string systemName)
        {
            return ConnectInternal(nodeName, systemName, 30);
        }

        private bool ConnectInternal(string nodeName, string systemName, int timeout)
        {
            if (_instrument == null)
                throw new InvalidOperationException("Instrument object not created. Call Create() first.");

            try
            {
                // Official pattern: Connect(nodeName, systemName) - node first!
                _instrument.GetType().InvokeMember(
                    "Connect",
                    BindingFlags.InvokeMethod,
                    null,
                    _instrument,
                    new object[] { nodeName, systemName }
                );

                // Wait for connection to complete
                DateTime startTime = DateTime.Now;
                while ((DateTime.Now - startTime).TotalSeconds < timeout)
                {
                    ConnectionStatus status = GetConnectionStatus();
                    if (status.Done)
                    {
                        if (status.Text == "Successfully connected to instrument server" || string.IsNullOrEmpty(status.Text))
                        {
                            _connected = true;
                            return true;
                        }
                        else
                        {
                            throw new InvalidOperationException("Connection failed: " + status.Text);
                        }
                    }
                    Thread.Sleep(1000);
                }

                throw new TimeoutException("Connection timeout after " + timeout + " seconds");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Connection failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Disconnect from instrument
        /// </summary>
        public bool Disconnect()
        {
            if (_instrument != null && _connected)
            {
                try
                {
                    _instrument.GetType().InvokeMember(
                        "Disconnect",
                        BindingFlags.InvokeMethod,
                        null,
                        _instrument,
                        null
                    );
                    _connected = false;
                    return true;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Disconnect failed: " + ex.Message, ex);
                }
            }
            return false;
        }

        /// <summary>
        /// Get current connection status
        /// </summary>
        public ConnectionStatus GetConnectionStatus()
        {
            ConnectionStatus status = new ConnectionStatus();

            if (_instrument == null)
            {
                status.Done = true;
                status.Text = "No instrument object";
                status.ErrorCode = -1;
                return status;
            }

            try
            {
                object connectionStatus = _instrument.GetType().InvokeMember(
                    "ConnectionStatus",
                    BindingFlags.GetProperty,
                    null,
                    _instrument,
                    null
                );

                object doneProperty = connectionStatus.GetType().InvokeMember(
                    "Done",
                    BindingFlags.GetProperty,
                    null,
                    connectionStatus,
                    null
                );

                object textProperty = connectionStatus.GetType().InvokeMember(
                    "Text",
                    BindingFlags.GetProperty,
                    null,
                    connectionStatus,
                    null
                );

                status.Done = (bool)doneProperty;
                status.Text = textProperty.ToString();
                
                // Try to get error code if available
                try
                {
                    object errorCodeProperty = connectionStatus.GetType().InvokeMember(
                        "ErrorCode",
                        BindingFlags.GetProperty,
                        null,
                        connectionStatus,
                        null
                    );
                    status.ErrorCode = Convert.ToInt32(errorCodeProperty);
                }
                catch
                {
                    status.ErrorCode = 0;
                }

                return status;
            }
            catch (Exception)
            {
                status.Done = true;
                status.Text = "Status unavailable";
                status.ErrorCode = -1;
                return status;
            }
        }

        /// <summary>
        /// Check if connected to instrument
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (_instrument == null)
                    return false;

                try
                {
                    object result = _instrument.GetType().InvokeMember(
                        "IsConnected",
                        BindingFlags.GetProperty,
                        null,
                        _instrument,
                        null
                    );
                    return (bool)result;
                }
                catch (Exception)
                {
                    return _connected;
                }
            }
        }

        /// <summary>
        /// Get available sample set methods
        /// </summary>
        public string[] SampleSetMethods
        {
            get
            {
                if (_instrument == null)
                    return new string[0];

                try
                {
                    object methodsObj = _instrument.GetType().InvokeMember(
                        "SampleSetMethods",
                        BindingFlags.GetProperty,
                        null,
                        _instrument,
                        null
                    );

                    if (methodsObj != null && methodsObj != System.DBNull.Value)
                    {
                        return (string[])methodsObj;
                    }
                    return new string[0];
                }
                catch (Exception)
                {
                    return new string[0];
                }
            }
        }

        /// <summary>
        /// Execute sample set with specified method
        /// </summary>
        public bool ReplaceSampleSet(string methodName)
        {
            if (_instrument == null)
                throw new InvalidOperationException("Instrument object not created");

            if (!_connected)
                throw new InvalidOperationException("Not connected to instrument");

            try
            {
                _instrument.GetType().InvokeMember(
                    "Replace",
                    BindingFlags.InvokeMethod,
                    null,
                    _instrument,
                    new object[] { methodName }
                );
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Sample set execution failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Stop current operation
        /// </summary>
        public bool Stop()
        {
            if (_instrument == null || !_connected)
                return false;

            try
            {
                _instrument.GetType().InvokeMember(
                    "Stop",
                    BindingFlags.InvokeMethod,
                    null,
                    _instrument,
                    null
                );
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Stop failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Pause current operation
        /// </summary>
        public bool Pause()
        {
            if (_instrument == null || !_connected)
                return false;

            try
            {
                _instrument.GetType().InvokeMember(
                    "Pause",
                    BindingFlags.InvokeMethod,
                    null,
                    _instrument,
                    null
                );
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Pause failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Resume paused operation
        /// </summary>
        public bool Resume()
        {
            if (_instrument == null || !_connected)
                return false;

            try
            {
                _instrument.GetType().InvokeMember(
                    "Resume",
                    BindingFlags.InvokeMethod,
                    null,
                    _instrument,
                    null
                );
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Resume failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Add sample set to queue
        /// </summary>
        public bool QueueSampleSet(string sampleSetName)
        {
            if (_instrument == null || !_connected)
                throw new InvalidOperationException("Instrument not connected");

            try
            {
                _instrument.GetType().InvokeMember(
                    "QueueSampleSet",
                    BindingFlags.InvokeMethod,
                    null,
                    _instrument,
                    new object[] { sampleSetName }
                );
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Queue operation failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Start queue processing
        /// </summary>
        public bool StartQueue()
        {
            if (_instrument == null || !_connected)
                return false;

            try
            {
                _instrument.GetType().InvokeMember(
                    "StartQueue",
                    BindingFlags.InvokeMethod,
                    null,
                    _instrument,
                    null
                );
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Start queue failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Stop queue processing
        /// </summary>
        public bool StopQueue()
        {
            if (_instrument == null || !_connected)
                return false;

            try
            {
                _instrument.GetType().InvokeMember(
                    "StopQueue",
                    BindingFlags.InvokeMethod,
                    null,
                    _instrument,
                    null
                );
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Stop queue failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Clear all queued items
        /// </summary>
        public bool ClearQueue()
        {
            if (_instrument == null || !_connected)
                return false;

            try
            {
                _instrument.GetType().InvokeMember(
                    "ClearQueue",
                    BindingFlags.InvokeMethod,
                    null,
                    _instrument,
                    null
                );
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Clear queue failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Get current instrument status
        /// </summary>
        public string Status
        {
            get
            {
                if (_instrument == null || !_connected)
                    return "Not connected";

                try
                {
                    object statusObj = _instrument.GetType().InvokeMember(
                        "Status",
                        BindingFlags.GetProperty,
                        null,
                        _instrument,
                        null
                    );
                    return statusObj.ToString();
                }
                catch (Exception)
                {
                    return "Status unavailable";
                }
            }
        }

        /// <summary>
        /// Get progress information for current operation
        /// </summary>
        public string Progress
        {
            get
            {
                if (_instrument == null || !_connected)
                    return "Not connected";

                try
                {
                    object progressObj = _instrument.GetType().InvokeMember(
                        "Progress",
                        BindingFlags.GetProperty,
                        null,
                        _instrument,
                        null
                    );
                    return progressObj.ToString();
                }
                catch (Exception)
                {
                    return "Progress unavailable";
                }
            }
        }

        /// <summary>
        /// Get the underlying COM object (advanced users only)
        /// </summary>
        public object ComObject
        {
            get { return _instrument; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        Disconnect();
                    }
                    catch { }
                }

                if (_instrument != null)
                {
                    try
                    {
                        Marshal.ReleaseComObject(_instrument);
                    }
                    catch { }
                    _instrument = null;
                }

                _disposed = true;
            }
        }

        ~EmpowerInstrument()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Wrapper for MillenniumToolkit.SampleSetMethod COM object
    /// </summary>
    public class EmpowerSampleSetMethod : IDisposable
    {
        private object _sampleSetMethod;
        private bool _disposed;

        public EmpowerSampleSetMethod()
        {
            _sampleSetMethod = null;
            _disposed = false;
        }

        /// <summary>
        /// Create the SampleSetMethod COM object
        /// </summary>
        public bool Create()
        {
            try
            {
                Type sampleSetMethodType = Type.GetTypeFromProgID("MillenniumToolkit.SampleSetMethod");
                _sampleSetMethod = Activator.CreateInstance(sampleSetMethodType);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create SampleSetMethod object: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Get available sample set method names
        /// </summary>
        public string[] MethodNames
        {
            get
            {
                if (_sampleSetMethod == null)
                    return new string[0];

                try
                {
                    object methodsObj = _sampleSetMethod.GetType().InvokeMember(
                        "SampleSetMethodNames",
                        BindingFlags.GetProperty,
                        null,
                        _sampleSetMethod,
                        null
                    );

                    if (methodsObj != null && methodsObj != System.DBNull.Value)
                    {
                        return (string[])methodsObj;
                    }
                    return new string[0];
                }
                catch (Exception)
                {
                    return new string[0];
                }
            }
        }

        /// <summary>
        /// Load specific method
        /// </summary>
        public bool Load(string methodName)
        {
            if (_sampleSetMethod == null)
                throw new InvalidOperationException("SampleSetMethod object not created");

            try
            {
                _sampleSetMethod.GetType().InvokeMember(
                    "Load",
                    BindingFlags.InvokeMethod,
                    null,
                    _sampleSetMethod,
                    new object[] { methodName }
                );
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Load method failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Save method
        /// </summary>
        public bool Save(string methodName)
        {
            if (_sampleSetMethod == null)
                throw new InvalidOperationException("SampleSetMethod object not created");

            try
            {
                _sampleSetMethod.GetType().InvokeMember(
                    "Save",
                    BindingFlags.InvokeMethod,
                    null,
                    _sampleSetMethod,
                    new object[] { methodName }
                );
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Save method failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Delete method
        /// </summary>
        public bool Delete(string methodName)
        {
            if (_sampleSetMethod == null)
                throw new InvalidOperationException("SampleSetMethod object not created");

            try
            {
                _sampleSetMethod.GetType().InvokeMember(
                    "Delete",
                    BindingFlags.InvokeMethod,
                    null,
                    _sampleSetMethod,
                    new object[] { methodName }
                );
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Delete method failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Get the underlying COM object (advanced users only)
        /// </summary>
        public object ComObject
        {
            get { return _sampleSetMethod; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_sampleSetMethod != null)
                {
                    try
                    {
                        Marshal.ReleaseComObject(_sampleSetMethod);
                    }
                    catch { }
                    _sampleSetMethod = null;
                }

                _disposed = true;
            }
        }

        ~EmpowerSampleSetMethod()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Main toolkit class combining all Empower functionality
    /// </summary>
    public class EmpowerToolkit : IDisposable
    {
        private EmpowerProject _project;
        private EmpowerInstrument _instrument;
        private EmpowerSampleSetMethod _sampleSetMethod;
        private Dictionary<string, string> _config;
        private bool _disposed;

        public EmpowerToolkit(string configFile)
        {
            InitializeToolkit(configFile);
        }

        public EmpowerToolkit()
        {
            InitializeToolkit("secrets.ini");
        }

        private void InitializeToolkit(string configFile)
        {
            _config = LoadConfig(configFile);
            _project = new EmpowerProject();
            _instrument = new EmpowerInstrument();
            _sampleSetMethod = new EmpowerSampleSetMethod();
            _disposed = false;
        }

        /// <summary>
        /// Get the Project wrapper
        /// </summary>
        public EmpowerProject Project
        {
            get { return _project; }
        }

        /// <summary>
        /// Get the Instrument wrapper
        /// </summary>
        public EmpowerInstrument Instrument
        {
            get { return _instrument; }
        }

        /// <summary>
        /// Get the SampleSetMethod wrapper
        /// </summary>
        public EmpowerSampleSetMethod SampleSetMethod
        {
            get { return _sampleSetMethod; }
        }

        /// <summary>
        /// Get configuration settings
        /// </summary>
        public Dictionary<string, string> Config
        {
            get { return new Dictionary<string, string>(_config); }
        }

        /// <summary>
        /// Initialize all COM objects and login
        /// </summary>
        public bool Initialize()
        {
            try
            {
                // Create COM objects
                _project.Create();
                _instrument.Create();
                _sampleSetMethod.Create();

                // Login to project
                string database = _config.ContainsKey("database") ? _config["database"] : "";
                string project = _config.ContainsKey("project") ? _config["project"] : "Waters GPC Training";
                string username = _config.ContainsKey("username") ? _config["username"] : "system";
                string password = _config.ContainsKey("password") ? _config["password"] : "manager";

                _project.Login(database, project, username, password);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Initialization failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Connect to instrument using config settings
        /// </summary>
        public bool ConnectInstrument()
        {
            try
            {
                string node = _config.ContainsKey("node") ? _config["node"] : "Waters-h4q6k34";
                string system = _config.ContainsKey("system") ? _config["system"] : "Arc HPLC";

                return _instrument.Connect(node, system);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Instrument connection failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Discover available systems, nodes, and methods
        /// </summary>
        public Dictionary<string, string[]> DiscoverSystems()
        {
            Dictionary<string, string[]> discovery = new Dictionary<string, string[]>();
            
            discovery["systems"] = _instrument.Systems;
            discovery["nodes"] = _instrument.AcquisitionServers;
            discovery["methods"] = _sampleSetMethod.MethodNames;
            
            return discovery;
        }

        /// <summary>
        /// Run comprehensive diagnostics
        /// </summary>
        public string RunDiagnostics()
        {
            List<string> report = new List<string>();
            
            report.Add("Waters Empower Toolkit - Comprehensive Diagnostics");
            report.Add("===================================================");
            report.Add("Timestamp: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            report.Add("");
            
            // Configuration
            report.Add("Configuration:");
            foreach (KeyValuePair<string, string> kvp in _config)
            {
                string value = kvp.Key == "password" ? new string('*', kvp.Value.Length) : kvp.Value;
                report.Add("  " + kvp.Key + ": " + value);
            }
            report.Add("");
            
            // Test authentication
            bool authSuccess = false;
            try
            {
                if (_project.IsConnected)
                {
                    authSuccess = true;
                    report.Add("✅ Authentication Test: PASS");
                }
                else
                {
                    report.Add("❌ Authentication Test: FAIL - Not logged in");
                }
            }
            catch (Exception ex)
            {
                report.Add("❌ Authentication Test: FAIL - " + ex.Message);
            }
            report.Add("");
            
            // Test discovery
            bool discoverySuccess = false;
            try
            {
                Dictionary<string, string[]> discovery = DiscoverSystems();
                discoverySuccess = true;
                
                report.Add("✅ System Discovery: PASS");
                report.Add("  Systems found: " + discovery["systems"].Length);
                report.Add("  Nodes found: " + discovery["nodes"].Length);
                report.Add("  Methods found: " + discovery["methods"].Length);
                
                if (discovery["systems"].Length > 0)
                {
                    report.Add("  Available Systems:");
                    foreach (string system in discovery["systems"])
                    {
                        report.Add("    - " + system);
                    }
                }
                
                if (discovery["nodes"].Length > 0)
                {
                    report.Add("  Available Nodes:");
                    foreach (string node in discovery["nodes"])
                    {
                        report.Add("    - " + node);
                    }
                }
            }
            catch (Exception ex)
            {
                report.Add("❌ System Discovery: FAIL - " + ex.Message);
            }
            report.Add("");
            
            // Test connection
            bool connectionSuccess = false;
            try
            {
                if (_instrument.IsConnected)
                {
                    connectionSuccess = true;
                    report.Add("✅ Connection Test: PASS");
                    report.Add("  Status: " + _instrument.Status);
                }
                else
                {
                    report.Add("❌ Connection Test: FAIL - Not connected to instrument");
                }
            }
            catch (Exception ex)
            {
                report.Add("❌ Connection Test: FAIL - " + ex.Message);
            }
            report.Add("");
            
            // Summary
            int totalTests = 3;
            int passedTests = (authSuccess ? 1 : 0) + (discoverySuccess ? 1 : 0) + (connectionSuccess ? 1 : 0);
            
            report.Add("Summary: " + passedTests + "/" + totalTests + " tests passed");
            
            if (passedTests == totalTests)
            {
                report.Add("🎉 All tests passed! Empower Toolkit is fully functional.");
            }
            else if (passedTests > 0)
            {
                report.Add("⚠ Partial success. Some functionality available.");
            }
            else
            {
                report.Add("❌ All tests failed. Check Empower installation and configuration.");
            }
            
            return string.Join(Environment.NewLine, report.ToArray());
        }

        /// <summary>
        /// Cleanup all connections
        /// </summary>
        public void Cleanup()
        {
            try
            {
                if (_instrument != null)
                    _instrument.Disconnect();
                
                if (_project != null)
                    _project.Logoff();
            }
            catch (Exception)
            {
                // Ignore cleanup errors
            }
        }

        /// <summary>
        /// Load configuration from file
        /// </summary>
        private static Dictionary<string, string> LoadConfig(string configFile)
        {
            Dictionary<string, string> config = new Dictionary<string, string>();

            // Default configuration
            config["username"] = "system";
            config["password"] = "manager";
            config["database"] = "";
            config["project"] = "Waters GPC Training";
            config["system"] = "Arc HPLC";
            config["node"] = "Waters-h4q6k34";

            if (!File.Exists(configFile))
                return config;

            try
            {
                foreach (string line in File.ReadAllLines(configFile))
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("#") || trimmed.StartsWith("[") || string.IsNullOrEmpty(trimmed))
                        continue;

                    string[] parts = trimmed.Split('=');
                    if (parts.Length == 2)
                    {
                        config[parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }
            catch (Exception)
            {
                // Return defaults if config loading fails
            }

            return config;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Cleanup();

                    if (_project != null)
                    {
                        _project.Dispose();
                        _project = null;
                    }

                    if (_instrument != null)
                    {
                        _instrument.Dispose();
                        _instrument = null;
                    }

                    if (_sampleSetMethod != null)
                    {
                        _sampleSetMethod.Dispose();
                        _sampleSetMethod = null;
                    }
                }

                _disposed = true;
            }
        }

        ~EmpowerToolkit()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Static helper class for quick operations
    /// </summary>
    public static class EmpowerHelper
    {
        /// <summary>
        /// Quick function to discover available Empower systems
        /// </summary>
        public static Dictionary<string, string[]> DiscoverSystems(string configFile)
        {
            using (EmpowerToolkit toolkit = new EmpowerToolkit(configFile))
            {
                toolkit.Initialize();
                return toolkit.DiscoverSystems();
            }
        }

        /// <summary>
        /// Quick function to discover available Empower systems with default config
        /// </summary>
        public static Dictionary<string, string[]> DiscoverSystems()
        {
            using (EmpowerToolkit toolkit = new EmpowerToolkit("secrets.ini"))
            {
                toolkit.Initialize();
                return toolkit.DiscoverSystems();
            }
        }

        /// <summary>
        /// Quick function to execute a sample set method
        /// </summary>
        public static bool ExecuteSampleSet(string methodName, string configFile)
        {
            using (EmpowerToolkit toolkit = new EmpowerToolkit(configFile))
            {
                toolkit.Initialize();
                if (toolkit.ConnectInstrument())
                {
                    return toolkit.Instrument.ReplaceSampleSet(methodName);
                }
                return false;
            }
        }

        /// <summary>
        /// Quick function to execute a sample set method with default config
        /// </summary>
        public static bool ExecuteSampleSet(string methodName)
        {
            using (EmpowerToolkit toolkit = new EmpowerToolkit("secrets.ini"))
            {
                toolkit.Initialize();
                if (toolkit.ConnectInstrument())
                {
                    return toolkit.Instrument.ReplaceSampleSet(methodName);
                }
                return false;
            }
        }

        /// <summary>
        /// Quick function to run diagnostics
        /// </summary>
        public static string RunDiagnostics(string configFile)
        {
            using (EmpowerToolkit toolkit = new EmpowerToolkit(configFile))
            {
                try
                {
                    toolkit.Initialize();
                    return toolkit.RunDiagnostics();
                }
                catch (Exception ex)
                {
                    return "Diagnostics failed: " + ex.Message;
                }
            }
        }

        /// <summary>
        /// Quick function to run diagnostics with default config
        /// </summary>
        public static string RunDiagnostics()
        {
            using (EmpowerToolkit toolkit = new EmpowerToolkit("secrets.ini"))
            {
                try
                {
                    toolkit.Initialize();
                    return toolkit.RunDiagnostics();
                }
                catch (Exception ex)
                {
                    return "Diagnostics failed: " + ex.Message;
                }
            }
        }
    }
}
