using System;
using System.Runtime.InteropServices;

namespace EmpowerAutomation
{
    class SampleSetLister
    {
        static void Main(string[] args)
        {
            dynamic project = null;
            dynamic instrument = null;

            try
            {
                Console.WriteLine("Creating Empower project connection...");
                
                // Use exact pattern from working EmpowerWorkflow
                Type projectType = Type.GetTypeFromProgID("MillenniumToolkit.Project");
                project = Activator.CreateInstance(projectType);

                Console.WriteLine("Attempting login...");
                project.Login("EmpowerPersonal", "ACQUITY UPLC", "system", "manager");
                Console.WriteLine("✅ Project login successful");

                // Create instrument using exact pattern from VBScript
                Type instrumentType = Type.GetTypeFromProgID("MillenniumToolkit.Instrument");
                instrument = Activator.CreateInstance(instrumentType);

                Console.WriteLine("Connecting to instrument...");
                instrument.Connect("EmpowerPersonal", "ACQUITY UPLC");
                Console.WriteLine("✅ Instrument connected");

                // Check what sample set methods are available (from VBScript)
                Console.WriteLine("\nQuerying sample set methods...");
                try
                {
                    dynamic methods = instrument.SampleSetMethods;
                    Console.WriteLine("Sample set methods: " + methods.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SampleSetMethods not available: " + ex.Message);
                }

                // Check connection status (from VBScript)
                try
                {
                    dynamic status = instrument.ConnectionStatus;
                    Console.WriteLine("Connection status: " + status.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ConnectionStatus not available: " + ex.Message);
                }

                Console.WriteLine("✅ Sample set query completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error: " + ex.Message);
            }
            finally
            {
                // Cleanup COM objects
                if (instrument != null) Marshal.ReleaseComObject(instrument);
                if (project != null) Marshal.ReleaseComObject(project);
                
                Console.WriteLine("✅ COM cleanup completed");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
