using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;

class SampleSetCreator 
{
    // Configuration settings from secrets.ini
    static Dictionary<string, string> empowerConfig = new Dictionary<string, string>();
    
    static void Main(string[] args)
    {
        Console.WriteLine("Waters Empower Sample Set Creator");
        Console.WriteLine("=================================");
        
        // Load configuration from secrets.ini
        LoadConfiguration();
        
        // Parse command line arguments
        string newSampleSetName = null;
        string templateName = empowerConfig.ContainsKey("default_template") ? empowerConfig["default_template"] : "20251002_KC"; // Default template from config
        string injectionVolume = null;
        List<string> sampleNames = new List<string>();
        List<string> vials = new List<string>();
        string runtime = null;
        string sampleWeight = null;
        string dilution = null;
        bool showHelp = false;
        
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
                case "--volume":
                case "-v":
                    if (i + 1 < args.Length)
                        injectionVolume = args[++i];
                    break;
                case "--sample-names":
                case "--samples":
                case "-s":
                    // Allow comma-separated list
                    if (i + 1 < args.Length)
                    {
                        string sampleList = args[++i];
                        sampleNames.AddRange(sampleList.Split(',').Select(s => s.Trim()));
                    }
                    break;
                case "--vials":
                case "--positions":
                case "-p":
                    // Single vial position only (format: "1:A,2" for tray 1, row A, column 2)
                    if (i + 1 < args.Length)
                    {
                        string vialPosition = args[++i];
                        vials.Add(vialPosition.Trim());
                    }
                    break;
                case "--runtime":
                case "-r":
                    if (i + 1 < args.Length)
                        runtime = args[++i];
                    break;
                case "--sample-weight":
                case "-w":
                    if (i + 1 < args.Length)
                        sampleWeight = args[++i];
                    break;
                case "--dilution":
                case "-d":
                    if (i + 1 < args.Length)
                        dilution = args[++i];
                    break;
                default:
                    // Support legacy positional arguments for backward compatibility
                    if (!args[i].StartsWith("-"))
                    {
                        if (newSampleSetName == null)
                        {
                            newSampleSetName = args[i];
                        }
                        else if (injectionVolume == null)
                        {
                            injectionVolume = args[i];
                        }
                    }
                    break;
            }
        }
        
        if (showHelp)
        {
            Console.WriteLine("Usage: SampleSetCreator.exe [options] or [SampleSetName] [InjectionVolume]");
            Console.WriteLine("Options:");
            Console.WriteLine("  --name <name>, -n <name>               New sample set name (required)");
            Console.WriteLine("  --template <name>, -t <name>           Template sample set to copy from (default: " + (empowerConfig.ContainsKey("default_template") ? empowerConfig["default_template"] : "20251002_KC") + ")");
            Console.WriteLine("  --injection-volume <vol>, -v <vol>     Injection volume in µL");
            Console.WriteLine("  --sample-names <names>, -s <names>     Comma-separated sample names (e.g., \"MN11,MN12,MN13\")");
            Console.WriteLine("  --vials <position>, -p <position>      Single vial position (e.g., \"1:A,2\" for tray 1, row A, column 2)");
            Console.WriteLine("  --runtime <time>, -r <time>            Runtime in minutes");
            Console.WriteLine("  --sample-weight <weight>, -w <weight>  Sample weight");
            Console.WriteLine("  --dilution <factor>, -d <factor>       Dilution factor");
            Console.WriteLine("  --help, -h                             Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  # Legacy format");
            Console.WriteLine("  SampleSetCreator.exe \"MyNewSet\" \"10.0\"");
            Console.WriteLine();
            Console.WriteLine("  # Enhanced format with custom injection volume");
            Console.WriteLine("  SampleSetCreator.exe --name \"MySet\" --injection-volume 10.0");
            Console.WriteLine();
            Console.WriteLine("  # Create with vial position and injection volume");
            Console.WriteLine("  SampleSetCreator.exe --name \"TestSet\" --vials \"1:A,2\" --injection-volume 5.0");
            Console.WriteLine();
            Console.WriteLine("  # Use different template");
            Console.WriteLine("  SampleSetCreator.exe --name \"FromOther\" --template \"20251029_KC\"");
            return;
        }
        
        // Fallback to legacy default if no name provided
        if (newSampleSetName == null)
        {
            Console.WriteLine("❌ Error: Sample set name is required. Use --name or provide as first argument.");
            Console.WriteLine("Use --help for usage information.");
            return;
        }
        
        Console.WriteLine("Configuration:");
        Console.WriteLine("  New Sample Set: " + newSampleSetName);
        Console.WriteLine("  Template: " + templateName);
        if (injectionVolume != null) Console.WriteLine("  Injection Volume: " + injectionVolume + " µL");
        if (sampleNames.Count > 0) Console.WriteLine("  Sample Names: " + string.Join(", ", sampleNames.ToArray()));
        if (vials.Count > 0) Console.WriteLine("  Vial Positions: " + string.Join(", ", vials.ToArray()));
        if (runtime != null) Console.WriteLine("  Runtime: " + runtime + " min");
        if (sampleWeight != null) Console.WriteLine("  Sample Weight: " + sampleWeight);
        if (dilution != null) Console.WriteLine("  Dilution: " + dilution);
        
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
            
            // Create new sample set by copying existing one and modifying it
            Console.WriteLine("\n🔨 CREATING NEW SAMPLE SET: " + newSampleSetName);
            Console.WriteLine("==========================================");
            
            try
            {
                // First, load the template sample set
                Console.WriteLine("Loading template sample set: " + templateName);
                
                sampleSetMethodObj.GetType().InvokeMember(
                    "Name",
                    BindingFlags.SetProperty,
                    null,
                    sampleSetMethodObj,
                    new object[] { templateName }
                );
                
                sampleSetMethodObj.GetType().InvokeMember(
                    "Fetch",
                    BindingFlags.InvokeMethod,
                    null,
                    sampleSetMethodObj,
                    null
                );
                
                Console.WriteLine("✅ Template sample set loaded");
                
                // Now change the name to create a copy
                Console.WriteLine("Setting new sample set name: " + newSampleSetName);
                sampleSetMethodObj.GetType().InvokeMember(
                    "Name",
                    BindingFlags.SetProperty,
                    null,
                    sampleSetMethodObj,
                    new object[] { newSampleSetName }
                );
                
                // Get sample set lines to modify
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
                    Console.WriteLine("Template has " + lineCount + " sample lines");
                    
                    // Determine how many lines we need (limited to existing template lines)
                    int maxLines = Math.Max(Math.Max(sampleNames.Count, vials.Count), 1);
                    maxLines = Math.Min(maxLines, lineCount); // Can't exceed template line count
                    
                    // Modify each sample line
                    for (int i = 0; i < lineCount && i < maxLines; i++)
                    {
                        var sampleLine = sampleSetLines.GetType().InvokeMember(
                            "Item",
                            BindingFlags.InvokeMethod | BindingFlags.GetProperty,
                            null,
                            sampleSetLines,
                            new object[] { i }
                        );
                        
                        if (sampleLine != null)
                        {
                            Console.WriteLine("✅ Modifying sample line " + (i + 1) + "...");
                            
                            // Update sample name if provided
                            if (sampleNames.Count > i)
                            {
                                SetSampleLineField(sampleLine, "SampleName", sampleNames[i]);
                            }
                            
                            // Update vial position if provided
                            if (vials.Count > i)
                            {
                                SetSampleLineField(sampleLine, "Vial", vials[i]);
                            }
                            
                            // Update injection volume if provided
                            if (injectionVolume != null)
                            {
                                SetSampleLineField(sampleLine, "InjVol", injectionVolume);
                            }
                            else if (i == 0)
                            {
                                // Read the current injection volume for display (first line only)
                                try
                                {
                                    var currentInjVol = sampleLine.GetType().InvokeMember(
                                        "Get", 
                                        BindingFlags.InvokeMethod, 
                                        null, 
                                        sampleLine, 
                                        new object[] { "InjVol", true }
                                    );
                                    Console.WriteLine("✅ Preserving original injection volume: " + currentInjVol + " µL");
                                }
                                catch
                                {
                                    Console.WriteLine("✅ Preserving original template parameters (injection volume unchanged)");
                                }
                            }
                            
                            // Update runtime if provided
                            if (runtime != null)
                            {
                                SetSampleLineField(sampleLine, "Runtime", runtime);
                            }
                            
                            // Update sample weight if provided
                            if (sampleWeight != null)
                            {
                                SetSampleLineField(sampleLine, "SampleWeight", sampleWeight);
                            }
                            
                            // Update dilution if provided
                            if (dilution != null)
                            {
                                SetSampleLineField(sampleLine, "Dilution", dilution);
                            }
                        }
                    }
                    
                    // Try to save the modified sample set
                    Console.WriteLine("\nAttempting to store new sample set...");
                    try
                    {
                        sampleSetMethodObj.GetType().InvokeMember(
                            "Store",
                            BindingFlags.InvokeMethod,
                            null,
                            sampleSetMethodObj,
                            null
                        );
                        Console.WriteLine("🎉 NEW SAMPLE SET CREATED SUCCESSFULLY!");
                        Console.WriteLine("Sample Set: " + newSampleSetName);
                        Console.WriteLine("Based on template: " + templateName);
                        Console.WriteLine("Sample lines: " + Math.Min(lineCount, maxLines));
                        
                        if (injectionVolume != null) Console.WriteLine("Injection Volume: " + injectionVolume + " µL");
                        if (sampleNames.Count > 0) Console.WriteLine("Sample Names: " + string.Join(", ", sampleNames.Take(Math.Min(lineCount, maxLines)).ToArray()));
                        if (vials.Count > 0) Console.WriteLine("Vial Positions: " + string.Join(", ", vials.Take(Math.Min(lineCount, maxLines)).ToArray()));
                    }
                    catch (Exception storeEx)
                    {
                        Console.WriteLine("❌ Error storing sample set: " + storeEx.Message);
                        if (storeEx.InnerException != null)
                        {
                            Console.WriteLine("Inner exception: " + storeEx.InnerException.Message);
                            if (storeEx.InnerException.InnerException != null)
                            {
                                Console.WriteLine("Inner inner exception: " + storeEx.InnerException.InnerException.Message);
                            }
                        }
                        Console.WriteLine("Full stack trace: " + storeEx.StackTrace);
                        Console.WriteLine("Sample set may exist in memory but not saved to database");
                    }
                }
                else
                {
                    Console.WriteLine("❌ Failed to get SampleSetLines collection");
                }
                
            }
            catch (Exception createEx)
            {
                Console.WriteLine("❌ Error creating sample set: " + createEx.Message);
                if (createEx.InnerException != null)
                {
                    Console.WriteLine("Inner exception: " + createEx.InnerException.Message);
                    if (createEx.InnerException.InnerException != null)
                    {
                        Console.WriteLine("Inner inner exception: " + createEx.InnerException.InnerException.Message);
                    }
                }
                Console.WriteLine("Stack trace: " + createEx.StackTrace);
            }
            
            Console.WriteLine("\n🎉 Sample set creation process completed!");
            
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
    
    // Helper method to set sample line field using official Waters pattern
    static void SetSampleLineField(object sampleLine, string fieldName, string value)
    {
        try
        {
            sampleLine.GetType().InvokeMember(
                "Set", 
                BindingFlags.InvokeMethod, 
                null, 
                sampleLine, 
                new object[] { fieldName, value }
            );
            Console.WriteLine("  " + fieldName + " = " + value);
        }
        catch (Exception ex)
        {
            Console.WriteLine("  ❌ Failed to set " + fieldName + ": " + ex.Message);
        }
    }
    
    // Helper method to read a sample line field for verification
    static void ReadSampleLineField(object sampleLine, string fieldName)
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
                Console.WriteLine("  " + fieldName + ": " + value.ToString());
            }
        }
        catch
        {
            Console.WriteLine("  " + fieldName + ": (unable to read)");
        }
    }
}
