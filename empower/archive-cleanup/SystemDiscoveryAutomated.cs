using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

class SystemDiscoveryAutomated 
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
    
    static void Main(string[] args)
    {
        Console.WriteLine("System Discovery - Automated (Non-Interactive)");
        Console.WriteLine("==============================================");
        
        // Check for non-interactive mode
        bool nonInteractive = args.Length > 0 && args[0] == "--non-interactive";
        
        // Load configuration from secrets.ini
        var config = LoadConfig("secrets.ini");
        
        string username = config.ContainsKey("username") ? config["username"] : "system";
        string password = config.ContainsKey("password") ? config["password"] : "manager";
        string database = config.ContainsKey("database") ? config["database"] : "";
        string project = config.ContainsKey("project") ? config["project"] : "Waters GPC Training";
        
        Console.WriteLine("Configuration loaded:");
        Console.WriteLine("  Username: " + username);
        Console.WriteLine("  Database: " + (string.IsNullOrEmpty(database) ? "(empty)" : database));
        Console.WriteLine("  Project: " + project);
        Console.WriteLine();
        
        object projectObj = null;
        object instrument = null;
        object sampleSetMethod = null;
        
        try 
        {
            // Create Project object (official pattern)
            Console.WriteLine("Creating MillenniumToolkit.Project...");
            var projectType = Type.GetTypeFromProgID("MillenniumToolkit.Project");
            projectObj = Activator.CreateInstance(projectType);
            Console.WriteLine("✅ Project object created");
            
            // Login using official pattern: Login(database, project, username, password)
            Console.WriteLine("Attempting login...");
            projectObj.GetType().InvokeMember(
                "Login",
                BindingFlags.InvokeMethod,
                null,
                projectObj,
                new object[] { database, project, username, password }
            );
            Console.WriteLine("✅ Project login successful");
            
            // Create Instrument object (official pattern)
            Console.WriteLine("Creating MillenniumToolkit.Instrument...");
            var instrumentType = Type.GetTypeFromProgID("MillenniumToolkit.Instrument");
            instrument = Activator.CreateInstance(instrumentType);
            Console.WriteLine("✅ Instrument object created");
            
            // Discover available systems (official pattern: _instrument.Systems)
            Console.WriteLine("\nDiscovering available systems...");
            try 
            {
                var systemsObj = instrument.GetType().InvokeMember(
                    "Systems",
                    BindingFlags.GetProperty,
                    null,
                    instrument,
                    null
                );
                
                if (systemsObj is System.DBNull)
                {
                    Console.WriteLine("⚠ No systems available");
                }
                else
                {
                    string[] systems = (string[])systemsObj;
                    Console.WriteLine("✅ Available systems:");
                    foreach (string system in systems)
                    {
                        Console.WriteLine("  - " + system);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠ Systems discovery failed: " + ex.Message);
            }
            
            // Discover available nodes/acquisition servers (official pattern: _instrument.AcqServers)
            Console.WriteLine("\nDiscovering available acquisition servers...");
            try 
            {
                var nodesObj = instrument.GetType().InvokeMember(
                    "AcqServers",
                    BindingFlags.GetProperty,
                    null,
                    instrument,
                    null
                );
                
                if (nodesObj is System.DBNull)
                {
                    Console.WriteLine("⚠ No acquisition servers available");
                }
                else
                {
                    string[] nodes = (string[])nodesObj;
                    Console.WriteLine("✅ Available acquisition servers:");
                    foreach (string node in nodes)
                    {
                        Console.WriteLine("  - " + node);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠ Node discovery failed: " + ex.Message);
            }
            
            // Create SampleSetMethod object to get available sample set methods
            Console.WriteLine("\nDiscovering available sample set methods...");
            try 
            {
                var ssmType = Type.GetTypeFromProgID("MillenniumToolkit.SampleSetMethod");
                sampleSetMethod = Activator.CreateInstance(ssmType);
                
                var methodsObj = sampleSetMethod.GetType().InvokeMember(
                    "SampleSetMethodNames",
                    BindingFlags.GetProperty,
                    null,
                    sampleSetMethod,
                    null
                );
                
                if (methodsObj is System.DBNull)
                {
                    Console.WriteLine("⚠ No sample set methods available");
                }
                else
                {
                    string[] methods = (string[])methodsObj;
                    Console.WriteLine("✅ Available sample set methods:");
                    foreach (string method in methods)
                    {
                        Console.WriteLine("  - " + method);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠ Sample set method discovery failed: " + ex.Message);
            }
            
            // Try connecting using official pattern: Connect(nodeName, systemName) - NODE FIRST!
            Console.WriteLine("\nTesting connection with official pattern (node first, then system)...");
            string testNode = "Waters-h4q6k34";
            string testSystem = "Arc HPLC";
            
            try 
            {
                Console.WriteLine("Attempting: Connect(\"" + testNode + "\", \"" + testSystem + "\")");
                instrument.GetType().InvokeMember(
                    "Connect",
                    BindingFlags.InvokeMethod,
                    null,
                    instrument,
                    new object[] { testNode, testSystem }  // NODE FIRST, SYSTEM SECOND
                );
                
                Console.WriteLine("✅ Connection initiated, checking status...");
                
                // Check connection status using official pattern
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
                            Console.WriteLine("Connection in progress, waiting...");
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
                            if (text.Equals("Successfully connected to instrument server") || text.Length == 0)
                            {
                                Console.WriteLine("✅ Connection successful!");
                                
                                // Test getting instrument status
                                try
                                {
                                    var status = instrument.GetType().InvokeMember(
                                        "Status",
                                        BindingFlags.GetProperty,
                                        null,
                                        instrument,
                                        null
                                    );
                                    Console.WriteLine("✅ Instrument status accessible");
                                }
                                catch (Exception statusEx)
                                {
                                    Console.WriteLine("⚠ Status not accessible: " + statusEx.Message);
                                }
                            }
                            else
                            {
                                Console.WriteLine("❌ Connection failed: " + text);
                            }
                        }
                    }
                    catch (Exception statusEx)
                    {
                        Console.WriteLine("⚠ Status check failed: " + statusEx.Message);
                        break;
                    }
                }
            }
            catch (COMException comEx)
            {
                Console.WriteLine("❌ Connection failed with COM error: 0x" + comEx.ErrorCode.ToString("X"));
                
                // Use official error description method
                try
                {
                    var errorDesc = projectObj.GetType().InvokeMember(
                        "TkErrorDescription",
                        BindingFlags.InvokeMethod,
                        null,
                        projectObj,
                        new object[] { comEx.ErrorCode }
                    );
                    Console.WriteLine("Error description: " + errorDesc.ToString());
                }
                catch (Exception)
                {
                    Console.WriteLine("Could not get error description");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Connection failed: " + ex.Message);
            }
            
        }
        catch (Exception ex) 
        {
            Console.WriteLine("❌ Fatal Error: " + ex.Message);
            if (ex.InnerException != null)
            {
                Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
            }
        }
        finally 
        {
            // Cleanup using official pattern
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
            if (projectObj != null) Marshal.ReleaseComObject(projectObj);
            Console.WriteLine("✅ COM cleanup completed");
        }
        
        // Only wait for input in interactive mode
        if (!nonInteractive)
        {
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        else
        {
            Console.WriteLine("\nAutomated execution completed.");
        }
    }
}
