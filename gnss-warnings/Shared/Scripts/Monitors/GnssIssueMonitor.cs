using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts;
using Assets._vGISComponents.Components.Gnss.Shared;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Monitors
{
    public abstract class GnssIssueMonitor : IGnssIssueMonitor
    {
        public IssueMonitorInfo MonitorInfo { get; }
        public bool IssueDetected { get; private set; }

        protected IGnssManager GnssManager { get; }

        private readonly int _totalStepsCount;
        private int _badResultsCount;

        protected GnssIssueMonitor(IGnssManager gnssManager, IssueMonitorInfo monitorInfo, int totalStepsCount)
        {
            GnssManager = gnssManager;
            MonitorInfo = monitorInfo;

            _totalStepsCount = totalStepsCount;
        }

        public virtual void BeforeRangeStart() { }

        public virtual void AfterStep()
        {
            if (IsIssueDetectedNow()) {
                _badResultsCount++;
            }
        }

        public virtual void AfterRangeEnd()
        {
            IssueDetected = HasIssueInMeasuredPeriod();
            Reset();
        }

        public void Reset()
        {
            _badResultsCount = 0;
        }

        protected abstract bool IsIssueDetectedNow();

        private bool HasIssueInMeasuredPeriod()
        {
            var goodResultsCount = _totalStepsCount - _badResultsCount;
            return _badResultsCount > goodResultsCount;
        }
    }
}