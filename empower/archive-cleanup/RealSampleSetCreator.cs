using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

class RealSampleSetCreator 
{
    // Configuration settings from secrets.ini
    static Dictionary<string, string> empowerConfig = new Dictionary<string, string>();
    
    static void Main(string[] args)
    {
        Console.WriteLine("Waters Empower Real Sample Set Creator");
        Console.WriteLine("=====================================");
        Console.WriteLine("This tool creates actual Sample Set instances (not just methods/templates)");
        
        // Load configuration from secrets.ini
        LoadConfiguration();
        
        // Parse command line arguments
        string newSampleSetName = null;
        string templateName = "202500606 NKG PS Standard"; // Default template
        string injectionVolume = null;
        string sampleName = "KC_Test_Sample";
        string vial = "1:A,1";
        string runtime = null;
        bool showHelp = false;
        bool executeImmediately = false;
        
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--help":
                case "-h":
                    showHelp = true;
                    break;
                case "--name":
                case "-n":
                    if (i + 1 < args.Length)
                        newSampleSetName = args[++i];
                    break;
                case "--template":
                case "-t":
                    if (i + 1 < args.Length)
                        templateName = args[++i];
                    break;
                case "--injection-volume":
                case "-v":
                    if (i + 1 < args.Length)
                        injectionVolume = args[++i];
                    break;
                case "--sample-name":
                case "-s":
                    if (i + 1 < args.Length)
                        sampleName = args[++i];
                    break;
                case "--vial":
                case "-p":
                    if (i + 1 < args.Length)
                        vial = args[++i];
                    break;
                case "--runtime":
                case "-r":
                    if (i + 1 < args.Length)
                        runtime = args[++i];
                    break;
                case "--execute":
                case "-e":
                    executeImmediately = true;
                    break;
                default:
                    // Support legacy positional arguments
                    if (!args[i].StartsWith("-"))
                    {
                        if (newSampleSetName == null)
                        {
                            newSampleSetName = args[i];
                        }
                    }
                    break;
            }
        }
        
        if (showHelp)
        {
            Console.WriteLine("Usage: RealSampleSetCreator.exe [options]");
            Console.WriteLine("Options:");
            Console.WriteLine("  --name <name>, -n <name>               New sample set name (required)");
            Console.WriteLine("  --template <name>, -t <name>           Template method to use (default: 202500606 NKG PS Standard)");
            Console.WriteLine("  --injection-volume <vol>, -v <vol>     Injection volume in µL");
            Console.WriteLine("  --sample-name <name>, -s <name>        Sample name (default: KC_Test_Sample)");
            Console.WriteLine("  --vial <pos>, -p <pos>                 Vial position (default: 1:A,1)");
            Console.WriteLine("  --runtime <time>, -r <time>            Runtime in minutes");
            Console.WriteLine("  --execute, -e                          Execute immediately after creation");
            Console.WriteLine("  --help, -h                             Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  RealSampleSetCreator.exe --name \"MyRealSet\" --execute");
            Console.WriteLine("  RealSampleSetCreator.exe -n \"TestSet\" -v 15.0 -s \"MySample\" -p \"1:A,2\"");
            return;
        }
        
        // Generate default name if none provided
        if (newSampleSetName == null)
        {
            newSampleSetName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_RealSet";
            Console.WriteLine("No sample set name specified, using: " + newSampleSetName);
        }
        
        Console.WriteLine("Configuration:");
        Console.WriteLine("  Sample Set Name: " + newSampleSetName);
        Console.WriteLine("  Template Method: " + templateName);
        Console.WriteLine("  Sample Name: " + sampleName);
        Console.WriteLine("  Vial Position: " + vial);
        if (injectionVolume != null) Console.WriteLine("  Injection Volume: " + injectionVolume + " µL");
        if (runtime != null) Console.WriteLine("  Runtime: " + runtime + " min");
        Console.WriteLine("  Execute Immediately: " + (executeImmediately ? "Yes" : "No"));
        
        object project = null;
        object instrument = null;
        
        try 
        {
            // Create Project object and login
            Console.WriteLine("\nCreating MillenniumToolkit.Project...");
            var projectType = Type.GetTypeFromProgID("MillenniumToolkit.Project");
            project = Activator.CreateInstance(projectType);
            Console.WriteLine("✅ Project object created");
            
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
            
            // Method 1: Try creating actual sample set instance using Instrument.Run method
            Console.WriteLine("\n🎯 METHOD 1: Creating Sample Set via Instrument Execution");
            Console.WriteLine("=========================================================");
            bool success1 = TryCreateViaInstrumentExecution(newSampleSetName, templateName, executeImmediately);
            
            if (!success1)
            {
                // Method 2: Try creating sample set instance using different COM approach
                Console.WriteLine("\n🎯 METHOD 2: Creating Sample Set via Alternative COM Objects");
                Console.WriteLine("============================================================");
                bool success2 = TryCreateViaAlternativeCOM(newSampleSetName, templateName, sampleName, vial, injectionVolume, runtime);
                
                if (!success2)
                {
                    // Method 3: Create method and then instantiate it
                    Console.WriteLine("\n🎯 METHOD 3: Create Method Then Instantiate");
                    Console.WriteLine("===========================================");
                    bool success3 = TryCreateMethodThenInstantiate(newSampleSetName, templateName, sampleName, vial, injectionVolume, runtime, executeImmediately);
                    
                    if (!success3)
                    {
                        Console.WriteLine("\n❌ All methods failed to create actual sample set instance");
                        Console.WriteLine("💡 Note: Your current SampleSetCreator creates methods/templates, not instances");
                        Console.WriteLine("💡 Actual sample set instances may only exist during execution");
                    }
                }
            }
            
        }
        catch (Exception ex) 
        {
            Console.WriteLine("❌ Error: " + ex.Message);
            Console.WriteLine("Stack Trace: " + ex.StackTrace);
        }
        finally 
        {
            // Cleanup
            if (instrument != null) 
            {
                try
                {
                    instrument.GetType().InvokeMember("Disconnect", BindingFlags.InvokeMethod, null, instrument, null);
                }
                catch { }
                Marshal.ReleaseComObject(instrument);
            }
            if (project != null) Marshal.ReleaseComObject(project);
            Console.WriteLine("✅ COM cleanup completed");
        }
    }
    
    static bool TryCreateViaInstrumentExecution(string sampleSetName, string templateName, bool execute)
    {
        try
        {
            Console.WriteLine("Connecting to instrument for direct execution...");
            
            // Create and connect to instrument
            var instrumentType = Type.GetTypeFromProgID("MillenniumToolkit.Instrument");
            var instrument = Activator.CreateInstance(instrumentType);
            
            string nodeName = empowerConfig.ContainsKey("node") ? empowerConfig["node"] : "Waters-h4q6k34";
            string systemName = empowerConfig.ContainsKey("system") ? empowerConfig["system"] : "Arc HPLC";
            
            object[] connectParams = { nodeName, systemName };
            instrument.GetType().InvokeMember("Connect", BindingFlags.InvokeMethod, null, instrument, connectParams);
            Console.WriteLine("✅ Connected to instrument");
            
            System.Threading.Thread.Sleep(1000);
            
            if (execute)
            {
                // Try to run the template method with a new name (this should create an instance)
                Console.WriteLine("Attempting to run method '" + templateName + "' as sample set '" + sampleSetName + "'...");
                
                try
                {
                    // Method: Run(methodName, newSampleSetName)
                    object[] runParams = { templateName, sampleSetName };
                    instrument.GetType().InvokeMember(
                        "Run",
                        BindingFlags.InvokeMethod,
                        null,
                        instrument,
                        runParams
                    );
                    
                    Console.WriteLine("✅ Sample set execution started!");
                    Console.WriteLine("🎯 Real sample set instance created: " + sampleSetName);
                    
                    // Check status to confirm
                    System.Threading.Thread.Sleep(2000);
                    var statusObj = instrument.GetType().InvokeMember("Status", BindingFlags.GetProperty, null, instrument, null);
                    if (statusObj != null)
                    {
                        try
                        {
                            var currentName = statusObj.GetType().InvokeMember("SampleSetName", BindingFlags.GetProperty, null, statusObj, null);
                            var currentMethod = statusObj.GetType().InvokeMember("SampleSetMethodName", BindingFlags.GetProperty, null, statusObj, null);
                            var state = statusObj.GetType().InvokeMember("SystemStateDescription", BindingFlags.GetProperty, null, statusObj, null);
                            
                            Console.WriteLine("📊 Current Status:");
                            Console.WriteLine("  Sample Set Instance: " + (currentName != null ? currentName.ToString() : "Unknown"));
                            Console.WriteLine("  Based on Method: " + (currentMethod != null ? currentMethod.ToString() : "Unknown"));
                            Console.WriteLine("  State: " + (state != null ? state.ToString() : "Unknown"));
                        }
                        catch (Exception statusEx)
                        {
                            Console.WriteLine("⚠️ Could not read detailed status: " + statusEx.Message);
                        }
                    }
                    
                    instrument.GetType().InvokeMember("Disconnect", BindingFlags.InvokeMethod, null, instrument, null);
                    Marshal.ReleaseComObject(instrument);
                    return true;
                }
                catch (Exception runEx)
                {
                    Console.WriteLine("❌ Run method failed: " + runEx.Message);
                    
                    // Try alternative: Replace method (loads for execution)
                    try
                    {
                        Console.WriteLine("Trying Replace method instead...");
                        instrument.GetType().InvokeMember(
                            "Replace",
                            BindingFlags.InvokeMethod,
                            null,
                            instrument,
                            new object[] { templateName }
                        );
                        Console.WriteLine("✅ Sample set loaded for execution (Replace method)");
                        Console.WriteLine("💡 This creates a sample set instance when executed");
                        
                        instrument.GetType().InvokeMember("Disconnect", BindingFlags.InvokeMethod, null, instrument, null);
                        Marshal.ReleaseComObject(instrument);
                        return true;
                    }
                    catch (Exception replaceEx)
                    {
                        Console.WriteLine("❌ Replace method also failed: " + replaceEx.Message);
                    }
                }
            }
            else
            {
                Console.WriteLine("🔍 Execute flag not set, skipping actual execution");
                Console.WriteLine("💡 Use --execute flag to create actual sample set instance");
            }
            
            instrument.GetType().InvokeMember("Disconnect", BindingFlags.InvokeMethod, null, instrument, null);
            Marshal.ReleaseComObject(instrument);
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Method 1 failed: " + ex.Message);
            return false;
        }
    }
    
    static bool TryCreateViaAlternativeCOM(string sampleSetName, string templateName, string sampleName, string vial, string injectionVolume, string runtime)
    {
        Console.WriteLine("Trying alternative COM objects for sample set creation...");
        
        // Try different COM objects that might create instances vs methods
        string[] alternativeObjects = {
            "MillenniumToolkit.SampleSet",
            "MillenniumToolkit.SampleSetInstance", 
            "MillenniumToolkit.SampleSetExecutor",
            "MillenniumToolkit.SampleSetRunner"
        };
        
        foreach (string objName in alternativeObjects)
        {
            try
            {
                Console.WriteLine("Trying " + objName + "...");
                var objType = Type.GetTypeFromProgID(objName);
                if (objType != null)
                {
                    var obj = Activator.CreateInstance(objType);
                    Console.WriteLine("✅ " + objName + " created successfully");
                    
                    // Try to create sample set instance
                    try
                    {
                        // Try different methods for creation
                        string[] creationMethods = { "Create", "CreateInstance", "New", "Add", "CreateSampleSet" };
                        
                        foreach (string method in creationMethods)
                        {
                            try
                            {
                                obj.GetType().InvokeMember(
                                    method,
                                    BindingFlags.InvokeMethod,
                                    null,
                                    obj,
                                    new object[] { sampleSetName }
                                );
                                Console.WriteLine("✅ Successfully called " + method + " on " + objName);
                                Marshal.ReleaseComObject(obj);
                                return true;
                            }
                            catch
                            {
                                // Continue trying other methods
                            }
                        }
                    }
                    catch (Exception createEx)
                    {
                        Console.WriteLine("❌ Failed to create with " + objName + ": " + createEx.Message);
                    }
                    
                    Marshal.ReleaseComObject(obj);
                }
                else
                {
                    Console.WriteLine("❌ " + objName + " not available");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ " + objName + " failed: " + ex.Message);
            }
        }
        
        return false;
    }
    
    static bool TryCreateMethodThenInstantiate(string sampleSetName, string templateName, string sampleName, string vial, string injectionVolume, string runtime, bool execute)
    {
        Console.WriteLine("Creating method first, then trying to instantiate it...");
        
        try
        {
            // First create the method (like your current SampleSetCreator does)
            var sampleSetMethodType = Type.GetTypeFromProgID("MillenniumToolkit.SampleSetMethod");
            var sampleSetMethodObj = Activator.CreateInstance(sampleSetMethodType);
            
            // Load template
            sampleSetMethodObj.GetType().InvokeMember("Name", BindingFlags.SetProperty, null, sampleSetMethodObj, new object[] { templateName });
            sampleSetMethodObj.GetType().InvokeMember("Fetch", BindingFlags.InvokeMethod, null, sampleSetMethodObj, null);
            
            // Change name
            sampleSetMethodObj.GetType().InvokeMember("Name", BindingFlags.SetProperty, null, sampleSetMethodObj, new object[] { sampleSetName });
            
            // Modify sample line
            var sampleSetLines = sampleSetMethodObj.GetType().InvokeMember("SampleSetLines", BindingFlags.GetProperty, null, sampleSetMethodObj, null);
            if (sampleSetLines != null)
            {
                var sampleLine = sampleSetLines.GetType().InvokeMember("Item", BindingFlags.InvokeMethod | BindingFlags.GetProperty, null, sampleSetLines, new object[] { 0 });
                if (sampleLine != null)
                {
                    // Update sample properties
                    sampleLine.GetType().InvokeMember("Set", BindingFlags.InvokeMethod, null, sampleLine, new object[] { "SampleName", sampleName });
                    sampleLine.GetType().InvokeMember("Set", BindingFlags.InvokeMethod, null, sampleLine, new object[] { "Vial", vial });
                    if (injectionVolume != null)
                        sampleLine.GetType().InvokeMember("Set", BindingFlags.InvokeMethod, null, sampleLine, new object[] { "InjVol", injectionVolume });
                    if (runtime != null)
                        sampleLine.GetType().InvokeMember("Set", BindingFlags.InvokeMethod, null, sampleLine, new object[] { "Runtime", runtime });
                }
            }
            
            // Store the method
            sampleSetMethodObj.GetType().InvokeMember("Store", BindingFlags.InvokeMethod, null, sampleSetMethodObj, null);
            Console.WriteLine("✅ Sample set method '" + sampleSetName + "' created");
            
            if (execute)
            {
                Console.WriteLine("Now trying to execute the method to create instance...");
                
                // Connect to instrument and execute
                var instrumentType = Type.GetTypeFromProgID("MillenniumToolkit.Instrument");
                var instrument = Activator.CreateInstance(instrumentType);
                
                string nodeName = empowerConfig.ContainsKey("node") ? empowerConfig["node"] : "Waters-h4q6k34";
                string systemName = empowerConfig.ContainsKey("system") ? empowerConfig["system"] : "Arc HPLC";
                
                object[] connectParams = { nodeName, systemName };
                instrument.GetType().InvokeMember("Connect", BindingFlags.InvokeMethod, null, instrument, connectParams);
                
                System.Threading.Thread.Sleep(1000);
                
                // Execute the newly created method
                string instanceName = sampleSetName + "_executed";
                try
                {
                    object[] runParams = { sampleSetName, instanceName };
                    instrument.GetType().InvokeMember("Run", BindingFlags.InvokeMethod, null, instrument, runParams);
                    Console.WriteLine("✅ Sample set instance '" + instanceName + "' created and executing!");
                    
                    instrument.GetType().InvokeMember("Disconnect", BindingFlags.InvokeMethod, null, instrument, null);
                    Marshal.ReleaseComObject(instrument);
                    Marshal.ReleaseComObject(sampleSetMethodObj);
                    return true;
                }
                catch (Exception runEx)
                {
                    Console.WriteLine("❌ Failed to execute: " + runEx.Message);
                    Console.WriteLine("💡 Method created successfully, but execution failed");
                    
                    instrument.GetType().InvokeMember("Disconnect", BindingFlags.InvokeMethod, null, instrument, null);
                    Marshal.ReleaseComObject(instrument);
                }
            }
            else
            {
                Console.WriteLine("💡 Method created successfully. Use --execute to create instance");
            }
            
            Marshal.ReleaseComObject(sampleSetMethodObj);
            return true; // Method was created successfully
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Method 3 failed: " + ex.Message);
            return false;
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
