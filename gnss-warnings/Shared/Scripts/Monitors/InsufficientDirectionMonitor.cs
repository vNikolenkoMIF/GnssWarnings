using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts;
using Assets._vGISComponents.Components.Gnss.Shared;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Monitors
{
    public class InsufficientDirectionMonitor : GnssIssueMonitor
    {
        public InsufficientDirectionMonitor(IGnssManager gnssManager, IssueMonitorInfo monitorInfo, int totalStepsCount) 
            : base(gnssManager, monitorInfo, totalStepsCount) { }


        protected override bool IsIssueDetectedNow()
        {
            // If actual H. accuracy bigger than min acceptable threshold.
            // Detect an issue.
            return true;
        }
    }
}