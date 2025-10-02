using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

class EmpowerToolkitComplete 
{
    private static Dictionary<string, string> LoadConfig(string configFile)
    {
        var config = new Dictionary<string, string>();
        
        if (!File.Exists(configFile))
        {
            Console.WriteLine("⚠ Config file not found: " + configFile);
            return config;
        }
        
        foreach (string line in File.ReadAllLines(configFile))
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("#") || trimmed.StartsWith("[") || string.IsNullOrEmpty(trimmed))
                continue;
                
            string[] parts = trimmed.Split('=');
            if (parts.Length == 2)
            {
                config[parts[0].Trim()] = parts[1].Trim();
            }
        }
        
        return config;
    }
    
    private static void TestProjectMethods(object projectObj, string username, string password, string database, string project)
    {
        Console.WriteLine("\n=== Testing Project Methods ===");
        
        try
        {
            // Test Projects property
            Console.WriteLine("Testing Projects property...");
            var projectsObj = projectObj.GetType().InvokeMember(
                "Projects",
                BindingFlags.GetProperty,
                null,
                projectObj,
                null
            );
            
            if (projectsObj != null && projectsObj != System.DBNull.Value)
            {
                string[] projects = (string[])projectsObj;
                Console.WriteLine("✅ Found " + projects.Length + " projects:");
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
            Console.WriteLine($"⚠ Projects property failed: {ex.Message}");
        }
        
        try
        {
            // Test Services property
            Console.WriteLine("\nTesting Services property...");
            var servicesObj = projectObj.GetType().InvokeMember(
                "Services",
                BindingFlags.GetProperty,
                null,
                projectObj,
                null
            );
            
            if (servicesObj != null && servicesObj != System.DBNull.Value)
            {
                string[] services = (string[])servicesObj;
                Console.WriteLine($"✅ Found {services.Length} services:");
                foreach (string service in services)
                {
                    Console.WriteLine($"  - {service}");
                }
            }
            else
            {
                Console.WriteLine("⚠ No services found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Services property failed: {ex.Message}");
        }
    }
    
    private static void TestInstrumentMethods(object instrument)
    {
        Console.WriteLine("\n=== Testing Instrument Methods ===");
        
        // Test Systems property
        try
        {
            Console.WriteLine("Testing Systems property...");
            var systemsObj = instrument.GetType().InvokeMember(
                "Systems",
                BindingFlags.GetProperty,
                null,
                instrument,
                null
            );
            
            if (systemsObj != null && systemsObj != System.DBNull.Value)
            {
                string[] systems = (string[])systemsObj;
                Console.WriteLine($"✅ Found {systems.Length} systems:");
                foreach (string system in systems)
                {
                    Console.WriteLine($"  - {system}");
                }
            }
            else
            {
                Console.WriteLine("⚠ No systems found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Systems property failed: {ex.Message}");
        }
        
        // Test AcqServers property
        try
        {
            Console.WriteLine("\nTesting AcqServers property...");
            var serversObj = instrument.GetType().InvokeMember(
                "AcqServers",
                BindingFlags.GetProperty,
                null,
                instrument,
                null
            );
            
            if (serversObj != null && serversObj != System.DBNull.Value)
            {
                string[] servers = (string[])serversObj;
                Console.WriteLine($"✅ Found {servers.Length} acquisition servers:");
                foreach (string server in servers)
                {
                    Console.WriteLine($"  - {server}");
                }
            }
            else
            {
                Console.WriteLine("⚠ No acquisition servers found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ AcqServers property failed: {ex.Message}");
        }
        
        // Test SampleSetMethods property
        try
        {
            Console.WriteLine("\nTesting SampleSetMethods property...");
            var methodsObj = instrument.GetType().InvokeMember(
                "SampleSetMethods",
                BindingFlags.GetProperty,
                null,
                instrument,
                null
            );
            
            if (methodsObj != null && methodsObj != System.DBNull.Value)
            {
                string[] methods = (string[])methodsObj;
                Console.WriteLine($"✅ Found {methods.Length} sample set methods:");
                foreach (string method in methods)
                {
                    Console.WriteLine($"  - {method}");
                }
            }
            else
            {
                Console.WriteLine("⚠ No sample set methods found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ SampleSetMethods property failed: {ex.Message}");
        }
        
        // Test IsConnected property
        try
        {
            Console.WriteLine("\nTesting IsConnected property...");
            var connectedObj = instrument.GetType().InvokeMember(
                "IsConnected",
                BindingFlags.GetProperty,
                null,
                instrument,
                null
            );
            
            bool isConnected = (bool)connectedObj;
            Console.WriteLine($"✅ IsConnected: {isConnected}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ IsConnected property failed: {ex.Message}");
        }
        
        // Test Status property
        try
        {
            Console.WriteLine("\nTesting Status property...");
            var statusObj = instrument.GetType().InvokeMember(
                "Status",
                BindingFlags.GetProperty,
                null,
                instrument,
                null
            );
            
            Console.WriteLine($"✅ Status: {statusObj}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Status property failed: {ex.Message}");
        }
        
        // Test Progress property
        try
        {
            Console.WriteLine("\nTesting Progress property...");
            var progressObj = instrument.GetType().InvokeMember(
                "Progress",
                BindingFlags.GetProperty,
                null,
                instrument,
                null
            );
            
            Console.WriteLine($"✅ Progress: {progressObj}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Progress property failed: {ex.Message}");
        }
    }
    
    private static void TestConnectionMethods(object instrument, string nodeName, string systemName)
    {
        Console.WriteLine("\n=== Testing Connection Methods ===");
        
        try
        {
            Console.WriteLine($"Testing Connect(\"{nodeName}\", \"{systemName}\")...");
            instrument.GetType().InvokeMember(
                "Connect",
                BindingFlags.InvokeMethod,
                null,
                instrument,
                new object[] { nodeName, systemName }
            );
            
            Console.WriteLine("✅ Connect method called successfully");
            
            // Wait for connection and test ConnectionStatus
            bool done = false;
            int attempts = 0;
            while (!done && attempts < 10)
            {
                try
                {
                    var connectionStatus = instrument.GetType().InvokeMember(
                        "ConnectionStatus",
                        BindingFlags.GetProperty,
                        null,
                        instrument,
                        null
                    );
                    
                    var doneProperty = connectionStatus.GetType().InvokeMember(
                        "Done",
                        BindingFlags.GetProperty,
                        null,
                        connectionStatus,
                        null
                    );
                    
                    done = (bool)doneProperty;
                    
                    if (!done)
                    {
                        Console.WriteLine($"Connection attempt {attempts + 1}, waiting...");
                        System.Threading.Thread.Sleep(1000);
                        attempts++;
                    }
                    else
                    {
                        var statusText = connectionStatus.GetType().InvokeMember(
                            "Text",
                            BindingFlags.GetProperty,
                            null,
                            connectionStatus,
                            null
                        );
                        
                        string text = statusText.ToString();
                        Console.WriteLine($"✅ Connection completed: {text}");
                        
                        // Test other connection methods if connected
                        if (text.Contains("Successfully connected") || text.Length == 0)
                        {
                            TestOperationalMethods(instrument);
                        }
                    }
                }
                catch (Exception statusEx)
                {
                    Console.WriteLine($"⚠ ConnectionStatus check failed: {statusEx.Message}");
                    break;
                }
            }
            
            // Test Disconnect method
            Console.WriteLine("\nTesting Disconnect method...");
            instrument.GetType().InvokeMember(
                "Disconnect",
                BindingFlags.InvokeMethod,
                null,
                instrument,
                null
            );
            Console.WriteLine("✅ Disconnect method called successfully");
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Connection methods failed: {ex.Message}");
        }
    }
    
    private static void TestOperationalMethods(object instrument)
    {
        Console.WriteLine("\n=== Testing Operational Methods ===");
        
        // Test Stop method
        try
        {
            Console.WriteLine("Testing Stop method...");
            instrument.GetType().InvokeMember(
                "Stop",
                BindingFlags.InvokeMethod,
                null,
                instrument,
                null
            );
            Console.WriteLine("✅ Stop method available");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Stop method failed: {ex.Message}");
        }
        
        // Test Pause method
        try
        {
            Console.WriteLine("Testing Pause method...");
            instrument.GetType().InvokeMember(
                "Pause",
                BindingFlags.InvokeMethod,
                null,
                instrument,
                null
            );
            Console.WriteLine("✅ Pause method available");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Pause method failed: {ex.Message}");
        }
        
        // Test Resume method
        try
        {
            Console.WriteLine("Testing Resume method...");
            instrument.GetType().InvokeMember(
                "Resume",
                BindingFlags.InvokeMethod,
                null,
                instrument,
                null
            );
            Console.WriteLine("✅ Resume method available");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Resume method failed: {ex.Message}");
        }
        
        // Test ClearQueue method
        try
        {
            Console.WriteLine("Testing ClearQueue method...");
            instrument.GetType().InvokeMember(
                "ClearQueue",
                BindingFlags.InvokeMethod,
                null,
                instrument,
                null
            );
            Console.WriteLine("✅ ClearQueue method available");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ ClearQueue method failed: {ex.Message}");
        }
        
        // Test StartQueue method
        try
        {
            Console.WriteLine("Testing StartQueue method...");
            instrument.GetType().InvokeMember(
                "StartQueue",
                BindingFlags.InvokeMethod,
                null,
                instrument,
                null
            );
            Console.WriteLine("✅ StartQueue method available");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ StartQueue method failed: {ex.Message}");
        }
        
        // Test StopQueue method
        try
        {
            Console.WriteLine("Testing StopQueue method...");
            instrument.GetType().InvokeMember(
                "StopQueue",
                BindingFlags.InvokeMethod,
                null,
                instrument,
                null
            );
            Console.WriteLine("✅ StopQueue method available");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ StopQueue method failed: {ex.Message}");
        }
    }
    
    private static void TestSampleSetMethods(object sampleSetMethod)
    {
        Console.WriteLine("\n=== Testing SampleSetMethod Object ===");
        
        // Test SampleSetMethodNames property
        try
        {
            Console.WriteLine("Testing SampleSetMethodNames property...");
            var methodNamesObj = sampleSetMethod.GetType().InvokeMember(
                "SampleSetMethodNames",
                BindingFlags.GetProperty,
                null,
                sampleSetMethod,
                null
            );
            
            if (methodNamesObj != null && methodNamesObj != System.DBNull.Value)
            {
                string[] methodNames = (string[])methodNamesObj;
                Console.WriteLine($"✅ Found {methodNames.Length} sample set method names:");
                foreach (string methodName in methodNames)
                {
                    Console.WriteLine($"  - {methodName}");
                }
                
                // Test Load method with first available method
                if (methodNames.Length > 0)
                {
                    try
                    {
                        Console.WriteLine($"\nTesting Load method with '{methodNames[0]}'...");
                        sampleSetMethod.GetType().InvokeMember(
                            "Load",
                            BindingFlags.InvokeMethod,
                            null,
                            sampleSetMethod,
                            new object[] { methodNames[0] }
                        );
                        Console.WriteLine("✅ Load method available");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠ Load method failed: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("⚠ No sample set method names found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ SampleSetMethodNames property failed: {ex.Message}");
        }
        
        // Test Save method
        try
        {
            Console.WriteLine("\nTesting Save method...");
            sampleSetMethod.GetType().InvokeMember(
                "Save",
                BindingFlags.InvokeMethod,
                null,
                sampleSetMethod,
                new object[] { "TestMethod" }
            );
            Console.WriteLine("✅ Save method available");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Save method failed: {ex.Message}");
        }
        
        // Test Delete method
        try
        {
            Console.WriteLine("Testing Delete method...");
            sampleSetMethod.GetType().InvokeMember(
                "Delete",
                BindingFlags.InvokeMethod,
                null,
                sampleSetMethod,
                new object[] { "TestMethod" }
            );
            Console.WriteLine("✅ Delete method available");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Delete method failed: {ex.Message}");
        }
    }
    
    static void Main()
    {
        Console.WriteLine("Waters Empower Toolkit - Complete Method Testing");
        Console.WriteLine("===============================================");
        
        // Load configuration
        var config = LoadConfig("secrets.ini");
        
        string username = config.ContainsKey("username") ? config["username"] : "system";
        string password = config.ContainsKey("password") ? config["password"] : "manager";
        string database = config.ContainsKey("database") ? config["database"] : "";
        string project = config.ContainsKey("project") ? config["project"] : "Waters GPC Training";
        string system = config.ContainsKey("system") ? config["system"] : "Arc HPLC";
        string node = config.ContainsKey("node") ? config["node"] : "Waters-h4q6k34";
        
        Console.WriteLine("Configuration:");
        Console.WriteLine($"  Username: {username}");
        Console.WriteLine($"  Database: {(string.IsNullOrEmpty(database) ? "(empty)" : database)}");
        Console.WriteLine($"  Project: {project}");
        Console.WriteLine($"  System: {system}");
        Console.WriteLine($"  Node: {node}");
        Console.WriteLine();
        
        object projectObj = null;
        object instrument = null;
        object sampleSetMethod = null;
        
        try 
        {
            // Create and test Project object
            Console.WriteLine("=== Creating Project Object ===");
            var projectType = Type.GetTypeFromProgID("MillenniumToolkit.Project");
            projectObj = Activator.CreateInstance(projectType);
            Console.WriteLine("✅ Project object created");
            
            // Login
            Console.WriteLine("\n=== Testing Login ===");
            projectObj.GetType().InvokeMember(
                "Login",
                BindingFlags.InvokeMethod,
                null,
                projectObj,
                new object[] { database, project, username, password }
            );
            Console.WriteLine("✅ Login successful");
            
            // Test Project methods
            TestProjectMethods(projectObj, username, password, database, project);
            
            // Create and test Instrument object
            Console.WriteLine("\n=== Creating Instrument Object ===");
            var instrumentType = Type.GetTypeFromProgID("MillenniumToolkit.Instrument");
            instrument = Activator.CreateInstance(instrumentType);
            Console.WriteLine("✅ Instrument object created");
            
            // Test Instrument methods
            TestInstrumentMethods(instrument);
            
            // Test connection methods
            TestConnectionMethods(instrument, node, system);
            
            // Create and test SampleSetMethod object
            Console.WriteLine("\n=== Creating SampleSetMethod Object ===");
            var sampleSetMethodType = Type.GetTypeFromProgID("MillenniumToolkit.SampleSetMethod");
            sampleSetMethod = Activator.CreateInstance(sampleSetMethodType);
            Console.WriteLine("✅ SampleSetMethod object created");
            
            // Test SampleSetMethod methods
            TestSampleSetMethods(sampleSetMethod);
            
            Console.WriteLine("\n=== Complete Method Testing Summary ===");
            Console.WriteLine("✅ All COM objects created successfully");
            Console.WriteLine("✅ All available methods tested");
            Console.WriteLine("✅ Ready for Python wrapper implementation");
            
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"❌ Fatal Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }
        finally 
        {
            // Cleanup
            if (instrument != null)
            {
                try
                {
                    instrument.GetType().InvokeMember("Disconnect", BindingFlags.InvokeMethod, null, instrument, null);
                }
                catch (Exception) { }
                Marshal.ReleaseComObject(instrument);
            }
            if (sampleSetMethod != null) Marshal.ReleaseComObject(sampleSetMethod);
            if (projectObj != null) 
            {
                try
                {
                    projectObj.GetType().InvokeMember("Logoff", BindingFlags.InvokeMethod, null, projectObj, null);
                }
                catch (Exception) { }
                Marshal.ReleaseComObject(projectObj);
            }
            Console.WriteLine("\n✅ COM cleanup completed");
        }
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
