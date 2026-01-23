using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

class StatusExplorer 
{
    // Configuration settings from secrets.ini
    static Dictionary<string, string> empowerConfig = new Dictionary<string, string>();
    
    static void Main(string[] args)
    {
        Console.WriteLine("Waters Empower Status Object Explorer");
        Console.WriteLine("====================================");
        Console.WriteLine("This tool explores the Instrument.Status object to find actual sample set info");
        
        // Load configuration from secrets.ini
        LoadConfiguration();
        
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
            
            // Get the Status object
            Console.WriteLine("\n🔍 EXPLORING INSTRUMENT STATUS OBJECT:");
            Console.WriteLine("======================================");
            
            var statusObj = instrument.GetType().InvokeMember(
                "Status",
                BindingFlags.GetProperty,
                null,
                instrument,
                null
            );
            
            if (statusObj != null)
            {
                Console.WriteLine("✅ Got Status object: " + statusObj.GetType().Name);
                
                // Explore all properties of the Status object
                ExploreStatusObject(statusObj);
                
                // Try specific properties that might contain sample set info
                ExploreStatusSampleSets(statusObj);
            }
            else
            {
                Console.WriteLine("❌ Status object is null");
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
    
    static void ExploreStatusObject(object statusObj)
    {
        Console.WriteLine("\n📋 Status Object Properties and Methods:");
        Console.WriteLine("=========================================");
        
        try
        {
            Type type = statusObj.GetType();
            
            // Get all properties
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Console.WriteLine("Properties (" + properties.Length + "):");
            foreach (var prop in properties)
            {
                try
                {
                    var value = prop.GetValue(statusObj, null);
                    string valueStr = (value != null) ? value.ToString() : "null";
                    if (valueStr.Length > 100) valueStr = valueStr.Substring(0, 100) + "...";
                    Console.WriteLine("  ✅ " + prop.Name + " (" + prop.PropertyType.Name + ") = " + valueStr);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("  ❌ " + prop.Name + " - Error: " + ex.Message);
                }
            }
            
            // Get all methods
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            Console.WriteLine("\nMethods (" + methods.Length + "):");
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
                Console.WriteLine("  🔧 " + methodName + "()");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Failed to explore Status object: " + ex.Message);
        }
    }
    
    static void ExploreStatusSampleSets(object statusObj)
    {
        Console.WriteLine("\n🔍 Looking for Sample Set info in Status object:");
        Console.WriteLine("================================================");
        
        // Try various property names that might contain sample set info
        string[] possibleProperties = {
            "SampleSet", "SampleSetName", "CurrentSampleSet", "RunningSampleSet",
            "ActiveSampleSet", "SampleSetMethod", "SampleSetMethodName", "Method",
            "MethodName", "CurrentMethod", "RunningMethod", "ActiveMethod",
            "Queue", "QueuedSampleSets", "QueuedMethods", "QueueName",
            "Status", "State", "SystemState", "SystemStateDescription",
            "RunStatus", "ExecutionStatus", "InstrumentState", "CurrentState",
            "Vial", "Injection", "RunTime", "Progress", "CurrentVial",
            "CurrentInjection", "CurrentRunTime", "ElapsedTime"
        };
        
        foreach (string propName in possibleProperties)
        {
            try
            {
                Console.WriteLine("  🔍 Trying Status." + propName + "...");
                var result = statusObj.GetType().InvokeMember(
                    propName,
                    BindingFlags.GetProperty,
                    null,
                    statusObj,
                    null
                );
                
                if (result != null)
                {
                    Console.WriteLine("    ✅ Found " + propName + ": " + result.GetType().Name);
                    string resultStr = result.ToString();
                    if (resultStr != "System.__ComObject" && resultStr.Length < 200)
                    {
                        Console.WriteLine("    📝 Value: " + resultStr);
                    }
                    else if (resultStr == "System.__ComObject")
                    {
                        Console.WriteLine("    🔍 This is another COM object - exploring...");
                        ExploreSubObject(result, propName);
                    }
                    
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
                }
            }
            catch (Exception ex)
            {
                // Only show successful finds to avoid clutter
            }
        }
    }
    
    static void ExploreSubObject(object subObj, string parentName)
    {
        try
        {
            Console.WriteLine("      🔍 Exploring " + parentName + " sub-object:");
            
            // Try a few key properties that might be in sub-objects
            string[] subProperties = {
                "Name", "SampleSet", "SampleSetName", "Method", "MethodName",
                "Status", "State", "Value", "Text", "Description"
            };
            
            foreach (string propName in subProperties)
            {
                try
                {
                    var result = subObj.GetType().InvokeMember(
                        propName,
                        BindingFlags.GetProperty,
                        null,
                        subObj,
                        null
                    );
                    
                    if (result != null && result.ToString() != "System.__ComObject")
                    {
                        Console.WriteLine("        ✅ " + parentName + "." + propName + " = " + result.ToString());
                    }
                }
                catch
                {
                    // Ignore failures in sub-object exploration
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("      ❌ Failed to explore " + parentName + " sub-object: " + ex.Message);
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
