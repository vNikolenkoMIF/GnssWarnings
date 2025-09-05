using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts;
using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Monitors;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Factories
{
    public interface IGnssMonitorsFactory
    {
        public IGnssIssueMonitor Create(IssueMonitorInfo monitorInfo, int loopRangeSeconds);
    }
}