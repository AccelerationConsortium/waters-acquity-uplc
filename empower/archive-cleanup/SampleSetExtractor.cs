using System;
using System.Runtime.InteropServices;
using System.Reflection;

class SampleSetExtractor 
{
    static void Main()
    {
        Console.WriteLine("Waters Empower Sample Set Extractor");
        Console.WriteLine("===================================");
        
        object project = null;
        object instrument = null;
        
        try 
        {
            // Create Project object
            Console.WriteLine("Creating MillenniumToolkit.Project...");
            var projectType = Type.GetTypeFromProgID("MillenniumToolkit.Project");
            project = Activator.CreateInstance(projectType);
            Console.WriteLine("✅ Project object created: " + project.GetType().Name);
            
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
                if (systems is string[] systemArray)
                {
                    Console.WriteLine($"✅ Found {systemArray.Length} systems:");
                    foreach (string system in systemArray)
                    {
                        Console.WriteLine($"  - {system}");
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
                if (nodes is string[] nodeArray)
                {
                    Console.WriteLine($"✅ Found {nodeArray.Length} nodes:");
                    foreach (string node in nodeArray)
                    {
                        Console.WriteLine($"  - {node}");
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
            
            // Get sample set methods
            Console.WriteLine("\nGetting sample set methods...");
            try 
            {
                var methods = instrument.GetType().InvokeMember(
                    "SampleSetMethods",
                    BindingFlags.GetProperty,
                    null,
                    instrument,
                    null
                );
                if (methods is string[] methodArray)
                {
                    Console.WriteLine($"✅ Found {methodArray.Length} sample set methods:");
                    foreach (string method in methodArray)
                    {
                        Console.WriteLine($"  - {method}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠ SampleSetMethods error: " + ex.Message);
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
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}