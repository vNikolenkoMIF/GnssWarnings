using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Monitors
{
    public interface IGnssIssueMonitor : IMonitoringLifecycleListener
    {
        public IssueMonitorInfo MonitorInfo { get; }

        public bool IssueDetected { get; }
    }
}