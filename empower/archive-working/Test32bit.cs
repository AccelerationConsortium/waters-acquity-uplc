using System;
using WatersEmpowerToolkit;

class Program 
{
    static void Main()
    {
        Console.WriteLine("Waters Empower Toolkit - 32-bit Test");
        Console.WriteLine("====================================");
        
        try 
        {
            using (EmpowerToolkit toolkit = new EmpowerToolkit())
            {
                Console.WriteLine("Initializing toolkit...");
                toolkit.Initialize();
                Console.WriteLine("✅ Toolkit initialized successfully");
                
                Console.WriteLine("Discovering systems...");
                var discovery = toolkit.DiscoverSystems();
                
                Console.WriteLine("Systems found: " + discovery["systems"].Length);
                Console.WriteLine("Nodes found: " + discovery["nodes"].Length);
                Console.WriteLine("Methods found: " + discovery["methods"].Length);
                
                if (discovery["systems"].Length > 0)
                {
                    Console.WriteLine("Available Systems:");
                    foreach (string system in discovery["systems"])
                    {
                        Console.WriteLine("  - " + system);
                    }
                }
                
                if (discovery["nodes"].Length > 0)
                {
                    Console.WriteLine("Available Nodes:");
                    foreach (string node in discovery["nodes"])
                    {
                        Console.WriteLine("  - " + node);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error: " + ex.Message);
        }
        
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
