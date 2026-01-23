using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

class SampleSetReader 
{
    // Configuration settings from secrets.ini
    static Dictionary<string, string> empowerConfig = new Dictionary<string, string>();
    
    static void Main(string[] args)
    {
        Console.WriteLine("Waters Empower Sample Set Reader");
        Console.WriteLine("===============================");
        
        // Load configuration from secrets.ini
        LoadConfiguration();
        
        // Parse command line arguments
        string targetSampleSet = null;
        bool listAll = false;
        bool showHelp = false;
        
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--help":
                case "-h":
                    showHelp = true;
                    break;
                case "--list-all":
                case "-l":
                    listAll = true;
                    break;
                case "--name":
                case "-n":
                    if (i + 1 < args.Length)
                    {
                        targetSampleSet = args[++i];
                    }
                    break;
                default:
                    // If no flag specified, treat as sample set name
                    if (!args[i].StartsWith("-"))
                    {
                        targetSampleSet = args[i];
                    }
                    break;
            }
        }
        
        if (showHelp)
        {
            Console.WriteLine("Usage: SampleSetReader.exe [options]");
            Console.WriteLine("Options:");
            Console.WriteLine("  --list-all, -l              List all available sample sets");
            Console.WriteLine("  --name <name>, -n <name>     Read specific sample set by name");
            Console.WriteLine("  --help, -h                   Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  SampleSetReader.exe --list-all");
            Console.WriteLine("  SampleSetReader.exe --name \"20251002_KC\"");
            Console.WriteLine("  SampleSetReader.exe \"20251002_KC\"  (same as above)");
            return;
        }
        
        object project = null;
        object sampleSetMethodObj = null;
        
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
            
            // Create SampleSetMethod object
            Console.WriteLine("\nCreating MillenniumToolkit.SampleSetMethod...");
            var sampleSetMethodType = Type.GetTypeFromProgID("MillenniumToolkit.SampleSetMethod");
            sampleSetMethodObj = Activator.CreateInstance(sampleSetMethodType);
            Console.WriteLine("✅ SampleSetMethod object created");
            
            // Get all available sample set method names
            Console.WriteLine("\nGetting available sample set methods...");
            var allMethodNames = sampleSetMethodObj.GetType().InvokeMember(
                "SampleSetMethodNames",
                BindingFlags.GetProperty,
                null,
                sampleSetMethodObj,
                null
            );
            string[] allMethodArray = allMethodNames as string[];
            
            if (allMethodArray != null)
            {
                Console.WriteLine("✅ Found " + allMethodArray.Length + " total sample set methods");
                
                if (listAll)
                {
                    // List all sample sets
                    Console.WriteLine("\n📋 ALL AVAILABLE SAMPLE SETS:");
                    Console.WriteLine("==============================");
                    for (int i = 0; i < allMethodArray.Length; i++)
                    {
                        Console.WriteLine((i + 1).ToString().PadLeft(3) + ". " + allMethodArray[i]);
                    }
                    return;
                }
                
                if (targetSampleSet == null)
                {
                    Console.WriteLine("\n⚠️  No sample set specified. Use --list-all to see all available sample sets or --name to specify one.");
                    Console.WriteLine("First 10 available sample sets:");
                    for (int i = 0; i < Math.Min(10, allMethodArray.Length); i++)
                    {
                        Console.WriteLine("  " + (i + 1) + ". " + allMethodArray[i]);
                    }
                    if (allMethodArray.Length > 10)
                    {
                        Console.WriteLine("  ... and " + (allMethodArray.Length - 10) + " more. Use --list-all to see them all.");
                    }
                    return;
                }
                
                // Look for methods that match our target
                Console.WriteLine("\nLooking for methods related to '" + targetSampleSet + "':");
                bool foundExact = false;
                string[] matchingMethods = new string[allMethodArray.Length];
                int matchCount = 0;
                
                foreach (string method in allMethodArray)
                {
                    if (method.Equals(targetSampleSet, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("  ✅ EXACT MATCH: " + method);
                        foundExact = true;
                        matchingMethods[matchCount++] = method;
                    }
                    else if (method.IndexOf(targetSampleSet, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Console.WriteLine("  🔍 PARTIAL MATCH: " + method);
                        matchingMethods[matchCount++] = method;
                    }
                }
                
                if (matchCount == 0)
                {
                    Console.WriteLine("❌ No methods found matching '" + targetSampleSet + "'");
                    Console.WriteLine("\nDid you mean one of these? (showing first 10):");
                    for (int i = 0; i < Math.Min(10, allMethodArray.Length); i++)
                    {
                        Console.WriteLine("  - " + allMethodArray[i]);
                    }
                    Console.WriteLine("\nUse --list-all to see all available sample sets");
                    return;
                }
                
                // Show all matches if more than one
                if (matchCount > 1)
                {
                    Console.WriteLine("\nFound " + matchCount + " matching sample sets:");
                    for (int i = 0; i < matchCount; i++)
                    {
                        Console.WriteLine("  " + (i + 1) + ". " + matchingMethods[i]);
                    }
                    Console.WriteLine("\nReading the first match: " + matchingMethods[0]);
                }
                
                // Use the exact match if found, otherwise use the first match
                string methodToRead = foundExact ? targetSampleSet : matchingMethods[0];
                Console.WriteLine("\n📖 READING SAMPLE SET: " + methodToRead);
                Console.WriteLine("==========================================");
                
                try
                {
                    // Use the official pattern: set Name property, then call Fetch()
                    Console.WriteLine("Setting sample set name: " + methodToRead);
                    sampleSetMethodObj.GetType().InvokeMember(
                        "Name",
                        BindingFlags.SetProperty,
                        null,
                        sampleSetMethodObj,
                        new object[] { methodToRead }
                    );
                    
                    Console.WriteLine("Fetching sample set data...");
                    sampleSetMethodObj.GetType().InvokeMember(
                        "Fetch",
                        BindingFlags.InvokeMethod,
                        null,
                        sampleSetMethodObj,
                        null
                    );
                    
                    Console.WriteLine("✅ Sample set data loaded successfully");
                    
                    // Get basic sample set information
                    try
                    {
                        var name = sampleSetMethodObj.GetType().InvokeMember("Name", BindingFlags.GetProperty, null, sampleSetMethodObj, null);
                        Console.WriteLine("\n📋 SAMPLE SET INFORMATION:");
                        Console.WriteLine("Name: " + name.ToString());
                        
                        // Get sample set lines
                        Console.WriteLine("\n🧪 SAMPLE SET LINES:");
                        Console.WriteLine("===================");
                        
                        var sampleSetLines = sampleSetMethodObj.GetType().InvokeMember(
                            "SampleSetLines",
                            BindingFlags.GetProperty,
                            null,
                            sampleSetMethodObj,
                            null
                        );
                        
                        if (sampleSetLines != null)
                        {
                            var count = sampleSetLines.GetType().InvokeMember("Count", BindingFlags.GetProperty, null, sampleSetLines, null);
                            int lineCount = (int)count;
                            Console.WriteLine("Total Sample Lines: " + lineCount);
                            
                            // Read each sample line using 0-based indexing
                            for (int i = 0; i < lineCount; i++)
                            {
                                try
                                {
                                    Console.WriteLine("\n--- Sample Line " + (i + 1) + " ---");
                                    
                                    var sampleLine = sampleSetLines.GetType().InvokeMember(
                                        "Item",
                                        BindingFlags.InvokeMethod | BindingFlags.GetProperty,
                                        null,
                                        sampleSetLines,
                                        new object[] { i }
                                    );
                                    
                                    if (sampleLine != null)
                                    {
                                        ReadSampleLineFields(sampleLine);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("❌ Error reading sample line " + (i + 1) + ": " + ex.Message);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("❌ SampleSetLines is null");
                        }
                    }
                    catch (Exception infoEx)
                    {
                        Console.WriteLine("❌ Error reading sample set information: " + infoEx.Message);
                        if (infoEx.InnerException != null)
                        {
                            Console.WriteLine("Inner exception: " + infoEx.InnerException.Message);
                        }
                    }
                }
                catch (Exception fetchEx)
                {
                    Console.WriteLine("❌ Error loading sample set '" + methodToRead + "': " + fetchEx.Message);
                    Console.WriteLine("This sample set may not exist or may be corrupted.");
                }
            }
            else
            {
                Console.WriteLine("❌ No sample set methods found");
            }
            
            Console.WriteLine("\n🎉 Sample set reading completed!");
            
        }
        catch (Exception ex) 
        {
            Console.WriteLine("❌ Error: " + ex.Message);
            Console.WriteLine("Stack Trace: " + ex.StackTrace);
        }
        finally 
        {
            // Cleanup
            if (sampleSetMethodObj != null) Marshal.ReleaseComObject(sampleSetMethodObj);
            if (project != null) Marshal.ReleaseComObject(project);
            Console.WriteLine("✅ COM cleanup completed");
        }
    }

    // Helper method to read key sample line fields
    static void ReadSampleLineFields(object sampleLine)
    {
        try
        {
            // Confirmed working field names from Waters COM interface
            string[] workingFields = { 
                "SampleName", "Vial", "Runtime", "Function", "InjVol", "SampleWeight", "Dilution"
            };
            
            foreach (string fieldName in workingFields)
            {
                TryReadField(sampleLine, fieldName);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error reading sample line fields: " + ex.Message);
        }
    }
    
    // Helper method to try reading a specific field
    static void TryReadField(object sampleLine, string fieldName)
    {
        try
        {
            var value = sampleLine.GetType().InvokeMember(
                "Get", 
                BindingFlags.InvokeMethod, 
                null, 
                sampleLine, 
                new object[] { fieldName, true }
            );
            
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                Console.WriteLine(fieldName + ": " + value.ToString());
            }
        }
        catch
        {
            // Field doesn't exist or can't be read, skip silently
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
