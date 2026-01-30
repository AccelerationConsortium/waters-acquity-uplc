// ************************************************************************
// (c) Copyright 2022 by Wyatt Technology Corporation. All rights reserved.
// ************************************************************************
using AstraLib;
using SDKCommon;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SdkCommandLineApp
{
    /// <summary>
    /// Example program of the ASTRA Automation API, showing how to collect data, get and set peaks and baselines and get results and data sets.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Unique identifier of client to the ASTRA Automation API.
        /// </summary>
        public static Guid ClientId = Guid.NewGuid ();

        /// <summary>
        /// Main entry point of application program.
        /// </summary>
        private static void Main ()
        {
            // Initialize the Astra instance by providing the name of the ASTRA
            // client, its version, its process ID and a unique GUID identifying the
            // client.
            var process = Process.GetCurrentProcess ();
            AstraAdmin.Get.SetAutomationIdentity ("SDK Command Line App", "1.0.0.0", process.Id, $"{ClientId}", 1);
            Console.WriteLine ("Waiting for instruments...");
            AstraAdmin.Get.WaitForInstruments ();
            Console.WriteLine ("Instruments detected.");

            if (!SecurityPackLogon ())
            {
                Console.WriteLine ("Could not logon to ASTRA. Incorrect username/password.");
                return;
            }

            // Uncomment one of those function calls to exercise a specific aspect of the ASTRA Automation API:
            //
            // RunSequence ();
            // StartCollectionAndProvideInfoAtTheEnd ();
            // ProcessExperiment ();
        }

        /// <summary>
        /// Open an experiment, set baselines and peaks, run the experiment, save results and data sets to a file.
        /// </summary>
        public static void ProcessExperiment ()
        {
            // Open an experiment given the path to the experiment file
            Console.WriteLine ("Opening experiment, enter path to file:");
            var path = Console.ReadLine ();
            var experimentId = AstraAdmin.Get.OpenExperiment (path);

            // Retrieve baselines from experiment then modify the array
            var baselines = AstraAdmin.Get.GetBaselines (experimentId);
            Console.WriteLine ($"Found {baselines.Length} baseline(s).");
            foreach (var baseline in baselines)
            {
                Console.WriteLine (
                    $"{baseline.seriesName}: ({baseline.start.x},{baseline.start.y}) to ({baseline.end.x}.{baseline.end.y}).");
            }

            // Here, we made sure there is at least one baseline in the array.
            // If so, we updated the first baseline.
            if (baselines.Length != 0)
            {
                baselines[0].start.x = 4.128;
                baselines[0].start.y = 0.049;
                baselines[0].start.x = 37.360;
                baselines[0].start.x = 0.049;
                baselines[0].seriesName = "detector1";
                baselines[0].type = BaselineType.eBT_SnapY;

                // Update the experiment baselines with the baselines from here
                AstraAdmin.Get.UpdateBaselines (experimentId, baselines);
            }

            // Get peaks from the experiment
            var peaks = AstraAdmin.Get.GetPeakRanges (experimentId);

            // Remove a peak range from the experiment
            AstraAdmin.Get.RemovePeakRange (experimentId, peaks[0].number);
            peaks = AstraAdmin.Get.GetPeakRanges (experimentId);
            Console.WriteLine ($"Found {peaks.Length} peak(s).");
            foreach (var peakRange in peaks)
            {
                Console.WriteLine ($"{peakRange.number} from {peakRange.start} to {peakRange.end}.");
            }

            // Add a peak range 
            AstraAdmin.Get.AddPeakRange (experimentId, 2.0, 3.0);
            peaks = AstraAdmin.Get.GetPeakRanges (experimentId);

            // Update a peak range given a PeakRange object with the same number.
            var peak = new PeakRange {number = peaks[0].number, start = 3.0, end = 4.0};
            AstraAdmin.Get.UpdatePeakRange (experimentId, peak);

            peaks = AstraAdmin.Get.GetPeakRanges (experimentId);
            Console.WriteLine ($"Found {peaks.Length} peak(s).");
            foreach (var peakRange in peaks)
            {
                Console.WriteLine ($"{peakRange.number} from {peakRange.start} to {peakRange.end}.");
            }

            // Because we made changes to the experiment, we need to run the experiment to get the updated results.
            AstraAdmin.Get.RunExperiment (experimentId);

            // Extract results to a file
            Console.WriteLine ("Saving results, enter path to file:");
            path = Console.ReadLine ();
            AstraAdmin.Get.SaveResults (experimentId, path);

            // Extract data set to a file given the definition name
            const string definitionName = "mean square radius vs volume";
            Console.WriteLine ("Saving data set, enter path to file:");
            path = Console.ReadLine ();
            AstraAdmin.Get.SaveDataSet (experimentId, definitionName, path);
        }

        /// <summary>
        /// Run a sequence from configuration given in a CSV file, then save to experiment files.
        /// </summary>
        public static void RunSequence ()
        {
            // Import CSV file.
            Console.WriteLine ("Enter CSV file path:");
            var path = Console.ReadLine ();

            while (string.IsNullOrEmpty (path) || !File.Exists (path))
            {
                Console.WriteLine ("File does not exist.\nEnter CSV file path:");
                path = Console.ReadLine ();
            }
            Console.WriteLine (@"Enter path to save experiment files (example: C:\Users\username\Documents\):");
            var exportPath = Console.ReadLine ();

            while (!Directory.Exists (exportPath))
            {
                Console.WriteLine ("Directory does not exist.\nEnter path:");
                exportPath = Console.ReadLine ();
            }

            using (var reader = new StreamReader (path))
            {
                // Skip first line as it contains the header.
                reader.ReadLine ();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine ();
                    if (string.IsNullOrWhiteSpace (line))
                    {
                        continue;
                    }

                    // values store data from a row in the csv file, where
                    // values[0]: Enable
                    // values[1]: Name
                    // values[2]: Description
                    // values[3]: Injection
                    // values[4]: Method
                    // values[5]: Duration (minutes)
                    // values[6]: Injection Volume (microL)
                    // values[7]: dn/dc (mL/g)
                    // values[8]: A2 (mol mL/g^2)
                    // values[9]: UV Ext (mL/(mg cm)))
                    // values[10]: Concentration (mg/mL)
                    // values [11]: Flow Rate (mL/min)
                    var values = line.Split (',');

                    if (values.Length != 12 || values[0] == "FALSE")
                    {
                    }

                    var injection = Convert.ToInt32 (values[3]);

                    for (var i = 1; i <= injection; i++)
                    {
                        // Run sequence row data collection, which creates experiment from template and then runs the experiment
                        var template = values[4];
                        var duration = Convert.ToDouble (values[5]);
                        var injectedVolume = Convert.ToDouble (values[6]);
                        var flowRate = Convert.ToDouble (values[11]);

                        var sampleInfo = new SampleInfo
                        {
                            name = values[1],
                            description = values[2],
                            dndc = Convert.ToDouble (values[7]),
                            a2 = Convert.ToDouble (values[8]),
                            uvExtinction = Convert.ToDouble (values[9]),
                            concentration = Convert.ToDouble (values[10])
                        };

                        var expFileName = "";
                        expFileName += sampleInfo.name.Length > 0 ? sampleInfo.name : "untitled";
                        if (injection > 1)
                        {
                            expFileName += $" ({i} of {injection})";
                        }

                        AstraAdmin.Get.CollectData (template, Path.Combine (exportPath, expFileName), sampleInfo, duration, injectedVolume, flowRate, Console.WriteLine);
                    }
                }
            }
        }

        /// <summary>
        /// Run a sequence from configuration given in a CSV file, then save to experiment files.
        /// </summary>
        public static void StartCollectionAndProvideInfoAtTheEnd ()
        {
            // Create an experiment given the path to a template.
            Console.WriteLine ("Enter method path:");
            var method = Console.ReadLine ();
            var experimentId = AstraAdmin.Get.NewExperimentFromTemplate (method);

            Console.WriteLine (@"Enter path to save experiment files (example: C:\Users\username\Documents\):");
            var exportPath = Console.ReadLine ();

            // Let's start a thread that will automatically stop the collection after 70s.
            // This assumes a method with a duration of at least 70s.
            var thread = new Thread (() =>
            {
                Thread.Sleep (70000);
                AstraAdmin.Get.StopCollection (experimentId);
            });
            thread.Start();

            AstraAdmin.Get.CollectDataWithMethodInfoCallback (experimentId, Console.WriteLine, () => new AstraMethodInfo ()
            {
                ExperimentPath = exportPath,
                FlowRate = 1.1,
                InjectedVolume = 5.2,
                Sample = new SampleInfo
                {
                    name = "BSA",
                    description = "BSA Description",
                    dndc = 0.195,
                    a2 = 0.1,
                    uvExtinction = 1,
                    concentration = 1.5
                }
            });
        }

        /// <summary>
        /// Logon to ASTRA. If in security pack mode a username/password/domain is requested.
        /// </summary>
        /// <returns>True if not in security pack mode or properly logged on, false otherwise.</returns>
        private static bool SecurityPackLogon ()
        {
            // Logon to security pack if needed
            var securityPackActive = AstraAdmin.Get.IsSecurityPackActive ();
            var isLoggedIn = AstraAdmin.Get.IsLoggedIn ();

            // If in security pack and not logged on already, logon with security pack credential.
            if (securityPackActive && isLoggedIn)
            {
                Console.WriteLine ("You need to login to security pack.");
                Console.WriteLine ("username:");
                var username = Console.ReadLine ();
                Console.WriteLine ("password:");
                var password = Console.ReadLine ();
                Console.WriteLine ("domain:");
                var domain = Console.ReadLine ();
                var result = AstraAdmin.Get.ValidateLogon (username, password, domain);

                // result.isValid return 0 if logon failed, return 1 if succeeded.
                if (result.isValid == 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
