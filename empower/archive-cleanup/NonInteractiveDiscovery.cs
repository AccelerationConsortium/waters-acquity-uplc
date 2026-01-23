using System;

class Program
{
    static void Main()
    {
        try
        {
            Console.WriteLine("Waters Empower System Discovery");
            Console.WriteLine("===============================");

            // Create COM objects
            Type projectType = Type.GetTypeFromProgID("MillenniumToolkit.Project");
            object project = Activator.CreateInstance(projectType);

            Type instrumentType = Type.GetTypeFromProgID("MillenniumToolkit.Instrument");
            object instrument = Activator.CreateInstance(instrumentType);

            Type sampleSetMethodType = Type.GetTypeFromProgID("MillenniumToolkit.SampleSetMethod");
            object sampleSetMethod = Activator.CreateInstance(sampleSetMethodType);

            Console.WriteLine("✅ COM objects created successfully");

            // Login to project
            project.GetType().InvokeMember(
                "Login",
                System.Reflection.BindingFlags.InvokeMethod,
                null,
                project,
                new object[] { "", "Waters GPC Training", "system", "manager" }
            );
            Console.WriteLine("✅ Project login successful");

            // Get available systems
            object systemsObj = instrument.GetType().InvokeMember(
                "Systems",
                System.Reflection.BindingFlags.GetProperty,
                null,
                instrument,
                null
            );

            Console.WriteLine();
            Console.WriteLine("Available Systems:");
            if (systemsObj != null && systemsObj != System.DBNull.Value)
            {
                string[] systems = (string[])systemsObj;
                foreach (string system in systems)
                {
                    Console.WriteLine("- " + system);
                }
            }

            // Get available acquisition servers
            object serversObj = instrument.GetType().InvokeMember(
                "AcqServers",
                System.Reflection.BindingFlags.GetProperty,
                null,
                instrument,
                null
            );

            Console.WriteLine();
            Console.WriteLine("Available Acquisition Servers:");
            if (serversObj != null && serversObj != System.DBNull.Value)
            {
                string[] servers = (string[])serversObj;
                foreach (string server in servers)
                {
                    Console.WriteLine("- " + server);
                }
            }

            // Get sample set methods
            object methodsObj = sampleSetMethod.GetType().InvokeMember(
                "SampleSetMethodNames",
                System.Reflection.BindingFlags.GetProperty,
                null,
                sampleSetMethod,
                null
            );

            Console.WriteLine();
            Console.WriteLine("Available Sample Set Methods:");
            if (methodsObj != null && methodsObj != System.DBNull.Value)
            {
                string[] methods = (string[])methodsObj;
                foreach (string method in methods)
                {
                    Console.WriteLine("- " + method);
                }
            }

            // Clean up
            project.GetType().InvokeMember(
                "Logoff",
                System.Reflection.BindingFlags.InvokeMethod,
                null,
                project,
                null
            );

            Console.WriteLine();
            Console.WriteLine("✅ Discovery completed successfully");
            
            // NO Console.ReadKey() - Exit cleanly for subprocess calls
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error: " + ex.Message);
            Console.WriteLine("Stack Trace: " + ex.StackTrace);
            
            // Exit with error code for subprocess detection
            Environment.Exit(1);
        }
    }
}
