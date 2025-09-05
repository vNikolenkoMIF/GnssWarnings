using System;
using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts;
using Assets._vGISComponents.Components.Gnss.Shared;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Monitors
{
    public class AccuracyJumpsMonitor : GnssIssueMonitor
    {
        public AccuracyJumpsMonitor(IGnssManager gnssManager, IssueMonitorInfo monitorInfo, int totalStepsCount) 
            : base(gnssManager, monitorInfo, totalStepsCount) { }


        public override void BeforeRangeStart()
        {
            base.BeforeRangeStart();
            //Remember initial accuracy
        }

        public override void AfterRangeEnd()
        {
            base.AfterRangeEnd();
            // Reset accuracy
        }

        public override void AfterStep()
        {
            base.AfterStep();
            // Write new accuracy
        }

        protected override bool IsIssueDetectedNow()
        {
            // TODO: Validate acceptable threshold
            // Get delta between previous and current accuracy
            // If delta bigger than threshold
            // Register accuracy jump
            return true;
        }
    }
}
