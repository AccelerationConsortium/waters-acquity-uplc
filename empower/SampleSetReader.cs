using System;
using System.Runtime.InteropServices;
using System.Reflection;

class SampleSetReader 
{
    static void Main(string[] args)
    {
        Console.WriteLine("Waters Empower Sample Set Reader");
        Console.WriteLine("===============================");
        
        // Check if sample set name was provided as argument
        string targetSampleSet = "20251002_KC"; // Default
        if (args.Length > 0)
        {
            targetSampleSet = args[0];
            Console.WriteLine("Target Sample Set: " + targetSampleSet);
        }
        else
        {
            Console.WriteLine("Usage: SampleSetReader.exe [SampleSetName]");
            Console.WriteLine("Using default: " + targetSampleSet);
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
            
            // Login to Empower
            Console.WriteLine("Attempting login...");
            object[] loginParams = { "", "Waters GPC Training", "system", "manager" };
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
            
            // Get all available sample set method names first
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
                
                // Look for methods that match our target (including variations)
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
                    else if (method.Contains(targetSampleSet.Replace("_KC", "")) || method.Contains(targetSampleSet))
                    {
                        Console.WriteLine("  🔍 RELATED: " + method);
                        matchingMethods[matchCount++] = method;
                    }
                }
                
                if (matchCount == 0)
                {
                    Console.WriteLine("❌ No methods found matching '" + targetSampleSet + "'");
                    Console.WriteLine("\nAll available methods:");
                    foreach (string method in allMethodArray)
                    {
                        Console.WriteLine("  - " + method);
                    }
                    return;
                }
                
                // Use the exact match if found, otherwise use the first related match
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
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
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
}
