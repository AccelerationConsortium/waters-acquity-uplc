using System;
using System.Runtime.InteropServices;
using System.Reflection;

class DocumentedSampleSetExtractor 
{
    static void Main()
    {
        Console.WriteLine("Empower Sample Set Extractor - Using Official Documentation");
        Console.WriteLine("=========================================================");
        
        object project = null;
        object instrument = null;
        
        try 
        {
            // Create Project object using working pattern from StaticAutomation
            Console.WriteLine("Creating MillenniumToolkit.Project...");
            var projectType = Type.GetTypeFromProgID("MillenniumToolkit.Project");
            project = Activator.CreateInstance(projectType);
            Console.WriteLine("✅ Project object created");
            
            // Login using working pattern - empty system name works
            Console.WriteLine("Attempting login...");
            object[] loginParams = { "", "ACQUITY UPLC", "system", "manager" };
            project.GetType().InvokeMember(
                "Login",
                BindingFlags.InvokeMethod,
                null,
                project,
                loginParams
            );
            Console.WriteLine("✅ Project login successful");
            
            // Create Instrument object using working pattern
            Console.WriteLine("Creating MillenniumToolkit.Instrument...");
            var instrumentType = Type.GetTypeFromProgID("MillenniumToolkit.Instrument");
            instrument = Activator.CreateInstance(instrumentType);
            Console.WriteLine("✅ Instrument object created");
            
            // Connect to instrument using documented pattern from VBScript
            Console.WriteLine("Connecting to instrument...");
            object[] connectParams = { "", "ACQUITY UPLC" };
            instrument.GetType().InvokeMember(
                "Connect",
                BindingFlags.InvokeMethod,
                null,
                instrument,
                connectParams
            );
            Console.WriteLine("✅ Instrument connected");
            
            // Get sample set methods using documented VBScript method
            Console.WriteLine("\nTesting documented sample set methods:");
            try 
            {
                var methods = instrument.GetType().InvokeMember(
                    "SampleSetMethods",
                    BindingFlags.GetProperty,
                    null,
                    instrument,
                    null
                );
                Console.WriteLine("✅ SampleSetMethods: " + methods.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠ SampleSetMethods not available: " + ex.Message);
            }
            
            // Get connection status using documented VBScript method
            try 
            {
                var status = instrument.GetType().InvokeMember(
                    "ConnectionStatus",
                    BindingFlags.GetProperty,
                    null,
                    instrument,
                    null
                );
                Console.WriteLine("✅ ConnectionStatus: " + status.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠ ConnectionStatus not available: " + ex.Message);
            }
            
        }
        catch (Exception ex) 
        {
            Console.WriteLine("❌ Error: " + ex.Message);
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
