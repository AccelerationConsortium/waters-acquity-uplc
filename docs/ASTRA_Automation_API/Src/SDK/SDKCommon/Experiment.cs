// ************************************************************************
// (c) Copyright 2021 by Wyatt Technology Corporation. All rights reserved.
// ************************************************************************
using AstraLib;
using System;
using System.Threading;

namespace SDKCommon
{
    public enum ExperimentStatus
    {
        Ready,
        WaitingForAutoInject,
        Executed,
        Busy
    }

    // Experiment wrapper: although all experiment operations can be performed through
    // the AstraAdmin singleton, this class wraps calls to get/set various properties,
    // providing a better sense of encapsulation.
    public class Experiment
    {
        // Constructor: retrieve experiment name and initialize data.
        public Experiment (int experimentID)
        {
            Id = experimentID;
            Status = ExperimentStatus.Ready;
            Name = AstraAdmin.Get.GetExperimentName (experimentID);
            HasData = false;
        }

        public Experiment Clone ()
        {
            return MemberwiseClone () as Experiment;
        }

        // Synch differences in experiment properties with ASTRA.
        public void Apply ()
        {
            // Make sure to wait for the experiment to be done running before doing any changes.
            AstraAdmin.Get.WaitExperimentNotRunning (Id);

            // Get the value of `HasVisionUv` before we start modifying the experiment.
            var hasVisionUv = AstraAdmin.Get.HasVisionUv (Id);

            // Description
            if (Data.Description != SynchronizedData.Description)
            {
                AstraAdmin.Get.SetExperimentDescription (Id, Data.Description);
            }
            // Collection duration
            if (Math.Abs (Data.CollectionDuration - SynchronizedData.CollectionDuration) > Tolerance)
            {
                AstraAdmin.Get.SetCollectionDuration (Id, Data.CollectionDuration);
            }

            // Pump/injector properties
            if (Math.Abs (Data.FlowRate - SynchronizedData.FlowRate) > Tolerance)
            {
                AstraAdmin.Get.SetPumpFlowRate (Id, Data.FlowRate);
            }

            if (Math.Abs (Data.InjectedVolume - SynchronizedData.InjectedVolume) > Tolerance)
            {
                AstraAdmin.Get.SetInjectedVolume (Id, Data.InjectedVolume);
            }

            // Sample properties
            if (Data.Sample.name != SynchronizedData.Sample.name ||
                Data.Sample.description != SynchronizedData.Sample.description ||
                Math.Abs (Data.Sample.dndc - SynchronizedData.Sample.dndc) > Tolerance ||
                Math.Abs (Data.Sample.a2 - SynchronizedData.Sample.a2) > Tolerance ||
                Math.Abs (Data.Sample.uvExtinction - SynchronizedData.Sample.uvExtinction) > Tolerance ||
                Math.Abs (Data.Sample.concentration - SynchronizedData.Sample.concentration) > Tolerance)
            {
                AstraAdmin.Get.SetSample (Id, Data.Sample);
            }

            if (hasVisionUv)
            {
                var deviceDetails = new UvDeviceDetails
                {
                    deviceName = "VWD/MWD/DAD",
                    deviceModel = "G9999z",
                    supportsPeakWidth = 1,
                    peakWidth = "> 0.1 min (2 s response time) (2.5 Hz)",
                    supportsSlitWidth = 1,
                    slitWidth = "4",
                    supportsRequireLampUV = 1,
                    requireLampUV = 1,
                    supportsRequireLampVis = 1,
                    requireLampVis = 1,
                    uvChannels = new UvChannelDetails[4]
                };

                var channel = new UvChannelDetails
                {
                    useChannel = 1,
                    waveLength = 280.1,
                    bandwidth = 4,
                    useReference = 0,
                    refWaveLength = 360.1,
                    refBandwidth = 101
                };

                deviceDetails.uvChannels.SetValue (channel, 0);

                channel.useChannel = 1;
                channel.waveLength = 280.2;
                channel.bandwidth = 5;
                channel.useReference = 0;
                channel.refWaveLength = 360.2;
                channel.refBandwidth = 102;
                deviceDetails.uvChannels.SetValue (channel, 1);

                channel.useChannel = 0;
                channel.waveLength = 280.3;
                channel.bandwidth = 6;
                channel.useReference = 1;
                channel.refWaveLength = 360.3;
                channel.refBandwidth = 103;
                deviceDetails.uvChannels.SetValue (channel, 2);

                channel.useChannel = 1;
                channel.waveLength = 280.4;
                channel.bandwidth = 7;
                channel.useReference = 0;
                channel.refWaveLength = 360.4;
                channel.refBandwidth = 104;
                deviceDetails.uvChannels.SetValue (channel, 3);

                AstraAdmin.Get.SetupVisionUv (Id, deviceDetails);
            }

            SynchronizedData = Data;

            // Make sure to wait for the experiment to be done running before retrieving the new state.
            // Note that only one setter is changed at a time, so it is safe to wait for running here.
            // If multiple settings were changed, we have to wait after each setting change.
            AstraAdmin.Get.WaitExperimentNotRunning (Id);
            AstraAdmin.Get.RefreshExperiment (Id);
        }

        // Get experiment settings from ASTRA.
        public void Read ()
        {
            if (Id == 0)
            {
                return;
            }

            SynchronizedData.Description = AstraAdmin.Get.GetExperimentDescription (Id);
            SynchronizedData.CollectionDuration = AstraAdmin.Get.GetCollectionDuration (Id);
            SynchronizedData.FlowRate = AstraAdmin.Get.GetPumpFlowRate (Id);
            SynchronizedData.InjectedVolume = AstraAdmin.Get.GetInjectedVolume (Id);
            SynchronizedData.Sample = AstraAdmin.Get.GetSample (Id);

            Data = SynchronizedData;
        }

        // Reset experiment state to synched state. Synched state will be the
        // last applied state, or the initial state.
        public void Reset ()
        {
            Data = SynchronizedData;
        }

        // POD struct for holding experiment class data
        private struct ExperimentData
        {
            public string Description;
            public double CollectionDuration;
            public double FlowRate;
            public double InjectedVolume;
            public SampleInfo Sample;
        }

        #region Fields

        private const double Tolerance = 1e-6;
        private ExperimentData SynchronizedData;
        private ExperimentData Data;

        #endregion

        #region Properties

        public string Name { get; protected set; }
        public int Id { get; protected set; }
        public ExperimentStatus Status { get; set; }
        public bool HasData { get; set; }

        public string Description
        {
            get { return Data.Description; }
            set { Data.Description = value; }
        }

        public double CollectionDuration
        {
            get { return Data.CollectionDuration; }
            set { Data.CollectionDuration = value; }
        }

        public double FlowRate
        {
            get { return Data.FlowRate; }
            set { Data.FlowRate = value; }
        }

        public double InjectedVolume
        {
            get { return Data.InjectedVolume; }
            set { Data.InjectedVolume = value; }
        }

        public SampleInfo Sample
        {
            get { return Data.Sample; }
            set { Data.Sample = value; }
        }

        #endregion
    }
}