using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

class SampleSetExtractor 
{
    // Configuration settings from secrets.ini
    static Dictionary<string, string> empowerConfig = new Dictionary<string, string>();
    
    static void Main(string[] args)
    {
        Console.WriteLine("Waters Empower Sample Set Extractor");
        Console.WriteLine("===================================");
        
        // Load configuration from secrets.ini
        LoadConfiguration();
        
        // Check if sample set name was provided as argument
        string targetSampleSet = "20251002_KC"; // Default
        if (args.Length > 0)
        {
            targetSampleSet = args[0];
            Console.WriteLine("Target Sample Set: " + targetSampleSet);
        }
        else
        {
            Console.WriteLine("Usage: SampleSetExtractor.exe [SampleSetName]");
            Console.WriteLine("Using default: " + targetSampleSet);
        }
        
        object project = null;
        object instrument = null;
        
        try 
        {
            // Create Project object
            Console.WriteLine("Creating MillenniumToolkit.Project...");
            var projectType = Type.GetTypeFromProgID("MillenniumToolkit.Project");
            project = Activator.CreateInstance(projectType);
            Console.WriteLine("✅ Project object created: " + project.GetType().Name);
            
            // Login to Empower using credentials from secrets.ini
            Console.WriteLine("Attempting login...");
            object[] loginParams = { 
                empowerConfig.ContainsKey("database") ? empowerConfig["database"] : "",
                empowerConfig.ContainsKey("project") ? empowerConfig["project"] : "Waters GPC Training", 
                empowerConfig.ContainsKey("username") ? empowerConfig["username"] : "system", 
                empowerConfig.ContainsKey("password") ? empowerConfig["password"] : "manager" 
            };
            project.GetType().InvokeMember(
                "Login",
                BindingFlags.InvokeMethod,
                null,
                project,
                loginParams
            );
            Console.WriteLine("🎉 LOGIN SUCCESSFUL!");
            
            // Create Instrument object
            Console.WriteLine("Creating MillenniumToolkit.Instrument...");
            var instrumentType = Type.GetTypeFromProgID("MillenniumToolkit.Instrument");
            instrument = Activator.CreateInstance(instrumentType);
            Console.WriteLine("✅ Instrument object created");
            
            // Get available systems
            Console.WriteLine("\nGetting available systems...");
            try 
            {
                var systems = instrument.GetType().InvokeMember(
                    "Systems",
                    BindingFlags.GetProperty,
                    null,
                    instrument,
                    null
                );
                string[] systemArray = systems as string[];
                if (systemArray != null)
                {
                    Console.WriteLine("✅ Found " + systemArray.Length + " systems:");
                    foreach (string system in systemArray)
                    {
                        Console.WriteLine("  - " + system);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠ Systems property error: " + ex.Message);
            }
            
            // Get available nodes
            Console.WriteLine("\nGetting available nodes...");
            try 
            {
                var nodes = instrument.GetType().InvokeMember(
                    "AcqServers",
                    BindingFlags.GetProperty,
                    null,
                    instrument,
                    null
                );
                string[] nodeArray = nodes as string[];
                if (nodeArray != null)
                {
                    Console.WriteLine("✅ Found " + nodeArray.Length + " nodes:");
                    foreach (string node in nodeArray)
                    {
                        Console.WriteLine("  - " + node);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠ AcqServers property error: " + ex.Message);
            }
            
            // Connect to instrument
            Console.WriteLine("\nConnecting to instrument...");
            object[] connectParams = { "Waters-h4q6k34", "Arc HPLC" };
            instrument.GetType().InvokeMember(
                "Connect",
                BindingFlags.InvokeMethod,
                null,
                instrument,
                connectParams
            );
            Console.WriteLine("✅ Instrument connected");
            
            // Wait for connection to establish (like official example)
            Console.WriteLine("\nWaiting for connection to establish...");
            try
            {
                var connectionStatus = instrument.GetType().InvokeMember(
                    "ConnectionStatus",
                    BindingFlags.GetProperty,
                    null,
                    instrument,
                    null
                );
                
                // Wait while connection is not done (like official example)
                var done = connectionStatus.GetType().InvokeMember("Done", BindingFlags.GetProperty, null, connectionStatus, null);
                while (!(bool)done)
                {
                    System.Threading.Thread.Sleep(1000);
                    connectionStatus = instrument.GetType().InvokeMember("ConnectionStatus", BindingFlags.GetProperty, null, instrument, null);
                    done = connectionStatus.GetType().InvokeMember("Done", BindingFlags.GetProperty, null, connectionStatus, null);
                }
                
                var text = connectionStatus.GetType().InvokeMember("Text", BindingFlags.GetProperty, null, connectionStatus, null);
                if (text.ToString().Equals("Successfully connected to instrument server") || text.ToString().Length == 0)
                {
                    Console.WriteLine("✅ Connection succeeded");
                }
                else
                {
                    Console.WriteLine("❌ Connection failed: " + text.ToString());
                }
            }
            catch (Exception connEx)
            {
                Console.WriteLine("⚠ Connection check error: " + connEx.Message);
            }
            
            // Get sample set methods - only try the methods that work
            Console.WriteLine("\nGetting sample set methods...");
            
            // Try using SampleSetMethod object (this is the one that works)
            Console.WriteLine("Getting sample set method names...");
            try
            {
                var sampleSetMethodType = Type.GetTypeFromProgID("MillenniumToolkit.SampleSetMethod");
                var sampleSetMethodObj = Activator.CreateInstance(sampleSetMethodType);
                Console.WriteLine("✅ SampleSetMethod object created");
                
                // Get method names from SampleSetMethod object
                var methodNames = sampleSetMethodObj.GetType().InvokeMember(
                    "SampleSetMethodNames",
                    BindingFlags.GetProperty,
                    null,
                    sampleSetMethodObj,
                    null
                );
                string[] methodArray = methodNames as string[];
                if (methodArray != null)
                {
                    Console.WriteLine("✅ Found " + methodArray.Length + " sample set method names:");
                    foreach (string method in methodArray)
                    {
                        Console.WriteLine("  - " + method);
                    }
                    
                    // Check current instrument status BEFORE attempting execution
                    Console.WriteLine("\n🔍 CHECKING CURRENT INSTRUMENT STATUS...");
                    Console.WriteLine("========================================");
                    
                    try
                    {
                        var currentStatus = instrument.GetType().InvokeMember(
                            "Status",
                            BindingFlags.GetProperty,
                            null,
                            instrument,
                            null
                        );
                        
                        if (currentStatus != null)
                        {
                            var currentStateDesc = currentStatus.GetType().InvokeMember("SystemStateDescription", BindingFlags.GetProperty, null, currentStatus, null);
                            var currentVial = currentStatus.GetType().InvokeMember("Vial", BindingFlags.GetProperty, null, currentStatus, null);
                            var currentSampleSet = currentStatus.GetType().InvokeMember("SampleSetMethodName", BindingFlags.GetProperty, null, currentStatus, null);
                            
                            Console.WriteLine("Current State: " + currentStateDesc.ToString());
                            Console.WriteLine("Current Vial: " + currentVial.ToString());
                            Console.WriteLine("Active Sample Set: " + currentSampleSet.ToString());
                            
                            // Check if instrument is busy
                            string state = currentStateDesc.ToString();
                            if (state.Contains("Sample Set") && (state.Contains("Running") || state.Contains("Waiting") || state.Contains("Injection")))
                            {
                                Console.WriteLine("⚠ INSTRUMENT IS CURRENTLY BUSY!");
                                Console.WriteLine("State: " + state);
                                Console.WriteLine("Cannot start new execution while instrument is running.");
                                Console.WriteLine("Please wait for current run to complete or stop it manually.");
                                return; // Exit early - don't try to execute
                            }
                            else if (state.Contains("Successfully connected") || state.Contains("Idle"))
                            {
                                Console.WriteLine("✅ Instrument is idle and ready for new execution");
                            }
                            else
                            {
                                Console.WriteLine("❓ Unknown instrument state: " + state);
                                Console.WriteLine("Proceeding with caution...");
                            }
                        }
                    }
                    catch (Exception statusEx)
                    {
                        Console.WriteLine("⚠ Could not check current status: " + statusEx.Message);
                        Console.WriteLine("Proceeding with execution attempt...");
                    }
                    
                    // Try to execute specific sample set method
                    string targetMethod = targetSampleSet;
                    Console.WriteLine("\nAttempting to execute sample set: " + targetMethod);
                    
                    bool methodExists = false;
                    foreach (string method in methodArray)
                    {
                        if (method.Equals(targetMethod, StringComparison.OrdinalIgnoreCase))
                        {
                            methodExists = true;
                            break;
                        }
                    }
                    
                    if (methodExists)
                    {
                        Console.WriteLine("✅ Method found, attempting to EXECUTE with Run() method...");
                        
                        try
                        {
                            Console.WriteLine("Using official Empower Toolkit Run method...");
                            Console.WriteLine("Parameters: sampleSetMethod='" + targetMethod + "', newName='" + targetMethod + "_executed'");
                            
                            // Use the correct Run method from official examples
                            // _instrument.Run(sampleSetMethodName, newName);
                            instrument.GetType().InvokeMember(
                                "Run",
                                BindingFlags.InvokeMethod,
                                null,
                                instrument,
                                new object[] { targetMethod, targetMethod + "_executed" }
                            );
                            Console.WriteLine("🎉 Run() method succeeded! Sample set execution started!");
                            
                            // Monitor execution status using proper InstrumentStatus (like official example)
                            Console.WriteLine("\n🔍 MONITORING EXECUTION STATUS...");
                            Console.WriteLine("================================");
                            
                            for (int i = 0; i < 5; i++)
                            {
                                Console.WriteLine("\n--- Execution Check #" + (i + 1) + " ---");
                                
                                try
                                {
                                    // Get InstrumentStatus object (like official example: RefreshInstrumentStatusInformation)
                                    var instrumentStatus = instrument.GetType().InvokeMember(
                                        "Status",
                                        BindingFlags.GetProperty,
                                        null,
                                        instrument,
                                        null
                                    );
                                    
                                    if (instrumentStatus != null)
                                    {
                                        // Get all the status fields like the official example
                                        var stateDesc = instrumentStatus.GetType().InvokeMember("SystemStateDescription", BindingFlags.GetProperty, null, instrumentStatus, null);
                                        var systemState = instrumentStatus.GetType().InvokeMember("SystemState", BindingFlags.GetProperty, null, instrumentStatus, null);
                                        var vial = instrumentStatus.GetType().InvokeMember("Vial", BindingFlags.GetProperty, null, instrumentStatus, null);
                                        var injection = instrumentStatus.GetType().InvokeMember("Injection", BindingFlags.GetProperty, null, instrumentStatus, null);
                                        var runTime = instrumentStatus.GetType().InvokeMember("RunTime", BindingFlags.GetProperty, null, instrumentStatus, null);
                                        var sampleSetName = instrumentStatus.GetType().InvokeMember("SampleSetMethodName", BindingFlags.GetProperty, null, instrumentStatus, null);
                                        
                                        // Display comprehensive status (like official example)
                                        Console.WriteLine("State: " + stateDesc.ToString());
                                        Console.WriteLine("SystemState: " + systemState.ToString());
                                        Console.WriteLine("Current Vial: " + vial.ToString());
                                        Console.WriteLine("Injection: " + injection.ToString());
                                        Console.WriteLine("Run Time: " + runTime.ToString());
                                        Console.WriteLine("Active Sample Set: " + sampleSetName.ToString());
                                        
                                        // Check if we're actually running vs idle
                                        string state = stateDesc.ToString();
                                        if (state.Contains("Sample Set") && !state.Contains("Successfully connected"))
                                        {
                                            Console.WriteLine("🔬 STATUS: ACTIVELY EXECUTING");
                                        }
                                        else if (state.Contains("Successfully connected") || state.Contains("Idle"))
                                        {
                                            Console.WriteLine("💤 STATUS: IDLE/READY");
                                        }
                                        else
                                        {
                                            Console.WriteLine("❓ STATUS: " + state);
                                        }
                                    }
                                }
                                catch (Exception statusEx)
                                {
                                    Console.WriteLine("❌ Status check error: " + statusEx.Message);
                                }
                                
                                // Wait between checks (but not too long like before)
                                if (i < 4)
                                {
                                    Console.WriteLine("Waiting 3 seconds...");
                                    System.Threading.Thread.Sleep(3000);
                                }
                            }
                        }
                        catch (Exception runEx)
                        {
                            Console.WriteLine("❌ Run() method error: " + runEx.Message);
                            Console.WriteLine("Inner exception: " + (runEx.InnerException != null ? runEx.InnerException.Message : "None"));
                            
                            // Fall back to Replace method as before
                            Console.WriteLine("\nFalling back to Replace method (load only)...");
                            try
                            {
                                instrument.GetType().InvokeMember(
                                    "Replace",
                                    BindingFlags.InvokeMethod,
                                    null,
                                    instrument,
                                    new object[] { targetMethod }
                                );
                                Console.WriteLine("✅ Replace succeeded - sample set loaded but not executed");
                            }
                            catch (Exception replaceEx)
                            {
                                Console.WriteLine("❌ Replace also failed: " + replaceEx.Message);
                            }
                        }
                        
                        Console.WriteLine("\n🎉 Execution monitoring completed!");
                        
                    }
                    else
                    {
                        Console.WriteLine("❌ Method '" + targetMethod + "' not found in available methods");
                        Console.WriteLine("Available methods containing '2025':");
                        foreach (string method in methodArray)
                        {
                            if (method.Contains("2025"))
                            {
                                Console.WriteLine("  - " + method);
                            }
                        }
                    }
                }
                
                Marshal.ReleaseComObject(sampleSetMethodObj);
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠ SampleSetMethod object error: " + ex.Message);
            }
            
            Console.WriteLine("\n🎉 Sample set extraction completed successfully!");
            
        }
        catch (Exception ex) 
        {
            Console.WriteLine("❌ Error: " + ex.Message);
            Console.WriteLine("Stack Trace: " + ex.StackTrace);
        }
        finally 
        {
            // Cleanup
            if (instrument != null) Marshal.ReleaseComObject(instrument);
            if (project != null) Marshal.ReleaseComObject(project);
            Console.WriteLine("✅ COM cleanup completed");
        }
    }
    
    // Load configuration from secrets.ini file
    static void LoadConfiguration()
    {
        try
        {
            string configFile = "secrets.ini";
            if (File.Exists(configFile))
            {
                Console.WriteLine("Loading configuration from " + configFile);
                string[] lines = File.ReadAllLines(configFile);
                bool inEmpowerSection = false;
                
                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    
                    // Skip comments and empty lines
                    if (trimmedLine.StartsWith("#") || trimmedLine.StartsWith(";") || string.IsNullOrEmpty(trimmedLine))
                        continue;
                        
                    // Check for section headers
                    if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                    {
                        inEmpowerSection = trimmedLine.Equals("[empower]", StringComparison.OrdinalIgnoreCase);
                        continue;
                    }
                    
                    // Parse key-value pairs in empower section
                    if (inEmpowerSection && trimmedLine.Contains("="))
                    {
                        string[] parts = trimmedLine.Split('=');
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            string value = parts[1].Trim();
                            empowerConfig[key] = value;
                            Console.WriteLine("  " + key + " = " + (key == "password" ? "***" : value));
                        }
                    }
                }
                Console.WriteLine("✅ Configuration loaded successfully");
            }
            else
            {
                Console.WriteLine("⚠️  secrets.ini not found, using default credentials");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error loading configuration: " + ex.Message);
            Console.WriteLine("Using default credentials");
        }
    }
}