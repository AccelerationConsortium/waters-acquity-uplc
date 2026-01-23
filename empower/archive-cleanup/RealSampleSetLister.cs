using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

class RealSampleSetLister 
{
    // Configuration settings from secrets.ini
    static Dictionary<string, string> empowerConfig = new Dictionary<string, string>();
    
    static void Main(string[] args)
    {
        Console.WriteLine("Waters Empower Real Sample Set Lister");
        Console.WriteLine("====================================");
        Console.WriteLine("This tool searches for actual Sample Set instances (not just methods/templates)");
        
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
            Console.WriteLine("Usage: RealSampleSetLister.exe [options]");
            Console.WriteLine("Options:");
            Console.WriteLine("  --verbose, -v               Show detailed exploration");
            Console.WriteLine("  --help, -h                  Show this help message");
            Console.WriteLine();
            Console.WriteLine("This tool searches for actual sample set instances vs templates/methods");
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
            
            // Method 1: Check current instrument status for active sample sets
            Console.WriteLine("\n1️⃣ CHECKING CURRENT INSTRUMENT STATUS:");
            Console.WriteLine("======================================");
            CheckCurrentSampleSets(verbose);
            
            // Method 2: Try to find Result Sets (completed sample sets)
            Console.WriteLine("\n2️⃣ SEARCHING FOR RESULT SETS (Completed Sample Sets):");
            Console.WriteLine("======================================================");
            SearchResultSets(verbose);
            
            // Method 3: Try different COM objects that might list actual sample sets
            Console.WriteLine("\n3️⃣ EXPLORING OTHER COM OBJECTS FOR SAMPLE SETS:");
            Console.WriteLine("================================================");
            ExploreOtherCOMObjects(verbose);
            
            // Method 4: Try to access sample sets through Project
            Console.WriteLine("\n4️⃣ SEARCHING THROUGH PROJECT OBJECT:");
            Console.WriteLine("=====================================");
            SearchThroughProject(project, verbose);
            
            // Method 5: For comparison, show Sample Set Methods
            Console.WriteLine("\n5️⃣ FOR COMPARISON - Sample Set Methods (Templates):");
            Console.WriteLine("===================================================");
            ShowSampleSetMethods(verbose);
            
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
    
    static void CheckCurrentSampleSets(bool verbose)
    {
        try
        {
            // Create Instrument object and connect
            var instrumentType = Type.GetTypeFromProgID("MillenniumToolkit.Instrument");
            var instrument = Activator.CreateInstance(instrumentType);
            
            string nodeName = empowerConfig.ContainsKey("node") ? empowerConfig["node"] : "Waters-h4q6k34";
            string systemName = empowerConfig.ContainsKey("system") ? empowerConfig["system"] : "Arc HPLC";
            
            object[] connectParams = { nodeName, systemName };
            instrument.GetType().InvokeMember("Connect", BindingFlags.InvokeMethod, null, instrument, connectParams);
            Console.WriteLine("✅ Connected to instrument");
            
            System.Threading.Thread.Sleep(1000);
            
            // Get status object
            var statusObj = instrument.GetType().InvokeMember("Status", BindingFlags.GetProperty, null, instrument, null);
            
            if (statusObj != null)
            {
                Console.WriteLine("📋 Current Instrument Status:");
                
                // Get current sample set name (actual instance)
                try
                {
                    var sampleSetName = statusObj.GetType().InvokeMember("SampleSetName", BindingFlags.GetProperty, null, statusObj, null);
                    if (sampleSetName != null)
                    {
                        Console.WriteLine("  🎯 Current Sample Set: " + sampleSetName.ToString());
                    }
                }
                catch { }
                
                // Get current sample set method (template)
                try
                {
                    var methodName = statusObj.GetType().InvokeMember("SampleSetMethodName", BindingFlags.GetProperty, null, statusObj, null);
                    if (methodName != null)
                    {
                        Console.WriteLine("  📄 Based on Method: " + methodName.ToString());
                    }
                }
                catch { }
                
                // Get system state
                try
                {
                    var stateDesc = statusObj.GetType().InvokeMember("SystemStateDescription", BindingFlags.GetProperty, null, statusObj, null);
                    if (stateDesc != null)
                    {
                        Console.WriteLine("  🔄 Status: " + stateDesc.ToString());
                    }
                }
                catch { }
                
                // Get current vial and injection info
                try
                {
                    var vial = statusObj.GetType().InvokeMember("Vial", BindingFlags.GetProperty, null, statusObj, null);
                    var injection = statusObj.GetType().InvokeMember("Injection", BindingFlags.GetProperty, null, statusObj, null);
                    var runtime = statusObj.GetType().InvokeMember("RunTime", BindingFlags.GetProperty, null, statusObj, null);
                    
                    if (vial != null && injection != null && runtime != null)
                    {
                        Console.WriteLine("  📊 Current: Vial " + vial + ", Injection " + injection + ", Runtime " + runtime + " min");
                    }
                }
                catch { }
            }
            
            instrument.GetType().InvokeMember("Disconnect", BindingFlags.InvokeMethod, null, instrument, null);
            Marshal.ReleaseComObject(instrument);
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Failed to check current status: " + ex.Message);
        }
    }
    
    static void SearchResultSets(bool verbose)
    {
        try
        {
            // Try MillenniumToolkit.ResultSet
            Console.WriteLine("Trying MillenniumToolkit.ResultSet...");
            try
            {
                var resultSetType = Type.GetTypeFromProgID("MillenniumToolkit.ResultSet");
                if (resultSetType != null)
                {
                    var resultSetObj = Activator.CreateInstance(resultSetType);
                    Console.WriteLine("✅ ResultSet object created");
                    
                    // Try to get result set names
                    string[] resultProperties = { "ResultSetNames", "Names", "SampleSetNames", "CompletedSampleSets" };
                    
                    foreach (string prop in resultProperties)
                    {
                        try
                        {
                            var result = resultSetObj.GetType().InvokeMember(prop, BindingFlags.GetProperty, null, resultSetObj, null);
                            if (result != null && result is string[])
                            {
                                string[] items = result as string[];
                                Console.WriteLine("✅ Found " + prop + " (" + items.Length + " items):");
                                for (int i = 0; i < Math.Min(10, items.Length); i++)
                                {
                                    Console.WriteLine("  " + (i + 1) + ". " + items[i]);
                                }
                                if (items.Length > 10)
                                {
                                    Console.WriteLine("  ... and " + (items.Length - 10) + " more");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (verbose) Console.WriteLine("  ❌ " + prop + " failed: " + ex.Message);
                        }
                    }
                    
                    Marshal.ReleaseComObject(resultSetObj);
                }
                else
                {
                    Console.WriteLine("❌ ResultSet type not available");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ResultSet failed: " + ex.Message);
            }
            
            // Try MillenniumToolkit.ResultSetViewer
            Console.WriteLine("\nTrying MillenniumToolkit.ResultSetViewer...");
            try
            {
                var viewerType = Type.GetTypeFromProgID("MillenniumToolkit.ResultSetViewer");
                if (viewerType != null)
                {
                    var viewerObj = Activator.CreateInstance(viewerType);
                    Console.WriteLine("✅ ResultSetViewer object created");
                    
                    // Try to get result sets through viewer
                    string[] viewerProperties = { "ResultSets", "ResultSetNames", "SampleSets", "SampleSetNames", "CompletedSets" };
                    
                    foreach (string prop in viewerProperties)
                    {
                        try
                        {
                            var result = viewerObj.GetType().InvokeMember(prop, BindingFlags.GetProperty, null, viewerObj, null);
                            if (result != null && result is string[])
                            {
                                string[] items = result as string[];
                                Console.WriteLine("✅ Found " + prop + " (" + items.Length + " items):");
                                for (int i = 0; i < Math.Min(10, items.Length); i++)
                                {
                                    Console.WriteLine("  " + (i + 1) + ". " + items[i]);
                                }
                                if (items.Length > 10)
                                {
                                    Console.WriteLine("  ... and " + (items.Length - 10) + " more");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (verbose) Console.WriteLine("  ❌ " + prop + " failed: " + ex.Message);
                        }
                    }
                    
                    Marshal.ReleaseComObject(viewerObj);
                }
                else
                {
                    Console.WriteLine("❌ ResultSetViewer type not available");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ResultSetViewer failed: " + ex.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ SearchResultSets failed: " + ex.Message);
        }
    }
    
    static void ExploreOtherCOMObjects(bool verbose)
    {
        // Try other possible COM objects that might contain sample sets
        string[] comObjects = {
            "MillenniumToolkit.SampleSet",
            "MillenniumToolkit.SampleSetCollection", 
            "MillenniumToolkit.Database",
            "MillenniumToolkit.QueryBuilder",
            "MillenniumToolkit.SampleSetExplorer"
        };
        
        foreach (string comObject in comObjects)
        {
            try
            {
                Console.WriteLine("Trying " + comObject + "...");
                var objType = Type.GetTypeFromProgID(comObject);
                if (objType != null)
                {
                    var obj = Activator.CreateInstance(objType);
                    Console.WriteLine("✅ " + comObject + " created successfully");
                    
                    // Try common property names
                    string[] properties = { "SampleSets", "SampleSetNames", "Names", "Items", "List" };
                    
                    foreach (string prop in properties)
                    {
                        try
                        {
                            var result = obj.GetType().InvokeMember(prop, BindingFlags.GetProperty, null, obj, null);
                            if (result != null)
                            {
                                if (result is string[])
                                {
                                    string[] items = result as string[];
                                    Console.WriteLine("  ✅ " + prop + " (" + items.Length + " items)");
                                    for (int i = 0; i < Math.Min(5, items.Length); i++)
                                    {
                                        Console.WriteLine("    " + (i + 1) + ". " + items[i]);
                                    }
                                    if (items.Length > 5)
                                    {
                                        Console.WriteLine("    ... and " + (items.Length - 5) + " more");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("  ✅ " + prop + ": " + result.GetType().Name);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (verbose) Console.WriteLine("  ❌ " + prop + " failed: " + ex.Message);
                        }
                    }
                    
                    Marshal.ReleaseComObject(obj);
                }
                else
                {
                    if (verbose) Console.WriteLine("❌ " + comObject + " type not available");
                }
            }
            catch (Exception ex)
            {
                if (verbose) Console.WriteLine("❌ " + comObject + " failed: " + ex.Message);
            }
        }
    }
    
    static void SearchThroughProject(object project, bool verbose)
    {
        // Try to find sample sets through the Project object
        string[] projectProperties = {
            "SampleSets", "SampleSetInstances", "ActiveSampleSets", "CompletedSampleSets",
            "Results", "ResultSets", "Database", "Query", "Explorer"
        };
        
        foreach (string prop in projectProperties)
        {
            try
            {
                Console.WriteLine("Trying Project." + prop + "...");
                var result = project.GetType().InvokeMember(prop, BindingFlags.GetProperty, null, project, null);
                if (result != null)
                {
                    Console.WriteLine("✅ Found " + prop + ": " + result.GetType().Name);
                    
                    if (result is string[])
                    {
                        string[] items = result as string[];
                        Console.WriteLine("  📋 Contains " + items.Length + " items:");
                        for (int i = 0; i < Math.Min(10, items.Length); i++)
                        {
                            Console.WriteLine("    " + (i + 1) + ". " + items[i]);
                        }
                        if (items.Length > 10)
                        {
                            Console.WriteLine("    ... and " + (items.Length - 10) + " more");
                        }
                    }
                    else if (result.ToString() == "System.__ComObject")
                    {
                        // This is another COM object, try to explore it
                        Console.WriteLine("  🔍 Exploring " + prop + " sub-object...");
                        ExploreSubObject(result, prop, verbose);
                    }
                }
            }
            catch (Exception ex)
            {
                if (verbose) Console.WriteLine("❌ Project." + prop + " failed: " + ex.Message);
            }
        }
    }
    
    static void ExploreSubObject(object subObj, string name, bool verbose)
    {
        try
        {
            string[] subProperties = { "SampleSets", "SampleSetNames", "Names", "Items", "List", "Query", "Search" };
            
            foreach (string prop in subProperties)
            {
                try
                {
                    var result = subObj.GetType().InvokeMember(prop, BindingFlags.GetProperty, null, subObj, null);
                    if (result != null && result is string[])
                    {
                        string[] items = result as string[];
                        Console.WriteLine("    ✅ " + name + "." + prop + " (" + items.Length + " items)");
                        for (int i = 0; i < Math.Min(3, items.Length); i++)
                        {
                            Console.WriteLine("      " + (i + 1) + ". " + items[i]);
                        }
                        if (items.Length > 3)
                        {
                            Console.WriteLine("      ... and " + (items.Length - 3) + " more");
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (verbose) Console.WriteLine("    ❌ " + name + "." + prop + " failed: " + ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            if (verbose) Console.WriteLine("  ❌ Failed to explore " + name + ": " + ex.Message);
        }
    }
    
    static void ShowSampleSetMethods(bool verbose)
    {
        try
        {
            var sampleSetMethodType = Type.GetTypeFromProgID("MillenniumToolkit.SampleSetMethod");
            var sampleSetMethodObj = Activator.CreateInstance(sampleSetMethodType);
            
            var methodNames = sampleSetMethodObj.GetType().InvokeMember(
                "SampleSetMethodNames",
                BindingFlags.GetProperty,
                null,
                sampleSetMethodObj,
                null
            );
            
            if (methodNames is string[])
            {
                string[] methods = methodNames as string[];
                Console.WriteLine("📄 Sample Set Methods/Templates (" + methods.Length + " items):");
                for (int i = 0; i < Math.Min(15, methods.Length); i++)
                {
                    Console.WriteLine("  " + (i + 1) + ". " + methods[i]);
                }
                if (methods.Length > 15)
                {
                    Console.WriteLine("  ... and " + (methods.Length - 15) + " more");
                }
            }
            
            Marshal.ReleaseComObject(sampleSetMethodObj);
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Failed to get Sample Set Methods: " + ex.Message);
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
