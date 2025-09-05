using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts;
using Assets._vGISComponents.Components.Gnss.Shared;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Monitors
{
    public class InsufficientAccuracyMonitor : GnssIssueMonitor
    {
        public InsufficientAccuracyMonitor(IGnssManager gnssManager, IssueMonitorInfo monitorInfo, int totalStepsCount) 
            : base(gnssManager, monitorInfo, totalStepsCount) { }

        protected override bool IsIssueDetectedNow()
        {
            // If actual accuracy bigger than max acceptable threshold
            // Detect an issue.
            return true;
        }
    }
}
