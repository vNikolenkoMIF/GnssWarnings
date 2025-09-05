using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts;
using Assets._vGISComponents.Components.Gnss.Shared;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Monitors
{
    public class GnssConnectionIssueMonitor : GnssIssueMonitor
    {
        public GnssConnectionIssueMonitor(IGnssManager gnssManager, IssueMonitorInfo monitorInfo, int totalStepsCount) 
            : base(gnssManager, monitorInfo, totalStepsCount)
        {
        }

        protected override bool IsIssueDetectedNow()
        {
            // Check when GNSS connection is lost 
            return true;
        }
    }
}
