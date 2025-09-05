using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts;
using Assets._vGISComponents.Components.Gnss.Shared;
using Assets._vGISComponents.Core.Shared.Scripts.Contracts;
using Assets._vGISComponents.Core.Shared.Scripts.WebAPI.vGISCompaniesAPI.ViewModels;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Monitors
{
    public class DataCollectionAccuracyMonitor : GnssIssueMonitor
    {
        public DataCollectionAccuracyMonitor(IGnssManager gnssManager, IssueMonitorInfo monitorInfo, 
            int totalStepsCount) : base(gnssManager, monitorInfo, totalStepsCount)
        {
        }

        protected override bool IsIssueDetectedNow()
        {
            // If actual accuracy bigger than min acceptable threshold for DC
            // Detect an issue.
            return true;
        }
    }
}