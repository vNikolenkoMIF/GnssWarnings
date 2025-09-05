using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts;
using Assets._vGISComponents.Components.Gnss.Shared;
using Meemim.VGis.Unity.Plugins.Interfaces.Dto;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Monitors
{
    public class BadRtkQualityMonitor : GnssIssueMonitor
    {
        public BadRtkQualityMonitor(IGnssManager gnssManager, IssueMonitorInfo monitorInfo, int totalStepsCount)
            : base(gnssManager, monitorInfo, totalStepsCount)
        {
        }

        protected override bool IsIssueDetectedNow()
        {
            // Check RTK quality.
            return true;
        }
    }
}