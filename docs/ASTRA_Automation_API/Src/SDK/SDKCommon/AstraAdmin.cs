/****************************************************************************
 (c) Copyright 2024 Wyatt Technology, LLC. All rights reserved.
 ****************************************************************************/
using AstraLib;
using AstraSecurityPackSdkLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using LogonResult = AstraLib.LogonResult;

namespace SDKCommon
{
    /// <summary>
    /// Thread safe access to the ASTRA Automation API.
    /// </summary>
    public class AstraAdmin : IDisposable
    {
        /// <summary>
        /// Minimum supported version of ASTRA.
        /// </summary>
        public Version MinAstraVersion = new Version (8, 1, 0, 0);

        /// <summary>
        /// ASTRA version 8.1.1.0 filter. It is used to figure out if
        /// the new events Read/Write/Run are present.
        /// </summary>
        public Version Version_8_1_1_0 = new Version (8, 1, 1, 0);

        /// <summary>
        /// ASTRA version 8.2.0.105 filter. It is used to figure out if we can change the collection duration while running a collection.
        /// </summary>
        public Version Version_8_2_0_105 = new Version (8, 2, 0, 105);

        /// <summary>
        /// ASTRA version 8.2.0.117 filter. It is used to figure out if we ASTRA has the fix for setting autofind properly on baselines and peaks.
        /// </summary>
        public Version Version_8_2_0_117 = new Version (8, 2, 0, 117);


        public delegate void ExperimentEventHandler (object source, Experiment experiment);
        public delegate void InstrumentsDetectedEventHandler ();

        /// <summary>
        /// Singleton storage for current instance.
        /// </summary>
        private static volatile AstraAdmin Instance;

        /// <summary>
        /// Has <see cref="Instance"/> been already initialized?
        /// </summary>
        private static volatile bool IsInstanceAlreadyInitialized;

        /// <summary>
        /// Flag to specify whether to show a messageBox when an error occurs.
        /// </summary>
        public bool ShouldShowErrorMessageBox = true;

        /// <summary>
        /// Lock object to ensure thread safety.
        /// </summary>
        private static readonly object SyncRoot = new object ();

        /// <summary>
        /// Dictionary of all opened experiments.
        /// </summary>
        private readonly Dictionary<int, Experiment> Experiments = new Dictionary<int, Experiment> ();

        /// <summary>
        /// Dictionary of all closing experiments. Once a call to <see cref="CloseExperiment"/> is made,
        /// the experiment is not available anymore via <see cref="GetInternalExperiment"/> or <see cref="GetExperiment"/>
        /// </summary>
        private readonly Dictionary<int, Experiment> ClosingExperiments = new Dictionary<int, Experiment> ();

        /// <summary>
        /// Event used to signal readiness of an experiment.
        /// Obsolete, only use ReadEvent, WriteEvent and RunEvent instead when using ASTRA 8.1.1.x or greater
        /// </summary>
        private readonly ManualResetEvent ReadyEvent = new ManualResetEvent (false);

        /// <summary>
        /// Event used to signal that an experiment has been fully loaded/read.
        /// </summary>
        private readonly ManualResetEvent ReadEvent = new ManualResetEvent (false);

        /// <summary>
        /// Event used to signal that an experiment has been saved.
        /// </summary>
        private readonly ManualResetEvent WriteEvent = new ManualResetEvent (false);

        /// <summary>
        /// Event used to signal that an experiment has run.
        /// </summary>
        private readonly ManualResetEvent RunEvent = new ManualResetEvent (false);

        /// <summary>
        /// Event used to signal completion of experiment closing.
        /// </summary>
        private readonly ManualResetEvent ClosedEvent = new ManualResetEvent (false);

        /// <summary>
        /// Event used to signal that ASTRA has fully loaded all instruments. Needed before starting a collection.
        /// </summary>
        public readonly AutoResetEvent InstrumentDetectedSignal = new AutoResetEvent (false);

        /// <summary>
        /// Event used to signal that an experiment is preparing for a collection.
        /// </summary>
        private readonly ManualResetEvent PreparingForCollectionEvent = new ManualResetEvent (false);

        /// <summary>
        /// Event used to signal that an ASTRA experiment is waiting for the auto-inject signal.
        /// </summary>
        private readonly ManualResetEvent WaitingForAutoInjectEvent = new ManualResetEvent (false);

        /// <summary>
        /// Event used to signal start of a collection.
        /// </summary>
        private readonly ManualResetEvent CollectionStartedEvent = new ManualResetEvent (false);

        /// <summary>
        /// Event used to signal finish of a collection (successful or not).
        /// </summary>
        private readonly ManualResetEvent CollectionFinishedEvent = new ManualResetEvent (false);

        // Runtime Callable Wrapper (RCW) around COM object
        private Astra AstraCOM;
        private AstraSp AstraSpCOM;
        public Guid EntityId = Guid.NewGuid ();

        /// <summary>
        /// Event triggered when an experiment is closed.
        /// </summary>
        public event ExperimentEventHandler ExperimentClosed;

        /// <summary>
        /// Event triggered when the status of an experiment changed.
        /// </summary>
        public event ExperimentEventHandler ExperimentStatusChanged;

        /// <summary>
        /// Event triggered when instruments have been fully detected.
        /// </summary>
        public event InstrumentsDetectedEventHandler InstrumentsDetected;

        /// <summary>
        /// Description message about how to disable errors in the client app.
        /// </summary>
        private readonly string SuppressErrorMessage = "\n\nError messages can be disabled setting 'AstraAdmin.ShouldShowErrorMessageBox' to false";

        /// <summary>
        /// Private constructor. To be called only from <see cref="Get"/>.
        /// </summary>
        protected AstraAdmin ()
        {
            try
            {
                AstraSpCOM = new AstraSp ();
                AstraCOM = new Astra ();
                var astraVersion = AstraVersion;
                Debug.Assert (astraVersion >= MinAstraVersion, "Only ASTRA 8.1 or later is supported.");

                lock (SyncRoot)
                {
                    AstraCOM.ExperimentClosed += OnExperimentClosed;

                    // ASTRA 8.1.0.x and earlier only handle `ExperimentReady`.
                    // ASTRA 8.1.1.x and newer handles all experiment events but also the newly added Read/Write/Run events.
                    AstraCOM.ExperimentReady += OnExperimentReady;

                    if (astraVersion >= Version_8_1_1_0)
                    {
                        AstraCOM.ExperimentRead += OnExperimentRead;
                        AstraCOM.ExperimentWrite += OnExperimentWrite;
                        AstraCOM.ExperimentRun += OnExperimentRun;
                    }

                    AstraCOM.PreparingForCollection += OnPreparingForCollection;
                    AstraCOM.WaitingForAutoInject += OnWaitingForAutoInject;
                    AstraCOM.CollectionStarted += OnCollectionStarted;
                    AstraCOM.CollectionFinished += OnCollectionFinished;
                    AstraCOM.CollectionAborted += OnCollectionFinished;
                    AstraCOM.InstrumentDetectionCompleted += OnInstrumentDetectionCompleted;
                }
            }
            catch (Exception ex)
            {
                throw new Exception ("Unexpected COM exception: " + ex.Message);
            }
        }

        /// <summary>
        /// Finalizer of <see cref="AstraAdmin"/>.
        /// </summary>
        ~AstraAdmin ()
        {
            Dispose (false);
        }

        /// <summary>
        /// Thread save access to the ASTRA Automation API component. On the first call, make sure to call <see cref="SetAutomationIdentity"/>.
        /// </summary>
        /// <returns>New unique instance of <see cref="AstraAdmin"/>.</returns>
        public static AstraAdmin Get
        {
            get
            {
                if (Instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (Instance == null && !IsInstanceAlreadyInitialized)
                        {
                            IsInstanceAlreadyInitialized = true;
                            Instance = new AstraAdmin ();
                        }
                    }
                }

                return Instance;
            }
        }

        /// <summary>
        /// Version of ASTRA
        /// </summary>
        public Version AstraVersion
        {
            get
            {
                try
                {
                    var versionStr = AstraCOM.GetVersion ();
                    Version version;
                    return Version.TryParse (versionStr, out version) ? version : new Version (0, 0);
                }
                catch (Exception)
                {
                    return new Version (0, 0);
                }
            }
        }

        /// <summary>
        /// Reset AstraAdmin to allow a new instance to be created if previous one was gracefully shutdown.
        /// Useful for testing framework that do not create a new process or AppDomain when running each tests.
        /// </summary>
        public static void ResetAstra ()
        {
            lock (SyncRoot)
            {
                IsInstanceAlreadyInitialized = false;
            }
        }

        /// <summary>
        /// Set identify of client. This is the first call a client should make before making any other API calls.
        /// </summary>
        /// <param name="entityName">Name of the client.</param>
        /// <param name="entityVersion">Version of the client.</param>
        /// <param name="pid">Process ID of the client.</param>
        /// <param name="entityGuid">Unique identifier of the client.</param>
        /// <param name="enabled">Unused, should always be set to 1.</param>
        /// <returns>True upon successful close, false otherwise.</returns>
        public bool SetAutomationIdentity (string entityName, string entityVersion, int pid, string entityGuid, int enabled)
        {
            return TryExecute (() =>
            {
                Guid id;
                EntityId = Guid.TryParse (entityGuid, out id) ? id : Guid.Empty;
                AstraCOM.SetAutomationIdentity (entityName, entityVersion, pid, entityGuid, enabled, null);
                AstraSpCOM?.SetAutomationIdentity (entityName, entityVersion, pid, entityGuid, enabled);
            });
        }

        /// <summary>
        /// Have instruments been fully detected yet?
        /// </summary>
        /// <returns>True when all instruments have been detected, false otherwise.</returns>
        public bool HasInstrumentDetectionCompleted => TryGet (() => AstraCOM.InstrumentsDetected == 1);

        /// <inheritdoc />
        public void Dispose ()
        {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        /// <summary>
        /// Helper function for <see cref="Dispose"/>.
        /// </summary>
        /// <param name="disposing">If set to true, we are disposing from Dispose, otherwise from finalizer.</param>
        private void Dispose (bool disposing)
        {
            lock (SyncRoot)
            {
                try
                {
                    if (AstraSpCOM != null)
                    {
                        Marshal.ReleaseComObject (AstraSpCOM);
                        AstraSpCOM = null;
                    }

                    if (AstraCOM != null)
                    {
                        try
                        {
                            AstraCOM.RequestQuit ();
                        }
                        catch (Exception)
                        {
                            // Ignore. It might fail due to lack of licensing when using ASTRA 8.0.x or older.
                        }
                        Marshal.ReleaseComObject (AstraCOM);
                        AstraCOM = null;
                    }
                }
                catch (Exception)
                {
                    // Ignore exceptions
                }
                Instance = null;
            }
            if (disposing)
            {
                ReadyEvent.Dispose ();
                ReadEvent.Dispose ();
                WriteEvent.Dispose ();
                RunEvent.Dispose ();
                PreparingForCollectionEvent.Dispose ();
                CollectionFinishedEvent.Dispose ();
                CollectionStartedEvent.Dispose ();
                ClosedEvent.Dispose ();
                WaitingForAutoInjectEvent.Dispose ();
                InstrumentDetectedSignal.Dispose ();
            }
        }

        /// <summary>
        /// Wait until experiment is fully read. To be called after loading an experiment.
        /// </summary>
        public void WaitExperimentRead ()
        {
            if (AstraVersion < Version_8_1_1_0)
            {
                // This version of ASTRA does not report read events, we have to trust that client code is still calling WaitExperimentRun.
                return;
            }
            ReadEvent.WaitOne ();
            ReadEvent.Reset ();
        }

        /// <summary>
        /// Wait until experiment is fully written. To be called after saving an experiment.
        /// </summary>
        public void WaitExperimentWrite ()
        {
            if (AstraVersion < Version_8_1_1_0)
            {
                // This version of ASTRA does not report write events, we have to trust that client code is calling SaveExperiment that handles saving properly.
                return;
            }
            WriteEvent.WaitOne ();
            WriteEvent.Reset ();
        }

        /// <summary>
        /// Wait until experiment is fully written. To be called after saving an experiment.
        /// </summary>
        private void WaitExperimentWriteAstra81Fix (string path, DateTime date)
        {
            Debug.Assert (AstraVersion < Version_8_1_1_0);
            // Special implementation of Run until ASTRA 8.1.1 is released.
            // Replace code with just `WriteEvent.WaitOne (); WriteEvent.Reset ();`.


            // Wait until the file exist or modified after a certain time.
            var file = new FileInfo (path);
            if (file.Extension != ".afe8")
            {
                file = new FileInfo (path + ".afe8");
            }
            while (!file.Exists || file.LastWriteTime <= date)
            {
                Thread.Sleep (100);
                file.Refresh ();
            }
            ReadyEvent.WaitOne ();
            ReadyEvent.Reset ();
        }

        /// <summary>
        /// Wait until experiment is fully run. To be called after an operation that will render experiment not ready (e.g. a collection or a run of the experiment).
        /// </summary>
        public void WaitExperimentRun ()
        {
            if (AstraVersion < Version_8_1_1_0)
            {
                ReadyEvent.WaitOne (30000);
                ReadyEvent.Reset ();
            }
            else
            {
                RunEvent.WaitOne ();
                RunEvent.Reset ();
            }
        }

        /// <summary>
        /// Wait for ASTRA to load all instruments.
        /// </summary>
        public void WaitForInstruments ()
        {
            // If instruments have already been detected, we should not wait again since it will
            // wait indefinitely.
            if (!HasInstrumentDetectionCompleted)
            {
                InstrumentDetectedSignal.WaitOne ();
            }
        }

        /// <summary>
        /// Wait until experiment is not running.
        /// </summary>
        public void WaitExperimentNotRunning (int experimentId)
        {
            while (IsRunning (experimentId))
            {
                Thread.Sleep (100);
            }
        }

        /// <summary>
        /// Wait until experiment is fully closed.
        /// </summary>
        public void WaitExperimentClosed ()
        {
            ClosedEvent.WaitOne ();
            ClosedEvent.Reset ();
        }

        /// <summary>
        /// Wait until collection is fully validated.
        /// </summary>
        public void WaitPreparingForCollection ()
        {
            PreparingForCollectionEvent.WaitOne ();
            PreparingForCollectionEvent.Reset ();
        }

        /// <summary>
        /// Wait until the waiting for auto-inject message is sent.
        /// </summary>
        public void WaitWaitingForAutoInject ()
        {
            WaitingForAutoInjectEvent.WaitOne ();
            WaitingForAutoInjectEvent.Reset ();
        }

        /// <summary>
        /// Wait until collection starts.
        /// </summary>
        public void WaitCollectionStarted ()
        {
            CollectionStartedEvent.WaitOne ();
            CollectionStartedEvent.Reset ();
        }

        /// <summary>
        /// Wait until collection is finished.
        /// </summary>
        public void WaitCollectionFinished ()
        {
            CollectionFinishedEvent.WaitOne ();
            CollectionFinishedEvent.Reset ();
        }

        /// <summary>
        /// Create new experiment and run collection, then save the experiment to a file or database.
        /// </summary>
        /// <param name="methodPath">Path to the method used to run a collection.</param>
        /// <param name="experimentSavePath">Path where ASTRA experiment file will be saved.</param>
        /// <param name="sample">Information about the sample to use for collecting data.</param>
        /// <param name="duration">Duration of collection expressed in minutes.</param>
        /// <param name="injectionVolume">Injection volume in microL.</param>
        /// <param name="flowRate">Desired/Set pump flow rate mL/min. If negative the one from the method.</param>
        /// <param name="progressUpdate">Action being executed for each major steps in the collection of data</param>
        /// <param name="findBaselinesAndPeaks">If enabled, will automaticall calculate baselines and peaks.</param>
        public void CollectData (string methodPath, string experimentSavePath, SampleInfo sample, double duration, double injectionVolume, double flowRate,
                                 Action<string> progressUpdate, bool findBaselinesAndPeaks = false)
        {
            var astraMethodInfo = new AstraMethodInfo ()
            {
                ExperimentPath = experimentSavePath,
                FlowRate       = flowRate,
                InjectedVolume = injectionVolume,
                Sample         = sample,
                Duration       = duration
            };

            // Create an experiment given the path to a template.
            progressUpdate?.Invoke ($"Starting collection using method \"{methodPath}\"...");
            var experimentId = NewExperimentFromTemplate (methodPath);

            if (findBaselinesAndPeaks)
            {
                SetAutoAutofindBaselines (experimentId, true);
                SetAutoAutofindPeaks (experimentId, true);
            }

            CollectDataWithMethodInfoCallback (experimentId, progressUpdate, () => astraMethodInfo, true);
        }

        /// <summary>
        /// Given a freshly created experiment with ID <paramref name="experimentId"/>, start a collection but only fill the details
        /// for sample, duration, flow rate, injected volume after completion of the collection.
        /// </summary>
        /// <param name="experimentId">ID of experiment used for collecting data</param>
        /// <param name="progressUpdate">Action being executed for each major steps in the collection of data</param>
        /// <param name="methodInfo">Function returning info about the run at the end of a collection</param>
        /// <param name="requestMethodAtEnd">Flag to populate method info after collection completes.</param>
        public void CollectDataWithMethodInfoCallback (int experimentId, Action<string> progressUpdate, Func<AstraMethodInfo> methodInfo, bool requestMethodAtEnd = true)
        {
            AstraMethodInfo info = null;
            if (!requestMethodAtEnd)
            {
                info = methodInfo ();
                SetSample (experimentId, info.Sample);
                SetCollectionDuration (experimentId, info.Duration);
                SetInjectedVolume (experimentId, info.InjectedVolume);
                if (info.FlowRate >= 0)
                {
                    SetPumpFlowRate (experimentId, info.FlowRate);
                }
            }
            // Run collection.
            progressUpdate?.Invoke ("Collection starting...");
            StartCollection (experimentId);
            WaitPreparingForCollection ();
            progressUpdate?.Invoke ("Preparing for collection...");

            WaitWaitingForAutoInject ();
            progressUpdate?.Invoke ("Waiting for auto-inject...");

            WaitCollectionStarted ();
            progressUpdate?.Invoke ("Starting collecting data...");

            // Get the current time to calculate the actual duration of the collection.
            var date = DateTime.Now;

            if (AstraVersion >= Version_8_2_0_105)
            {
                // This has to be done after the collection starts.
                // Due to an issue in earlier versions of ASTRA, this call can only be done for version >= 8.2.0.105.

                // If requestMethodAtEnd is true, we will not have the info.Duration value yet, so
                // the duration will be set to -1 (indefinite) even if we did specify a duration.
                if (!requestMethodAtEnd)
                    SetCollectionDuration (experimentId, info != null ? info.Duration : -1);
            }

            WaitCollectionFinished ();
            // Duration of the collection in minutes.
            var duration = (DateTime.Now - date).TotalMilliseconds / 60000;

            progressUpdate?.Invoke ("Collection finished.");

            progressUpdate?.Invoke ("Post-collection actions...");
            WaitExperimentRun ();

            if (requestMethodAtEnd)
            {
                // Note that setting the sample, injected volume, and flow rate each cause a rerun.
                // If we do not wait between calls, we will get an exception about modifying a
                // running experiment.
                //
                // SetSample () does not wait for the experiment to run so we must call WaitExperimentRun ().
                // The other calls already wait for the experiment to run.
                // SetCollectionDuration () does not cause a rerun.
                info = methodInfo ();
                SetSample (experimentId, info.Sample);
                WaitExperimentRun ();

                SetCollectionDuration (experimentId, duration);
                SetInjectedVolume (experimentId, info.InjectedVolume);

                if (info.FlowRate >= 0)
                {
                    SetPumpFlowRate (experimentId, info.FlowRate);
                }
            }

            // Save and close the experiment file.
            progressUpdate?.Invoke ($"Saving experiment \"{info.ExperimentPath}\"...");
            SaveExperiment (experimentId, info.ExperimentPath);

            progressUpdate?.Invoke ("Experiment saved.");
            CloseExperiment (experimentId);

            // Clear all events that were received to start fresh when a new collection is performed.
            ResetEvents ();
            progressUpdate?.Invoke ("Collection completed.");
        }

        /// <summary>
        /// Get active user in security pack mode. If no active user, a user with an empty userId.
        /// </summary>
        /// <returns>The active user logged in, or a default user.</returns>
        public ActiveUserInfo GetActiveUser ()
        {
            return TryGet (() =>
            {
                var astraSp = AstraSpCOM;
                var activeUser = TryGet (() => astraSp?.GetActiveUserInfo () ?? new ActiveUserInfo { userId = "", fullUserName = "", localDomain = "" });
                return activeUser.userId == null ? new ActiveUserInfo { userId = "", fullUserName = "", localDomain = "" } : activeUser;
            });
        }

        /// <summary>
        /// Shutdown the current instance. It will close all open experiments and then perform a gracious shutdown of ASTRA.
        /// </summary>
        public void Shutdown ()
        {
            foreach (var element in Experiments.ToList ())
            {
                CloseExperiment (element.Key);
            }

            lock (SyncRoot)
            {
                AstraCOM.ExperimentClosed -= OnExperimentClosed;
                AstraCOM.ExperimentReady -= OnExperimentReady;
                AstraCOM.ExperimentRead -= OnExperimentRead;
                AstraCOM.ExperimentWrite -= OnExperimentWrite;
                AstraCOM.ExperimentRun -= OnExperimentRun;
                AstraCOM.PreparingForCollection -= OnPreparingForCollection;
                AstraCOM.WaitingForAutoInject -= OnWaitingForAutoInject;
                AstraCOM.CollectionStarted -= OnCollectionStarted;
                AstraCOM.CollectionFinished -= OnCollectionFinished;
                AstraCOM.CollectionAborted -= OnCollectionFinished;
                AstraCOM.InstrumentDetectionCompleted -= OnInstrumentDetectionCompleted;

                Dispose ();
            }
        }

        public bool SetSystemState (string state, string pswd)
        {
            return TryExecute (() => AstraCOM.SetSystemState (state, pswd));
        }

        /// <summary>
        /// Get experiment wrapper for an opened experiment.
        /// </summary>
        /// <param name="experimentID">ID of experiment to get.</param>
        /// <returns>If an experiment with ID <see cref="experimentID"/> exists, a copy of that experiment, null otherwise.</returns>
        public Experiment GetExperiment (int experimentID)
        {
            lock (SyncRoot)
            {
                return Experiments.ContainsKey (experimentID) ? Experiments [experimentID].Clone () : null;
            }
        }

        /// <summary>
        /// Get experiment wrapper for an opened experiment.
        /// </summary>
        /// <param name="experimentID">ID of experiment to get.</param>
        /// <returns>If an experiment with ID <see cref="experimentID"/> exists, that experiment, null otherwise.</returns>
        protected Experiment GetInternalExperiment (int experimentID)
        {
            lock (SyncRoot)
            {
                return Experiments.ContainsKey (experimentID) ? Experiments [experimentID] : null;
            }
        }

        /// <summary>
        /// Show/hide the ASTRA application window. To be used only for debugging purposes.
        /// </summary>
        /// <param name="show">True to show ASTRA, false otherwise.</param>
        /// <returns>True upon successful close, false otherwise.</returns>
        public bool ShowWindow (bool show)
        {
            return TryExecute (() => AstraCOM.Show (show ? 1 : 0));
        }

        /// <summary>
        /// Get the list of experiment templates.
        /// </summary>
        /// <returns>List of experiment templates.</returns>
        public Array GetExperimentTemplates ()
        {
            return TryGet (() => AstraCOM.GetExperimentTemplates ());
        }

        /// <summary>
        /// Get list of directories from the Data database under <see cref="rootPath"/>.
        /// </summary>
        /// <param name="rootPath">Path used to get all sub directories.</param>
        /// <returns>List of directories under <see cref="rootPath"/>.</returns>
        public Array GetDataDatabaseDirectory (string rootPath)
        {
            return TryGet (() => AstraCOM.GetDataDatabaseDirectory (rootPath));
        }

        /// <summary>
        /// Create new experiment from template.
        /// </summary>
        /// <param name="templatePath">Location of template in system database.</param>
        /// <returns>ID of a newly created experiment.</returns>
        public int NewExperimentFromTemplate (string templatePath)
        {
            int experimentID;
            lock (SyncRoot)
            {
                experimentID = TryGet (() => AstraCOM.NewExperimentFromTemplate (templatePath));

                if (experimentID <= 0)
                {
                    return -1;
                }
                // Add Experiment wrapper to map of open experiments
                var experiment = new Experiment (experimentID) { Status = ExperimentStatus.Busy };
                Experiments.Add (experimentID, experiment);

                ExperimentStatusChanged?.Invoke (this, experiment);
            }

            // Wait until experiment is fully loaded and ready before proceeding.
            // This needs to be done outside of the lock otherwise events cannot be processed.
            WaitExperimentRead ();
            WaitExperimentRun ();

            lock (SyncRoot)
            {
                var experiment = GetInternalExperiment (experimentID);
                experiment.Read ();
                ExperimentStatusChanged?.Invoke (this, experiment);
            }

            return experimentID;
        }

        /// <summary>
        /// Get the name of an opened experiment.
        /// </summary>
        /// <param name="experimentID">ID of experiment to open</param>
        /// <returns>Name of experiment.</returns>
        public string GetExperimentName (int experimentID)
        {
            return TryGet (() => AstraCOM.GetExperimentName (experimentID));
        }

        /// <summary>
        /// Open an experiment from location <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">Location of experiment to open.</param>
        /// <returns>ID of experiment if successfully opened, otherwise 0.</returns>
        // Open an experiment.
        public int OpenExperiment (string fileName)
        {
            int experimentID;
            lock (SyncRoot)
            {
                experimentID = TryGet (() => AstraCOM.OpenExperiment (fileName));

                if (experimentID <= 0)
                {
                    return -1;
                }
                // Add Experiment wrapper to map of open experiments
                var experiment = new Experiment (experimentID);
                Experiments.Add (experimentID, experiment);

                ExperimentStatusChanged?.Invoke (this, experiment);
            }

            // Wait until experiment is fully loaded and ready before proceeding.
            // This needs to be done outside of the lock otherwise events cannot be processed.
            WaitExperimentRead ();
            WaitExperimentRun ();

            lock (SyncRoot)
            {
                var experiment = GetInternalExperiment (experimentID);
                experiment.Read ();
                ExperimentStatusChanged?.Invoke (this, experiment);
            }

            return experimentID;
        }

        /// <summary>
        /// Save experiment with ID <paramref name="experimentID"/> to location <paramref name="fileName"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment to save.</param>
        /// <param name="fileName">File location where experiment should be saved.</param>
        /// <returns>True when file is successfully saved, false otherwise.</returns>
        public bool SaveExperiment (int experimentID, string fileName)
        {
            var dateTime = DateTime.Now;
            lock (SyncRoot)
            {
                var experiment = GetInternalExperiment (experimentID);

                if (experiment == null)
                {
                    return false;
                }

                experiment.Status = ExperimentStatus.Busy;

                ExperimentStatusChanged?.Invoke (this, experiment);

                if (!TryExecute (() => AstraCOM.SaveExperiment (experimentID, fileName)))
                {
                    // Reset status of experiment and notify clients.
                    experiment.Status = ExperimentStatus.Ready;
                    ExperimentStatusChanged?.Invoke (this, experiment);
                    return false;
                }
            }

            // Wait has to be done outside of the lock (SyncRoot) otherwise the ASTRA messages cannot be sent.
            if (AstraVersion < Version_8_1_1_0)
            {
                WaitExperimentWriteAstra81Fix (fileName, dateTime);
            }
            else
            {
                WaitExperimentWrite ();
            }
            return true;
        }

        /// <summary>
        /// Close experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment to close.</param>
        /// <returns>True upon successful close, false otherwise.</returns>
        public bool CloseExperiment (int experimentID)
        {
            lock (SyncRoot)
            {
                ClosingExperiments.Add (experimentID, GetInternalExperiment (experimentID));
                Experiments.Remove (experimentID);
                if (!TryExecute (() => AstraCOM.CloseExperiment (experimentID)))
                {
                    return false;
                }
            }
            WaitExperimentClosed ();
            return true;
        }

        /// <summary>
        /// Is experiment with ID <paramref name="experimentID"/> currently running?
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>True if running, false otherwise.</returns>
        public bool IsRunning (int experimentID)
        {
            return TryGet (() => AstraCOM.GetIsExperimentRunning (experimentID) != 0);
        }

        /// <summary>
        /// Get an array of InstrumentInfo for the instruments in the configuration of the experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>Array of InstrumentInfo for configuration instruments</returns>
        public InstrumentInfo[] GetInstrumentInfo (int experimentID)
        {
            return TryGet (() => AstraCOM.GetExperimentInstrumentInfo (experimentID));
        }

        /// <summary>
        /// Get duration of collection for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>Duration in minutes.</returns>
        public double GetCollectionDuration (int experimentID)
        {
            return TryGet (() => AstraCOM.GetCollectionDuration (experimentID));
        }

        /// <summary>
        /// Set duration of collection for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="duration">Duration in minutes of the collection.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool SetCollectionDuration (int experimentID, double duration)
        {
            return TryExecute (() => AstraCOM.SetCollectionDuration (experimentID, duration));
        }

        /// <summary>
        /// Validate experiment with ID <paramref name="experimentID"/>. Useful before starting a collection.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="details">Warnings and errors reported during the validation.</param>
        /// <returns>True when experiment is valid, false otherwise.</returns>
        public bool ValidateExperiment (int experimentID, out string details)
        {
            var s = "";
            var result = TryGet (() => AstraCOM.ValidateExperiment (experimentID, out s) == 1);
            details = s;

            return result;
        }

        /// <summary>
        /// For experiment to use either the Instrument's calibration constant or the method's calibration constant.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="state">True to use the instrument's calibration constant, false to use the method's calibration constant.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool UseInstrumentCalibrationConstant (int experimentID, bool state)
        {
            return TryExecute (() => AstraCOM.UseInstrumentCalibrationConstant (experimentID, state ? 1 : 0));
        }

        /// <summary>
        /// Start the collection of experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool StartCollection (int experimentID)
        {
            return TryExecute (() =>
            {
                var experiment = GetInternalExperiment (experimentID);
                AstraCOM.StartCollection (experimentID);
                experiment.Status = ExperimentStatus.Busy;
                ExperimentStatusChanged?.Invoke (this, experiment);
            });
        }

        /// <summary>
        /// Stop collection of experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool StopCollection (int experimentID)
        {
            return TryExecute (() => AstraCOM.StopCollection (experimentID));
        }


        /// <summary>
        /// Get the description of experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>Description if call is successful, null otherwise.</returns>
        public string GetExperimentDescription (int experimentID)
        {
            return TryGet (() => AstraCOM.GetExperimentDescription (experimentID));
        }

        /// <summary>
        /// Set the description of experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="description">Description to use for experiment.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool SetExperimentDescription (int experimentID, string description)
        {
            return TryExecute (() => AstraCOM.SetExperimentDescription (experimentID, description));
        }

        /// <summary>
        /// Get flow rate on pump for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>Flow rate of pump in mL/min if successful, 0 otherwise.</returns>
        public double GetPumpFlowRate (int experimentID)
        {
            return TryGet (() => AstraCOM.GetPumpFlowRate (experimentID));
        }

        /// <summary>
        /// Set flow rate on pump for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="flowRate">Flow rate to set in mL/min.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool SetPumpFlowRate (int experimentID, double flowRate)
        {
            return TryExecuteAndWaitExperimentRun (() => AstraCOM.SetPumpFlowRate (experimentID, flowRate));
        }

        /// <summary>
        /// Get injected volume of the injector for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>Injected volume in mL if successful, 0 otherwise.</returns>
        public double GetInjectedVolume (int experimentID)
        {
            return TryGet (() => AstraCOM.GetInjectedVolume (experimentID));
        }

        /// <summary>
        /// Set injected volume of the injector for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="injectedVolume">Injected volume in mL.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool SetInjectedVolume (int experimentID, double injectedVolume)
        {
            return TryExecuteAndWaitExperimentRun (() => AstraCOM.SetInjectedVolume (experimentID, injectedVolume));
        }

        /// <summary>
        /// Get sample for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>Sample details of experiment.</returns>
        public SampleInfo GetSample (int experimentID)
        {
            return TryGet (() => AstraCOM.GetSample (experimentID));
        }

        /// <summary>
        /// Set sample for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="sample">Sample to use.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool SetSample (int experimentID, SampleInfo sample)
        {
            return TryExecute (() => AstraCOM.SetSample (experimentID, ref sample));
        }

        /// <summary>
        /// Get sample name for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>Sample name of experiment.</returns>
        public string GetSampleName (int experimentID)
        {
            return TryGet (() => AstraCOM.GetSampleName (experimentID));
        }

        /// <summary>
        /// Set sample name for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="sampleName">Sample name to use.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool SetSampleName (int experimentID, string sampleName)
        {
            return TryExecute (() => AstraCOM.SetSampleName (experimentID, sampleName));
        }

        /// <summary>
        /// Get sample description for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>Sample description of experiment.</returns>
        public string GetSampleDescription (int experimentID)
        {
            return TryGet (() => AstraCOM.GetSampleDescription (experimentID));
        }

        /// <summary>
        /// Set sample description for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="sampleDescription">Sample description to use.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool SetSampleDescription (int experimentID, string sampleDescription)
        {
            return TryExecute (() => AstraCOM.SetSampleDescription (experimentID, sampleDescription));
        }

        /// <summary>
        /// Get sample dndc for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>Sample dndc of experiment.</returns>
        public double GetSampleDndc (int experimentID)
        {
            return TryGet (() => AstraCOM.GetSampleDndc (experimentID));
        }

        /// <summary>
        /// Set sample dndc for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="sampleDndc">Sample dndc to use.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool SetSampleDndc (int experimentID, double sampleDndc)
        {
            return TryExecuteAndWaitExperimentRun (() => AstraCOM.SetSampleDndc (experimentID, sampleDndc));
        }

        /// <summary>
        /// Get sample a2 for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>Sample a2 of experiment.</returns>
        public double GetSampleA2 (int experimentID)
        {
            return TryGet (() => AstraCOM.GetSampleA2 (experimentID));
        }

        /// <summary>
        /// Set sample a2 for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="sampleA2">Sample a2 to use.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool SetSampleA2 (int experimentID, double sampleA2)
        {
            return TryExecuteAndWaitExperimentRun (() => AstraCOM.SetSampleA2 (experimentID, sampleA2));
        }

        /// <summary>
        /// Get sample uv extinction for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>Sample uv extinction of experiment.</returns>
        public double GetSampleUvExtinction (int experimentID)
        {
            return TryGet (() => AstraCOM.GetSampleUvExtinction (experimentID));
        }

        /// <summary>
        /// Set sample uv extinction for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="sampleUvExtinction">Sample uv extinction to use.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool SetSampleUvExtinction (int experimentID, double sampleUvExtinction)
        {
            return TryExecuteAndWaitExperimentRun (() => AstraCOM.SetSampleUvExtinction (experimentID, sampleUvExtinction));
        }

        /// <summary>
        /// Get sample concentration for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>Sample concentration of experiment.</returns>
        public double GetSampleConcentration (int experimentID)
        {
            return TryGet (() => AstraCOM.GetSampleConcentration (experimentID));
        }

        /// <summary>
        /// Set sample concentration for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="sampleConcentration">Sample concentration to use.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool SetSampleConcentration (int experimentID, double sampleConcentration)
        {
            return TryExecuteAndWaitExperimentRun (() => AstraCOM.SetSampleConcentration (experimentID, sampleConcentration));
        }

        /// <summary>
        /// Does experiment with ID <paramref name="experimentID"/> have a VISION UV profile?
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>True if experiment has a VISION UV profile, false otherwise.</returns>
        public bool HasVisionUv (int experimentID)
        {
            return TryGet (() => AstraCOM.HasVisionUv (experimentID) == 1);
        }

        /// <summary>
        /// Assuming experiment with ID <paramref name="experimentID"/> has a VISION UV profile, set the details of the UV detector(s).
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="deviceDetails">Details of the used UV detector(s).</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool SetupVisionUv (int experimentID, UvDeviceDetails deviceDetails)
        {
            return TryExecute (() => AstraCOM.SetupVisionUv (experimentID, deviceDetails));
        }

        /// <summary>
        /// Get baselines of experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>List of baselines if successful, null otherwise.</returns>
        public BaselineDetails [] GetBaselines (int experimentID)
        {
            return TryGet (() => AstraCOM.GetBaselines (experimentID));
        }

        /// <summary>
        /// String representation of a baseline's type.
        /// </summary>
        /// <param name="type">Type of baseline.</param>
        /// <returns>String representation of <paramref name="type"/>.</returns>
        public string GetBaselineTypeString (BaselineType type)
        {
            switch (type)
            {
                case BaselineType.eBT_Manual:
                    return "Manual";
                case BaselineType.eBT_SnapY:
                    return "SnapY";
                case BaselineType.eBT_Automatic:
                    return "Automatic";
                case BaselineType.eBT_None:
                    return "None";
                default:
                    return "None";
            }
        }

        /// <summary>
        /// Given a string representation of a baseline's type, return its corresponding <see cref="BaselineType" />.
        /// </summary>
        /// <param name="type">String representation of baseline.</param>
        /// <returns>Associated enumeration from <see cref="BaselineType"/>.</returns>
        public BaselineType GetBaselineTypeInt (string type)
        {
            switch (type.ToLower ())
            {
                case "manual":
                    return BaselineType.eBT_Manual;
                case "snapy":
                    return BaselineType.eBT_SnapY;
                case "automatic":
                    return BaselineType.eBT_Automatic;
                default:
                    return BaselineType.eBT_None;
            }
        }

        /// <summary>
        /// Update baselines of experiment with ID <paramref name="experimentID"/>. Length of <paramref name="baselines"/> should match the length returned by <see cref="GetBaselines"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="baselines">New baselines of experiment.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool UpdateBaselines (int experimentID, BaselineDetails [] baselines)
        {
            if (baselines.Length == 0)
            {
                return false;
            }

            return TryExecute (() => AstraCOM.UpdateBaselines (experimentID, baselines));
        }

        /// <summary>
        /// Get peaks of experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>List of peaks if successful, null otherwise.</returns>
        public PeakRange [] GetPeakRanges (int experimentID)
        {
            return TryGet (() => AstraCOM.GetPeakRanges (experimentID));
        }

        /// <summary>
        /// Add a peak to experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="start">Starting time of peaks in minutes.</param>
        /// <param name="end">Ending time of peaks in minutes.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool AddPeakRange (int experimentID, double start, double end)
        {
            return TryExecute (() => AstraCOM.AddPeakRange (experimentID, start, end));
        }

        /// <summary>
        /// Update existing peak <paramref name="peak"/> of experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="peak">Peak to update.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool UpdatePeakRange (int experimentID, PeakRange peak)
        {
            return TryExecute (() => AstraCOM.UpdatePeakRange (experimentID, peak));
        }

        /// <summary>
        /// Remove existing peak with number <paramref name="peakNumber"/> of experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="peakNumber">Peak to remove.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool RemovePeakRange (int experimentID, int peakNumber)
        {
            return TryExecute (() => AstraCOM.RemovePeakRange (experimentID, peakNumber));
        }

        /// <summary>
        /// Get results as XML for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>XML representation if successful of the results, null otherwise.</returns>
        public string GetResults (int experimentID)
        {
            return TryGet (() => AstraCOM.GetResults (experimentID));
        }

        /// <summary>
        /// Save results as XML for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="fileName">File location where to save the XML results.</param>
        /// <returns>True if results were successfully saved to <paramref name="fileName"/>, false otherwise.</returns>
        public bool SaveResults (int experimentID, string fileName)
        {
            return TryExecute (() => AstraCOM.SaveResults (experimentID, fileName));
        }

        /// <summary>
        /// Get data associated to a dataset name <paramref name="definitionName"/> for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="definitionName">Name of dataset to retrieve.</param>
        /// <returns>Dataset content as a formatted string, null otherwise.</returns>
        public string GetDataSet (int experimentID, string definitionName)
        {
            return TryGet (() => AstraCOM.GetDataSet (experimentID, definitionName));
        }

        /// <summary>
        /// Get a list of all dataset names for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>A list of dataset names.  They can be used with GetDataSet.</returns>
        public string[] GetDataSetNames (int experimentID)
        {
            return TryGet (() => AstraCOM.GetDataSetNames (experimentID));
        }

        /// <summary>
        /// Get data associated to all datasets for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>Dataset content as a formatted string, null otherwise.</returns>
        public string GetAllDataSets (int experimentID)
        {
            return TryGet (() => AstraCOM.GetAllDataSets (experimentID));
        }

        /// <summary>
        /// Save data associated to a dataset name <paramref name="definitionName"/> for experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="definitionName">Name of dataset to retrieve.</param>
        /// <param name="fileName">File location where to save the XML results.</param>
        /// <returns>True if results were successfully saved to <paramref name="fileName"/>, false otherwise.</returns>
        public bool SaveDataSet (int experimentID, string definitionName, string fileName)
        {
            return TryExecute (() => AstraCOM.SaveDataSet (experimentID, definitionName, fileName));
        }

        /// <summary>
        /// Automatically find baselines of experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="state">True to enable autofind, false to disable it.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool SetAutoAutofindBaselines (int experimentID, bool state)
        {
            return TryExecute (() => AstraCOM.SetAutoAutofindBaselines (experimentID, state ? 1 : 0));
        }

        /// <summary>
        /// Automatically find peaks of experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="state">True to enable autofind, false to disable it.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool SetAutoAutofindPeaks (int experimentID, bool state)
        {
            if (AstraVersion >= Version_8_1_1_0 && AstraVersion < Version_8_2_0_117)
            {
                // A defect was introduced in ASTRA 8.1.1.11 and fixed in ASTRA 8.2.0.117
                // where calling the autofind does not persist the calculated peaks, so
                // calling it has no effect.
                Debug.Assert (false, $"The installed version of ASTRA (ASTRA {AstraVersion}) does not perform AutoFind peaks. Update to {Version_8_2_0_117} or later.");
            }
            return TryExecute (() => AstraCOM.SetAutoAutofindPeaks (experimentID, state ? 1 : 0));
        }

        /// <summary>
        /// Add a single Fraction Result <paramref name="fractionResultJson"/> to
        /// the dataset of experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="index">Index of Fraction Result.</param>
        /// <param name="fractionResultJson">Fraction Result as json.</param>
        /// <returns>True for success, false otherwise.</returns>
        public bool AddFractionResult (int experimentID, double index, string fractionResultJson)
        {
            // Future implementation may iterator over a vector, or pass in a vector
            // of Fraction Results
            return TryExecute (() => AstraCOM.AddFractionResult (experimentID, index, fractionResultJson));
        }

        /// <summary>
        /// Get a single Fraction Result <paramref name="fractionResultJson"/> from
        /// the dataset of experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <param name="index">Index of Fraction Result.</param>
        /// <returns>String containing fraction result in JSON.</returns>
        public string GetFractionResult (int experimentID, int index)
        {
            return TryGet (() => AstraCOM.GetFractionResult (experimentID, index));
        }

        /// <summary>
        /// Is security pack active? If true, then <see cref="ValidateLogon"/> should be called to identify the user
        /// before any other operations can be performed.
        /// </summary>
        /// <returns>True if security pack is enabled, false otherwise.</returns>
        public bool IsSecurityPackActive ()
        {
            return TryGet (() => AstraSpCOM?.IsSecurityPackActive () != 0);
        }

        /// <summary>
        /// Is a user logged in?
        /// </summary>
        /// <returns>True if security pack is enabled and a user logged in, false otherwise.</returns>
        public bool IsLoggedIn ()
        {
            return TryGet (() => AstraSpCOM?.IsLoggedIn () != 0);
        }

        /// <summary>
        /// Validate logon of client with ASTRA.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="password">Password for <paramref name="userId"/>.</param>
        /// <param name="domain">Domain for <paramref name="userId"/>.</param>
        /// <returns>Logon information with details about the success of the logon.</returns>
        public LogonResult ValidateLogon (string userId, string password, string domain)
        {
            return TryGet (() =>
            {
                var result = AstraSpCOM?.ValidateLogon (userId, password, domain);
                return result.HasValue && result.Value.isValid == 1
                    ? AstraCOM.ValidateLogon (userId, password, domain)
                    : new LogonResult () { isValid = 0, errorDetails = result?.errorDetails, errorMessage = result?.errorMessage };
            });
        }

        /// <summary>
        /// Run experiment with ID <paramref name="experimentID"/>.
        /// </summary>
        /// <param name="experimentID">ID of experiment.</param>
        /// <returns>True if call was successful, false otherwise.</returns>
        public bool RunExperiment (int experimentID)
        {
            RunEvent.Reset ();
            var success = TryExecute (() => AstraCOM.RunExperiment (experimentID));
            WaitExperimentRun ();
            return success;
        }


        #region Event Handlers
        /// <summary>
        /// Reset all events. Recommended between 2 collections to ensure all events can be awaited for the next collection.
        /// </summary>
        public void ResetEvents ()
        {
            ReadyEvent.Reset ();
            ReadEvent.Reset ();
            WriteEvent.Reset ();
            RunEvent.Reset ();
            PreparingForCollectionEvent.Reset ();
            ClosedEvent.Reset ();
            CollectionStartedEvent.Reset ();
            CollectionFinishedEvent.Reset ();
            WaitingForAutoInjectEvent.Reset ();
        }

        private void OnExperimentClosed (int experimentID)
        {
            lock (SyncRoot)
            {
                if (AstraCOM == null)
                {
                    // COM object has been disposed but callback already started. Can't proceed anymore.
                    return;
                }
                if (!ClosingExperiments.ContainsKey (experimentID))
                {
                    Debug.Assert (false, "Experiment should exist in a callback.");
                    return;
                }
                ClosedEvent.Set ();
                ExperimentClosed?.Invoke (this, ClosingExperiments[experimentID]);
                ClosingExperiments.Remove (experimentID);
            }
        }

        private void OnExperimentReady (int experimentID)
        {
            lock (SyncRoot)
            {
                if (AstraCOM == null)
                {
                    // COM object has been disposed but callback already started. Can't proceed anymore.
                    return;
                }
                var experiment = GetInternalExperiment (experimentID);
                if (experiment == null)
                {
                    Debug.Assert (ClosingExperiments.ContainsKey (experimentID), "Experiment should exist in a callback.");
                    return;
                }

                experiment.Status = ExperimentStatus.Ready;

                ReadyEvent.Set ();

                ExperimentStatusChanged?.Invoke (this, experiment);
            }
        }

        private void OnExperimentRead (int experimentID)
        {
            lock (SyncRoot)
            {
                if (AstraCOM == null)
                {
                    // COM object has been disposed but callback already started. Can't proceed anymore.
                    return;
                }
                var experiment = GetInternalExperiment (experimentID);
                if (experiment == null)
                {
                    Debug.Assert (ClosingExperiments.ContainsKey (experimentID), "Experiment should exist in a callback.");
                    return;
                }

                experiment.Status = ExperimentStatus.Ready;

                ReadEvent.Set ();

                ExperimentStatusChanged?.Invoke (this, experiment);
            }
        }

        private void OnExperimentWrite (int experimentID)
        {
            lock (SyncRoot)
            {
                if (AstraCOM == null)
                {
                    // COM object has been disposed but callback already started. Can't proceed anymore.
                    return;
                }
                var experiment = GetInternalExperiment (experimentID);
                if (experiment == null)
                {
                    Debug.Assert (ClosingExperiments.ContainsKey (experimentID), "Experiment should exist in a callback.");
                    return;
                }

                experiment.Status = ExperimentStatus.Ready;

                WriteEvent.Set ();

                ExperimentStatusChanged?.Invoke (this, experiment);
            }
        }

        private void OnExperimentRun (int experimentID)
        {
            lock (SyncRoot)
            {
                if (AstraCOM == null)
                {
                    // COM object has been disposed but callback already started. Can't proceed anymore.
                    return;
                }
                var experiment = GetInternalExperiment (experimentID);
                if (experiment == null)
                {
                    Debug.Assert (ClosingExperiments.ContainsKey (experimentID), "Experiment should exist in a callback.");
                    return;
                }

                experiment.Status = ExperimentStatus.Ready;

                RunEvent.Set ();

                ExperimentStatusChanged?.Invoke (this, experiment);
            }
        }

        private void OnPreparingForCollection (int experimentID)
        {
            lock (SyncRoot)
            {
                if (AstraCOM == null)
                {
                    // COM object has been disposed but callback already started. Can't proceed anymore.
                    return;
                }
                var experiment = GetInternalExperiment (experimentID);
                if (experiment == null)
                {
                    Debug.Assert (ClosingExperiments.ContainsKey (experimentID), "Experiment should exist in a callback.");
                    return;
                }
                // Add additional experiment status enum value if needed!
                experiment.Status = ExperimentStatus.Busy;

                PreparingForCollectionEvent.Set ();

                ExperimentStatusChanged?.Invoke (this, experiment);
            }
        }

        private void OnWaitingForAutoInject (int experimentID)
        {
            lock (SyncRoot)
            {
                if (AstraCOM == null)
                {
                    // COM object has been disposed but callback already started. Can't proceed anymore.
                    return;
                }
                var experiment = GetInternalExperiment (experimentID);
                if (experiment == null)
                {
                    Debug.Assert (ClosingExperiments.ContainsKey (experimentID), "Experiment should exist in a callback.");
                    return;
                }
                experiment.Status = ExperimentStatus.WaitingForAutoInject;

                WaitingForAutoInjectEvent.Set ();

                ExperimentStatusChanged?.Invoke (this, experiment);
            }
        }

        private void OnCollectionStarted (int experimentID)
        {
            lock (SyncRoot)
            {
                if (AstraCOM == null)
                {
                    // COM object has been disposed but callback already started. Can't proceed anymore.
                    return;
                }
                var experiment = GetInternalExperiment (experimentID);
                if (experiment == null)
                {
                    Debug.Assert (ClosingExperiments.ContainsKey (experimentID), "Experiment should exist in a callback.");
                    return;
                }
                // Add additional experiment status enum value if needed!
                experiment.Status = ExperimentStatus.Busy;
                experiment.HasData = true;

                CollectionStartedEvent.Set ();

                ExperimentStatusChanged?.Invoke (this, experiment);
            }
        }

        private void OnCollectionFinished (int experimentID)
        {
            lock (SyncRoot)
            {
                if (AstraCOM == null)
                {
                    // COM object has been disposed but callback already started. Can't proceed anymore.
                    return;
                }
                var experiment = GetInternalExperiment (experimentID);
                if (experiment == null)
                {
                    Debug.Assert (ClosingExperiments.ContainsKey (experimentID), "Experiment should exist in a callback.");
                    return;
                }
                // Add additional experiment status enum value if needed!
                experiment.Status = ExperimentStatus.Ready;

                CollectionFinishedEvent.Set ();

                ExperimentStatusChanged?.Invoke (this, experiment);
            }
        }

        private void OnInstrumentDetectionCompleted ()
        {
            lock (SyncRoot)
            {
                if (AstraCOM == null)
                {
                    // COM object has been disposed but callback already started. Can't proceed anymore.
                    return;
                }
                InstrumentDetectedSignal.Set ();

                InstrumentsDetected?.Invoke ();
            }
        }

        #endregion

        /// <summary>
        /// Helper function to display the underlying API errors.
        /// </summary>
        /// <remarks>It automatically uses the <see cref="SyncRoot"/> object to guarantee thread safety.</remarks>>
        /// <typeparam name="T">Value returned by <paramref name="func"/>.</typeparam>
        /// <param name="func">Wrapper around an API call to be executed.</param>
        /// <returns>Value of <paramref name="func"/> upon successful completion, or default value of <see cref="T"/>.</returns>
        protected T TryGet<T> (Func<T> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException (nameof (func));
            }
            try
            {
                lock (SyncRoot)
                {
                    return func ();
                }
            }
            catch (Exception ex)
            {
                if (ShouldShowErrorMessageBox)
                {
                    var msg = AstraCOM.GetErrorMessage (ex.HResult);
                    MessageBox.Show ($"{(msg?.Length > 0 ? msg : ex.Message)}" + SuppressErrorMessage, "Automation Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                }
                else
                    throw ex;
            }
            return default (T);
        }

        /// <summary>
        /// Helper function to display the underlying API errors upon failure.
        /// </summary>
        /// <remarks>It automatically uses the <see cref="SyncRoot"/> object to guarantee thread safety.</remarks>>
        /// <param name="action">Wrapper around an API call to be executed.</param>
        /// <returns>True if <paramref name="action"/> completes without a failure, false otherwise.</returns>
        protected bool TryExecute (Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException (nameof (action));
            }
            try
            {
                lock (SyncRoot)
                {
                    action ();
                    return true;
                }
            }
            catch (Exception ex)
            {
                if (ShouldShowErrorMessageBox)
                {
                    var msg = AstraCOM.GetErrorMessage (ex.HResult);
                    MessageBox.Show ($"{(msg?.Length > 0 ? msg : ex.Message)}" + SuppressErrorMessage, "Automation Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                }
                else
                    throw ex;
            }
            return false;
        }

        /// <summary>
        /// Helper function to display the underlying API errors upon failure.
        /// And wait for experiment to finish running then continue.
        /// </summary>
        /// <remarks>It automatically uses the <see cref="SyncRoot"/> object to guarantee thread safety.</remarks>>
        /// <param name="action">Wrapper around an API call to be executed.</param>
        /// <returns>True if <paramref name="action"/> completes without a failure, false otherwise.</returns>
        protected bool TryExecuteAndWaitExperimentRun (Action action)
        {
            var success = false;
            if (action == null)
            {
                throw new ArgumentNullException (nameof (action));
            }
            try
            {
                lock (SyncRoot)
                {
                    action ();
                    success = true;
                }
            }
            catch (Exception ex)
            {
                if (ShouldShowErrorMessageBox)
                {
                    var msg = AstraCOM.GetErrorMessage (ex.HResult);
                    MessageBox.Show ($"{(msg?.Length > 0 ? msg : ex.Message)}" + SuppressErrorMessage, "Automation Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                }
            }
            if (success)
            {
                WaitExperimentRun ();
            }
            return success;
        }

        public void RefreshExperiment (int experimentID)
        {
            lock (SyncRoot)
            {
                var experiment = GetInternalExperiment (experimentID);
                experiment.Read ();
                ExperimentStatusChanged?.Invoke (this, experiment);
            }
        }
    }
}
