using System;
using System.Collections.Generic;
using WatersEmpowerToolkit;

namespace WatersEmpowerTest
{
    /// <summary>
    /// Comprehensive test application for Waters Empower Toolkit
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Waters Empower Toolkit - Comprehensive Test Suite");
            Console.WriteLine("===============================================");
            Console.WriteLine();

            try
            {
                // Test with console input handling
                using (EmpowerToolkit toolkit = new EmpowerToolkit())
                {
                    // Initialize toolkit
                    Console.WriteLine("Initializing Empower Toolkit...");
                    toolkit.Initialize();
                    Console.WriteLine("✅ Initialization successful");
                    Console.WriteLine();

                    // Run comprehensive diagnostics first
                    Console.WriteLine("Running diagnostics...");
                    string diagnostics = toolkit.RunDiagnostics();
                    Console.WriteLine(diagnostics);
                    Console.WriteLine();

                    // Test system discovery
                    TestSystemDiscovery(toolkit);
                    Console.WriteLine();

                    // Test instrument connection
                    TestInstrumentConnection(toolkit);
                    Console.WriteLine();

                    // Test all instrument operations
                    TestInstrumentOperations(toolkit);
                    Console.WriteLine();

                    // Test sample set method operations
                    TestSampleSetMethods(toolkit);
                    Console.WriteLine();

                    // Test project operations
                    TestProjectOperations(toolkit);
                    Console.WriteLine();

                    Console.WriteLine("All tests completed successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void TestSystemDiscovery(EmpowerToolkit toolkit)
        {
            Console.WriteLine("Testing System Discovery");
            Console.WriteLine("------------------------");

            try
            {
                Dictionary<string, string[]> discovery = toolkit.DiscoverSystems();

                Console.WriteLine("Systems discovered: " + discovery["systems"].Length);
                foreach (string system in discovery["systems"])
                {
                    Console.WriteLine("  - " + system);
                }

                Console.WriteLine("Nodes discovered: " + discovery["nodes"].Length);
                foreach (string node in discovery["nodes"])
                {
                    Console.WriteLine("  - " + node);
                }

                Console.WriteLine("Methods discovered: " + discovery["methods"].Length);
                if (discovery["methods"].Length > 0)
                {
                    Console.WriteLine("First few methods:");
                    for (int i = 0; i < Math.Min(5, discovery["methods"].Length); i++)
                    {
                        Console.WriteLine("  - " + discovery["methods"][i]);
                    }
                    if (discovery["methods"].Length > 5)
                    {
                        Console.WriteLine("  ... and " + (discovery["methods"].Length - 5) + " more");
                    }
                }

                Console.WriteLine("✅ System discovery test completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ System discovery failed: " + ex.Message);
            }
        }

        static void TestInstrumentConnection(EmpowerToolkit toolkit)
        {
            Console.WriteLine("Testing Instrument Connection");
            Console.WriteLine("-----------------------------");

            try
            {
                Console.WriteLine("Connecting to instrument...");
                bool connected = toolkit.ConnectInstrument();

                if (connected)
                {
                    Console.WriteLine("✅ Connection successful");
                    Console.WriteLine("Connection status: " + toolkit.Instrument.IsConnected);
                    Console.WriteLine("Instrument status: " + toolkit.Instrument.Status);
                }
                else
                {
                    Console.WriteLine("❌ Connection failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Connection test failed: " + ex.Message);
            }
        }

        static void TestInstrumentOperations(EmpowerToolkit toolkit)
        {
            Console.WriteLine("Testing Instrument Operations");
            Console.WriteLine("-----------------------------");

            if (!toolkit.Instrument.IsConnected)
            {
                Console.WriteLine("⚠ Skipping instrument operations - not connected");
                return;
            }

            try
            {
                // Test status and progress
                Console.WriteLine("Current status: " + toolkit.Instrument.Status);
                Console.WriteLine("Current progress: " + toolkit.Instrument.Progress);

                // Test sample set methods
                string[] methods = toolkit.Instrument.SampleSetMethods;
                Console.WriteLine("Available sample set methods: " + methods.Length);

                if (methods.Length > 0)
                {
                    // Test with first available method
                    string testMethod = methods[0];
                    Console.WriteLine("Testing with method: " + testMethod);

                    // Note: In a real test, you might want to comment out actual execution
                    Console.WriteLine("Would execute: ReplaceSampleSet(\"" + testMethod + "\")");
                    // toolkit.Instrument.ReplaceSampleSet(testMethod);

                    // Test control operations
                    Console.WriteLine("Testing control operations...");
                    Console.WriteLine("Would test: Pause, Resume, Stop operations");
                    // toolkit.Instrument.Pause();
                    // toolkit.Instrument.Resume();
                    // toolkit.Instrument.Stop();

                    // Test queue operations
                    Console.WriteLine("Testing queue operations...");
                    Console.WriteLine("Would test: QueueSampleSet, StartQueue, StopQueue, ClearQueue");
                    // toolkit.Instrument.QueueSampleSet("TestSampleSet");
                    // toolkit.Instrument.StartQueue();
                    // toolkit.Instrument.StopQueue();
                    // toolkit.Instrument.ClearQueue();
                }

                Console.WriteLine("✅ Instrument operations test completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Instrument operations failed: " + ex.Message);
            }
        }

        static void TestSampleSetMethods(EmpowerToolkit toolkit)
        {
            Console.WriteLine("Testing Sample Set Method Operations");
            Console.WriteLine("-----------------------------------");

            try
            {
                string[] methodNames = toolkit.SampleSetMethod.MethodNames;
                Console.WriteLine("Sample set methods available: " + methodNames.Length);

                if (methodNames.Length > 0)
                {
                    string testMethod = methodNames[0];
                    Console.WriteLine("Testing with method: " + testMethod);

                    // Test load
                    Console.WriteLine("Loading method...");
                    toolkit.SampleSetMethod.Load(testMethod);
                    Console.WriteLine("✅ Method loaded successfully");

                    // Note: Save and Delete operations would modify the system
                    Console.WriteLine("Would test: Save and Delete operations");
                    // toolkit.SampleSetMethod.Save("TestMethod_Copy");
                    // toolkit.SampleSetMethod.Delete("TestMethod_Copy");
                }

                Console.WriteLine("✅ Sample set method operations test completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Sample set method operations failed: " + ex.Message);
            }
        }

        static void TestProjectOperations(EmpowerToolkit toolkit)
        {
            Console.WriteLine("Testing Project Operations");
            Console.WriteLine("--------------------------");

            try
            {
                Console.WriteLine("Project connected: " + toolkit.Project.IsConnected);

                if (toolkit.Project.IsConnected)
                {
                    // Test project information
                    string[] projects = toolkit.Project.Projects;
                    Console.WriteLine("Available projects: " + projects.Length);
                    foreach (string project in projects)
                    {
                        Console.WriteLine("  - " + project);
                    }

                    string[] services = toolkit.Project.Services;
                    Console.WriteLine("Available services: " + services.Length);
                    foreach (string service in services)
                    {
                        Console.WriteLine("  - " + service);
                    }

                    // Test error handling
                    string errorDesc = toolkit.Project.GetErrorDescription(0);
                    Console.WriteLine("Error description for code 0: " + errorDesc);
                }

                Console.WriteLine("✅ Project operations test completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Project operations failed: " + ex.Message);
            }
        }
    }
}
