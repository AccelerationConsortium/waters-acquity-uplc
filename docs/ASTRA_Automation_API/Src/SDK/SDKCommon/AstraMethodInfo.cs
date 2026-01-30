// ************************************************************************
// (c) Copyright 2022 by Wyatt Technology Corporation. All rights reserved.
// ************************************************************************
using AstraLib;

namespace SDKCommon
{
    /// <summary>
    /// Information about a method that can be set at the end of a collection.
    /// </summary>
    public class AstraMethodInfo
    {
        /// <summary>
        /// Save location for experiment.
        /// </summary>
        public string ExperimentPath;

        /// <summary>
        /// Sample info for experiment.
        /// </summary>
        public SampleInfo Sample;

        /// <summary>
        /// Injected volume in mL.
        /// </summary>
        public double InjectedVolume;

        /// <summary>
        /// Flow rate in mL/min. If 0, ignored.
        /// </summary>
        public double FlowRate;

        /// <summary>
        /// Duration of collection in min.
        /// </summary>
        public double Duration;
    }
}