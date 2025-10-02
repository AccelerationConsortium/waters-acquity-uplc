using System;
using System.Reflection;

class Program
{
    // COM objects
    static object project = null;
    static object instrument = null;
    static object sampleSetMethod = null;
    
    // Connection state
    static bool projectLoggedIn = false;
    static bool instrumentConnected = false;

    static void Main()
    {
        Console.WriteLine("Waters Empower Toolkit - Comprehensive Function Test");
        Console.WriteLine("=================================================");
        Console.WriteLine();

        try
        {
            // Step 1: Create COM objects
            TestCreateCOMObjects();
            Console.WriteLine();

            // Step 2: Test authentication
            TestAuthentication();
            Console.WriteLine();

            // Step 3: Test system discovery
            TestSystemDiscovery();
            Console.WriteLine();

            // Step 4: Test instrument connection
            TestInstrumentConnection();
            Console.WriteLine();

            // Step 5: Test all instrument operations
            TestInstrumentOperations();
            Console.WriteLine();

            // Step 6: Test sample set method operations
            TestSampleSetMethodOperations();
            Console.WriteLine();

            // Step 7: Test project operations
            TestProjectOperations();
            Console.WriteLine();

            Console.WriteLine("✅ All function tests completed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Critical error: " + ex.Message);
            Console.WriteLine("Stack trace: " + ex.StackTrace);
        }
        finally
        {
            // Cleanup
            Cleanup();
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    static void TestCreateCOMObjects()
    {
        Console.WriteLine("🔧 Testing COM Object Creation");
        Console.WriteLine("------------------------------");

        try
        {
            // Create Project COM object
            Console.Write("Creating MillenniumToolkit.Project... ");
            Type projectType = Type.GetTypeFromProgID("MillenniumToolkit.Project");
            project = Activator.CreateInstance(projectType);
            Console.WriteLine("✅ Success");

            // Create Instrument COM object  
            Console.Write("Creating MillenniumToolkit.Instrument... ");
            Type instrumentType = Type.GetTypeFromProgID("MillenniumToolkit.Instrument");
            instrument = Activator.CreateInstance(instrumentType);
            Console.WriteLine("✅ Success");

            // Create SampleSetMethod COM object
            Console.Write("Creating MillenniumToolkit.SampleSetMethod... ");
            Type sampleSetMethodType = Type.GetTypeFromProgID("MillenniumToolkit.SampleSetMethod");
            sampleSetMethod = Activator.CreateInstance(sampleSetMethodType);
            Console.WriteLine("✅ Success");

            Console.WriteLine("🎉 All COM objects created successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Failed");
            Console.WriteLine("Error: " + ex.Message);
            throw;
        }
    }

    static void TestAuthentication()
    {
        Console.WriteLine("🔐 Testing Authentication");
        Console.WriteLine("-------------------------");

        try
        {
            Console.Write("Logging in to project... ");
            project.GetType().InvokeMember(
                "Login",
                BindingFlags.InvokeMethod,
                null,
                project,
                new object[] { "", "Waters GPC Training", "system", "manager" }
            );
            projectLoggedIn = true;
            Console.WriteLine("✅ Login successful");

            // Test error description function
            Console.Write("Testing error description function... ");
            object errorDesc = project.GetType().InvokeMember(
                "TkErrorDescription",
                BindingFlags.InvokeMethod,
                null,
                project,
                new object[] { 0 }
            );
            Console.WriteLine("✅ Error description: " + errorDesc.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Authentication failed");
            Console.WriteLine("Error: " + ex.Message);
            throw;
        }
    }

    static void TestSystemDiscovery()
    {
        Console.WriteLine("🔍 Testing System Discovery");
        Console.WriteLine("---------------------------");

        try
        {
            // Test available systems
            Console.Write("Getting available systems... ");
            object systemsObj = instrument.GetType().InvokeMember(
                "Systems",
                BindingFlags.GetProperty,
                null,
                instrument,
                null
            );

            if (systemsObj != null && systemsObj != System.DBNull.Value)
            {
                string[] systems = (string[])systemsObj;
                Console.WriteLine("✅ Found " + systems.Length + " systems");
                
                Console.WriteLine("Available Systems:");
                foreach (string system in systems)
                {
                    Console.WriteLine("  - " + system);
                }
            }
            else
            {
                Console.WriteLine("⚠ No systems found");
            }

            // Test available acquisition servers
            Console.Write("Getting acquisition servers... ");
            object serversObj = instrument.GetType().InvokeMember(
                "AcqServers",
                BindingFlags.GetProperty,
                null,
                instrument,
                null
            );

            if (serversObj != null && serversObj != System.DBNull.Value)
            {
                string[] servers = (string[])serversObj;
                Console.WriteLine("✅ Found " + servers.Length + " acquisition servers");
                
                Console.WriteLine("Available Acquisition Servers:");
                foreach (string server in servers)
                {
                    Console.WriteLine("  - " + server);
                }
            }
            else
            {
                Console.WriteLine("⚠ No acquisition servers found");
            }

            // Test sample set method names
            Console.Write("Getting sample set method names... ");
            object methodsObj = sampleSetMethod.GetType().InvokeMember(
                "SampleSetMethodNames",
                BindingFlags.GetProperty,
                null,
                sampleSetMethod,
                null
            );

            if (methodsObj != null && methodsObj != System.DBNull.Value)
            {
                string[] methods = (string[])methodsObj;
                Console.WriteLine("✅ Found " + methods.Length + " methods");
                
                Console.WriteLine("Available Sample Set Methods (first 10):");
                for (int i = 0; i < Math.Min(10, methods.Length); i++)
                {
                    Console.WriteLine("  - " + methods[i]);
                }
                if (methods.Length > 10)
                {
                    Console.WriteLine("  ... and " + (methods.Length - 10) + " more");
                }
            }
            else
            {
                Console.WriteLine("⚠ No sample set methods found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ System discovery failed");
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static void TestInstrumentConnection()
    {
        Console.WriteLine("🔗 Testing Instrument Connection");
        Console.WriteLine("--------------------------------");

        try
        {
            // Test connection with known system and node
            string nodeName = "Waters-h4q6k34";  // From our discovery
            string systemName = "Arc HPLC";      // From our discovery

            Console.Write("Connecting to instrument (" + nodeName + ", " + systemName + ")... ");
            
            // Initiate connection
            instrument.GetType().InvokeMember(
                "Connect",
                BindingFlags.InvokeMethod,
                null,
                instrument,
                new object[] { nodeName, systemName }
            );

            // Wait for connection to complete with status monitoring
            bool connected = false;
            int attempts = 0;
            int maxAttempts = 30;

            while (attempts < maxAttempts && !connected)
            {
                try
                {
                    // Check connection status
                    object statusObj = instrument.GetType().InvokeMember(
                        "ConnectionStatus",
                        BindingFlags.GetProperty,
                        null,
                        instrument,
                        null
                    );

                    object doneProperty = statusObj.GetType().InvokeMember(
                        "Done",
                        BindingFlags.GetProperty,
                        null,
                        statusObj,
                        null
                    );

                    bool isDone = (bool)doneProperty;

                    if (isDone)
                    {
                        object textProperty = statusObj.GetType().InvokeMember(
                            "Text",
                            BindingFlags.GetProperty,
                            null,
                            statusObj,
                            null
                        );

                        string statusText = textProperty.ToString();
                        
                        if (statusText == "Successfully connected to instrument server" || string.IsNullOrEmpty(statusText))
                        {
                            connected = true;
                            instrumentConnected = true;
                            Console.WriteLine("✅ Connection successful");
                        }
                        else
                        {
                            Console.WriteLine("❌ Connection failed: " + statusText);
                            return;
                        }
                    }
                    else
                    {
                        // Still connecting, wait a bit
                        System.Threading.Thread.Sleep(1000);
                        attempts++;
                        
                        if (attempts % 5 == 0)
                        {
                            Console.Write(".");
                        }
                    }
                }
                catch (Exception)
                {
                    // Connection status might not be available, try IsConnected instead
                    System.Threading.Thread.Sleep(1000);
                    attempts++;
                }
            }

            if (!connected && attempts >= maxAttempts)
            {
                Console.WriteLine("⚠ Connection timeout after " + maxAttempts + " seconds");
            }

            // Test IsConnected property
            Console.Write("Checking IsConnected property... ");
            try
            {
                object isConnectedObj = instrument.GetType().InvokeMember(
                    "IsConnected",
                    BindingFlags.GetProperty,
                    null,
                    instrument,
                    null
                );
                bool isConnected = (bool)isConnectedObj;
                Console.WriteLine("✅ IsConnected = " + isConnected);
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠ Could not check IsConnected: " + ex.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Connection test failed");
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static void TestInstrumentOperations()
    {
        Console.WriteLine("⚙️ Testing Instrument Operations");
        Console.WriteLine("--------------------------------");

        if (!instrumentConnected)
        {
            Console.WriteLine("⚠ Skipping instrument operations - not connected");
            return;
        }

        try
        {
            // Test Status property
            Console.Write("Getting instrument status... ");
            try
            {
                object statusObj = instrument.GetType().InvokeMember(
                    "Status",
                    BindingFlags.GetProperty,
                    null,
                    instrument,
                    null
                );
                Console.WriteLine("✅ Status: " + statusObj.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠ Status unavailable: " + ex.Message);
            }

            // Test Progress property
            Console.Write("Getting instrument progress... ");
            try
            {
                object progressObj = instrument.GetType().InvokeMember(
                    "Progress",
                    BindingFlags.GetProperty,
                    null,
                    instrument,
                    null
                );
                Console.WriteLine("✅ Progress: " + progressObj.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠ Progress unavailable: " + ex.Message);
            }

            // Test SampleSetMethods property
            Console.Write("Getting sample set methods from instrument... ");
            try
            {
                object methodsObj = instrument.GetType().InvokeMember(
                    "SampleSetMethods",
                    BindingFlags.GetProperty,
                    null,
                    instrument,
                    null
                );

                if (methodsObj != null && methodsObj != System.DBNull.Value)
                {
                    string[] methods = (string[])methodsObj;
                    Console.WriteLine("✅ Found " + methods.Length + " methods from instrument");

                    // Test Replace method (but don't actually execute for safety)
                    if (methods.Length > 0)
                    {
                        Console.WriteLine("Would test Replace with method: " + methods[0]);
                        Console.WriteLine("⚠ Skipping actual Replace execution for safety");

                        // Uncomment to actually test Replace (BE CAREFUL!)
                        // Console.Write("Testing Replace method... ");
                        // instrument.GetType().InvokeMember(
                        //     "Replace",
                        //     BindingFlags.InvokeMethod,
                        //     null,
                        //     instrument,
                        //     new object[] { methods[0] }
                        // );
                        // Console.WriteLine("✅ Replace executed");
                    }
                }
                else
                {
                    Console.WriteLine("⚠ No methods available from instrument");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠ Could not get methods: " + ex.Message);
            }

            // Test control methods (but don't actually execute)
            Console.WriteLine("Testing control method availability:");
            
            Console.WriteLine("  Stop method available: " + TestMethodExists(instrument, "Stop"));
            Console.WriteLine("  Pause method available: " + TestMethodExists(instrument, "Pause"));
            Console.WriteLine("  Resume method available: " + TestMethodExists(instrument, "Resume"));
            
            // Test queue methods
            Console.WriteLine("Testing queue method availability:");
            Console.WriteLine("  QueueSampleSet method available: " + TestMethodExists(instrument, "QueueSampleSet"));
            Console.WriteLine("  StartQueue method available: " + TestMethodExists(instrument, "StartQueue"));
            Console.WriteLine("  StopQueue method available: " + TestMethodExists(instrument, "StopQueue"));
            Console.WriteLine("  ClearQueue method available: " + TestMethodExists(instrument, "ClearQueue"));
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Instrument operations test failed");
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static void TestSampleSetMethodOperations()
    {
        Console.WriteLine("📋 Testing Sample Set Method Operations");
        Console.WriteLine("--------------------------------------");

        try
        {
            // Test Load method (with first available method)
            object methodsObj = sampleSetMethod.GetType().InvokeMember(
                "SampleSetMethodNames",
                BindingFlags.GetProperty,
                null,
                sampleSetMethod,
                null
            );

            if (methodsObj != null && methodsObj != System.DBNull.Value)
            {
                string[] methods = (string[])methodsObj;
                if (methods.Length > 0)
                {
                    string testMethod = methods[0];
                    Console.Write("Testing Load method with '" + testMethod + "'... ");
                    
                    try
                    {
                        sampleSetMethod.GetType().InvokeMember(
                            "Load",
                            BindingFlags.InvokeMethod,
                            null,
                            sampleSetMethod,
                            new object[] { testMethod }
                        );
                        Console.WriteLine("✅ Load successful");

                        // Test Save and Delete (but don't actually execute for safety)
                        Console.WriteLine("Save method available: " + TestMethodExists(sampleSetMethod, "Save"));
                        Console.WriteLine("Delete method available: " + TestMethodExists(sampleSetMethod, "Delete"));
                        
                        Console.WriteLine("⚠ Skipping actual Save/Delete operations for safety");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("⚠ Load failed: " + ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("⚠ No methods available to test Load");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Sample set method operations test failed");
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static void TestProjectOperations()
    {
        Console.WriteLine("🏢 Testing Project Operations");
        Console.WriteLine("-----------------------------");

        try
        {
            // Test Projects property
            Console.Write("Getting available projects... ");
            try
            {
                object projectsObj = project.GetType().InvokeMember(
                    "Projects",
                    BindingFlags.GetProperty,
                    null,
                    project,
                    null
                );

                if (projectsObj != null && projectsObj != System.DBNull.Value)
                {
                    string[] projects = (string[])projectsObj;
                    Console.WriteLine("✅ Found " + projects.Length + " projects");
                    
                    foreach (string proj in projects)
                    {
                        Console.WriteLine("  - " + proj);
                    }
                }
                else
                {
                    Console.WriteLine("⚠ No projects found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠ Could not get projects: " + ex.Message);
            }

            // Test Services property
            Console.Write("Getting available services... ");
            try
            {
                object servicesObj = project.GetType().InvokeMember(
                    "Services",
                    BindingFlags.GetProperty,
                    null,
                    project,
                    null
                );

                if (servicesObj != null && servicesObj != System.DBNull.Value)
                {
                    string[] services = (string[])servicesObj;
                    Console.WriteLine("✅ Found " + services.Length + " services");
                    
                    foreach (string service in services)
                    {
                        Console.WriteLine("  - " + service);
                    }
                }
                else
                {
                    Console.WriteLine("⚠ No services found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠ Could not get services: " + ex.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Project operations test failed");
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static bool TestMethodExists(object obj, string methodName)
    {
        try
        {
            MethodInfo method = obj.GetType().GetMethod(methodName);
            return method != null;
        }
        catch
        {
            return false;
        }
    }

    static void Cleanup()
    {
        Console.WriteLine("🧹 Cleaning up...");
        
        try
        {
            if (instrumentConnected && instrument != null)
            {
                Console.Write("Disconnecting from instrument... ");
                instrument.GetType().InvokeMember(
                    "Disconnect",
                    BindingFlags.InvokeMethod,
                    null,
                    instrument,
                    null
                );
                Console.WriteLine("✅ Disconnected");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("⚠ Disconnect failed: " + ex.Message);
        }

        try
        {
            if (projectLoggedIn && project != null)
            {
                Console.Write("Logging off from project... ");
                project.GetType().InvokeMember(
                    "Logoff",
                    BindingFlags.InvokeMethod,
                    null,
                    project,
                    null
                );
                Console.WriteLine("✅ Logged off");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("⚠ Logoff failed: " + ex.Message);
        }

        Console.WriteLine("✅ Cleanup completed");
    }
}
