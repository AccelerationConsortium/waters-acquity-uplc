using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

class SampleSetExplorer 
{
    // Configuration settings from secrets.ini
    static Dictionary<string, string> empowerConfig = new Dictionary<string, string>();
    
    static void Main(string[] args)
    {
        Console.WriteLine("Waters Empower Sample Set Explorer");
        Console.WriteLine("==================================");
        Console.WriteLine("This tool connects to instrument and explores actual running/queued sample sets");
        
        // Load configuration from secrets.ini
        LoadConfiguration();
        
        // Parse command line arguments
        bool showHelp = false;
        bool verbose = false;
        
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--help":
                case "-h":
                    showHelp = true;
                    break;
                case "--verbose":
                case "-v":
                    verbose = true;
                    break;
            }
        }
        
        if (showHelp)
        {
            Console.WriteLine("Usage: SampleSetExplorer.exe [options]");
            Console.WriteLine("Options:");
            Console.WriteLine("  --verbose, -v               Show detailed COM object exploration");
            Console.WriteLine("  --help, -h                  Show this help message");
            Console.WriteLine();
            Console.WriteLine("This tool connects to the instrument and explores running/queued sample sets");
            Console.WriteLine("vs sample set methods/templates.");
            return;
        }
        
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
            
            // Create Instrument object
            Console.WriteLine("\nCreating MillenniumToolkit.Instrument...");
            var instrumentType = Type.GetTypeFromProgID("MillenniumToolkit.Instrument");
            instrument = Activator.CreateInstance(instrumentType);
            Console.WriteLine("✅ Instrument object created");
            
            // Connect to instrument
            Console.WriteLine("\nConnecting to instrument...");
            string nodeName = empowerConfig.ContainsKey("node") ? empowerConfig["node"] : "Waters-h4q6k34";
            string systemName = empowerConfig.ContainsKey("system") ? empowerConfig["system"] : "Arc HPLC";
            
            object[] connectParams = { nodeName, systemName };
            instrument.GetType().InvokeMember(
                "Connect",
                BindingFlags.InvokeMethod,
                null,
                instrument,
                connectParams
            );
            Console.WriteLine("✅ Instrument connected to " + systemName + " on " + nodeName);
            
            // Wait for connection to establish
            Console.WriteLine("Waiting for connection to establish...");
            System.Threading.Thread.Sleep(2000);
            
            // Now explore what's available through the connected instrument
            Console.WriteLine("\n🔍 EXPLORING CONNECTED INSTRUMENT FOR SAMPLE SETS:");
            Console.WriteLine("==================================================");
            
            if (verbose)
            {
                ExploreObjectMethods(instrument, "Connected Instrument");
            }
            
            // Try to get sample sets from connected instrument
            ExploreInstrumentSampleSets(instrument, verbose);
            
            // Try to get queue information
            ExploreInstrumentQueue(instrument, verbose);
            
            // Try to get status/running information
            ExploreInstrumentStatus(instrument, verbose);
            
            // Try to get result sets (completed sample sets)
            ExploreInstrumentResults(instrument, verbose);
            
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
                    instrument.GetType().InvokeMember(
                        "Disconnect",
                        BindingFlags.InvokeMethod,
                        null,
                        instrument,
                        null
                    );
                    Console.WriteLine("✅ Instrument disconnected");
                }
                catch (Exception disconnectEx)
                {
                    Console.WriteLine("⚠️ Disconnect error: " + disconnectEx.Message);
                }
                Marshal.ReleaseComObject(instrument);
            }
            if (project != null) Marshal.ReleaseComObject(project);
            Console.WriteLine("✅ COM cleanup completed");
        }
    }
    
    static void ExploreInstrumentSampleSets(object instrument, bool verbose)
    {
        Console.WriteLine("\n1️⃣ Looking for Sample Sets on connected instrument...");
        
        // Try various property names that might contain actual sample sets
        string[] possibleProperties = {
            "SampleSets", "SampleSetNames", "SampleSetList", "ActiveSampleSets",
            "QueuedSampleSets", "RunningSampleSets", "PendingSampleSets",
            "SampleSetMethods", "SampleSetMethodNames", "Methods", "MethodNames"
        };
        
        foreach (string propName in possibleProperties)
        {
            try
            {
                Console.WriteLine("  🔍 Trying Instrument." + propName + "...");
                var result = instrument.GetType().InvokeMember(
                    propName,
                    BindingFlags.GetProperty,
                    null,
                    instrument,
                    null
                );
                
                if (result != null)
                {
                    Console.WriteLine("    ✅ Found " + propName + ": " + result.GetType().Name);
                    if (result is string[])
                    {
                        string[] items = result as string[];
                        Console.WriteLine("    📋 Contains " + items.Length + " items:");
                        for (int i = 0; i < Math.Min(10, items.Length); i++)
                        {
                            Console.WriteLine("      " + (i + 1) + ". " + items[i]);
                        }
                        if (items.Length > 10)
                        {
                            Console.WriteLine("      ... and " + (items.Length - 10) + " more");
                        }
                    }
                    else if (result.ToString() != "System.__ComObject")
                    {
                        Console.WriteLine("    📝 Value: " + result.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                if (verbose)
                {
                    Console.WriteLine("    ❌ " + propName + " failed: " + ex.Message);
                }
            }
        }
    }
    
    static void ExploreInstrumentQueue(object instrument, bool verbose)
    {
        Console.WriteLine("\n2️⃣ Looking for Queue information...");
        
        string[] queueProperties = {
            "Queue", "QueueNames", "QueuedItems", "QueueLength", "QueueStatus",
            "ActiveQueue", "QueueList", "QueueMethods", "PendingQueue"
        };
        
        foreach (string propName in queueProperties)
        {
            try
            {
                Console.WriteLine("  🔍 Trying Instrument." + propName + "...");
                var result = instrument.GetType().InvokeMember(
                    propName,
                    BindingFlags.GetProperty,
                    null,
                    instrument,
                    null
                );
                
                if (result != null)
                {
                    Console.WriteLine("    ✅ Found " + propName + ": " + result.GetType().Name);
                    DisplayResult(result, propName);
                }
            }
            catch (Exception ex)
            {
                if (verbose)
                {
                    Console.WriteLine("    ❌ " + propName + " failed: " + ex.Message);
                }
            }
        }
    }
    
    static void ExploreInstrumentStatus(object instrument, bool verbose)
    {
        Console.WriteLine("\n3️⃣ Looking for Status/Running information...");
        
        string[] statusProperties = {
            "Status", "InstrumentStatus", "State", "CurrentState", "RunningStatus",
            "CurrentSampleSet", "RunningSampleSet", "ActiveSampleSet", "CurrentMethod",
            "RunningMethod", "CurrentRun", "RunStatus", "ExecutionStatus"
        };
        
        foreach (string propName in statusProperties)
        {
            try
            {
                Console.WriteLine("  🔍 Trying Instrument." + propName + "...");
                var result = instrument.GetType().InvokeMember(
                    propName,
                    BindingFlags.GetProperty,
                    null,
                    instrument,
                    null
                );
                
                if (result != null)
                {
                    Console.WriteLine("    ✅ Found " + propName + ": " + result.GetType().Name);
                    DisplayResult(result, propName);
                }
            }
            catch (Exception ex)
            {
                if (verbose)
                {
                    Console.WriteLine("    ❌ " + propName + " failed: " + ex.Message);
                }
            }
        }
    }
    
    static void ExploreInstrumentResults(object instrument, bool verbose)
    {
        Console.WriteLine("\n4️⃣ Looking for Results/Completed information...");
        
        string[] resultProperties = {
            "Results", "ResultSets", "ResultSetNames", "CompletedSampleSets",
            "CompletedResults", "FinishedSampleSets", "ProcessedSampleSets",
            "ResultList", "CompletedMethods", "FinishedMethods"
        };
        
        foreach (string propName in resultProperties)
        {
            try
            {
                Console.WriteLine("  🔍 Trying Instrument." + propName + "...");
                var result = instrument.GetType().InvokeMember(
                    propName,
                    BindingFlags.GetProperty,
                    null,
                    instrument,
                    null
                );
                
                if (result != null)
                {
                    Console.WriteLine("    ✅ Found " + propName + ": " + result.GetType().Name);
                    DisplayResult(result, propName);
                }
            }
            catch (Exception ex)
            {
                if (verbose)
                {
                    Console.WriteLine("    ❌ " + propName + " failed: " + ex.Message);
                }
            }
        }
    }
    
    static void DisplayResult(object result, string propertyName)
    {
        if (result is string[])
        {
            string[] items = result as string[];
            Console.WriteLine("    📋 Contains " + items.Length + " items:");
            for (int i = 0; i < Math.Min(5, items.Length); i++)
            {
                Console.WriteLine("      " + (i + 1) + ". " + items[i]);
            }
            if (items.Length > 5)
            {
                Console.WriteLine("      ... and " + (items.Length - 5) + " more");
            }
        }
        else if (result.ToString() != "System.__ComObject" && result.ToString().Length < 200)
        {
            Console.WriteLine("    📝 Value: " + result.ToString());
        }
        else
        {
            Console.WriteLine("    📦 Object type: " + result.GetType().Name);
        }
    }
    
    static void ExploreObjectMethods(object obj, string objectName)
    {
        Console.WriteLine("  🔍 Exploring " + objectName + " object methods and properties:");
        try
        {
            Type type = obj.GetType();
            
            // Get properties
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (properties.Length > 0)
            {
                Console.WriteLine("    📋 Properties (" + properties.Length + "):");
                foreach (var prop in properties)
                {
                    Console.WriteLine("      - " + prop.Name + " (" + prop.PropertyType.Name + ")");
                }
            }
            
            // Get methods
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            if (methods.Length > 0)
            {
                Console.WriteLine("    🔧 Methods (" + methods.Length + "):");
                var uniqueMethods = new HashSet<string>();
                foreach (var method in methods)
                {
                    if (!method.Name.StartsWith("get_") && !method.Name.StartsWith("set_") && 
                        !method.Name.StartsWith("add_") && !method.Name.StartsWith("remove_") &&
                        !method.Name.Equals("GetType") && !method.Name.Equals("ToString") &&
                        !method.Name.Equals("Equals") && !method.Name.Equals("GetHashCode"))
                    {
                        uniqueMethods.Add(method.Name);
                    }
                }
                foreach (string methodName in uniqueMethods)
                {
                    Console.WriteLine("      - " + methodName + "()");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("    ❌ Failed to explore object: " + ex.Message);
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
