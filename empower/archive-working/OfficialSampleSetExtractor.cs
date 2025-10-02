using System;
using System.Runtime.InteropServices;
using System.Reflection;

class OfficialSampleSetExtractor 
{
    static void Main()
    {
        Console.WriteLine("Sample Set Extractor - Official VBScript Parameters");
        Console.WriteLine("===================================================");
        
        object project = null;
        object instrument = null;
        
        try 
        {
            // Create Project object using exact VBScript pattern
            Console.WriteLine("Creating MillenniumToolkit.Project...");
            var projectType = Type.GetTypeFromProgID("MillenniumToolkit.Project");
            project = Activator.CreateInstance(projectType);
            Console.WriteLine("✅ Project object created: " + project.GetType().Name);
            
            // Login using exact VBScript parameters: Login(Database, ProjectName, UserName, Password)
            Console.WriteLine("Attempting login with VBScript parameters...");
            object[] loginParams = { "Waters GPC Training", "Waters GPC Training", "system", "manager" };
            project.GetType().InvokeMember(
                "Login",
                BindingFlags.InvokeMethod,
                null,
                project,
                loginParams
            );
            Console.WriteLine("✅ Project login successful");
            
            // Create Instrument object
            Console.WriteLine("Creating MillenniumToolkit.Instrument...");
            var instrumentType = Type.GetTypeFromProgID("MillenniumToolkit.Instrument");
            instrument = Activator.CreateInstance(instrumentType);
            Console.WriteLine("✅ Instrument object created");
            
            // Connect using VBScript parameters: Connect(SystemName, NodeName)
            Console.WriteLine("Connecting to instrument with VBScript parameters...");
            object[] connectParams = { "ARC HPLC", "Waters-h4q6k34" };
            instrument.GetType().InvokeMember(
                "Connect",
                BindingFlags.InvokeMethod,
                null,
                instrument,
                connectParams
            );
            Console.WriteLine("✅ Instrument connected");
            
            // Test documented methods from VBScript
            Console.WriteLine("\nTesting documented sample set methods from VBScript:");
            
            // Test SampleSetMethods property
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
                Console.WriteLine("⚠ SampleSetMethods error: " + ex.Message);
            }
            
            // Test ConnectionStatus property
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
                Console.WriteLine("⚠ ConnectionStatus error: " + ex.Message);
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
