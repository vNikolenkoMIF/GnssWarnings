namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts
{
    public interface IMonitoringLifecycleListener
    {
        /// <summary>
        /// Triggers when monitoring loop starts to run.
        /// </summary>
        public void BeforeRangeStart();

        /// <summary>
        /// Triggers when monitoring worker has finished a single step of the loop.
        /// </summary>
        public void AfterStep();

        /// <summary>
        /// Triggers when monitoring worker finished the the loop in the range.
        /// </summary>
        public void AfterRangeEnd();

        /// <summary>
        /// Reset internal timers.
        /// </summary>
        public void Reset();
    }
}