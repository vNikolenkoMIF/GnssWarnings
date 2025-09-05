using System;
using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts;
using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Monitors;
using Assets._vGISComponents.Components.Gnss.Shared;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Factories
{
    // This is the simpliest version of Factory because no need to do it complicated
    // I just want to decouple this code from the high-level components
    public class GnssMonitorFactory : IGnssMonitorsFactory
    {
        private readonly IGnssManager _gnssManager;

        public GnssMonitorFactory(IGnssManager gnssManager)
        {
            _gnssManager = gnssManager;
        }

        public IGnssIssueMonitor Create(IssueMonitorInfo monitorInfo, int loopRangeSeconds)
        {
            return monitorInfo.IssueType switch
            {
                GnssIssue.GnssDisconnected => new GnssConnectionIssueMonitor(_gnssManager, monitorInfo, loopRangeSeconds),
                GnssIssue.RtkFixBecameFloat => new BadRtkQualityMonitor(_gnssManager, monitorInfo, loopRangeSeconds),
                GnssIssue.RtkInvalid => new BadRtkQualityMonitor(_gnssManager, monitorInfo, loopRangeSeconds),
                GnssIssue.FluctuatingAccuracy => new AccuracyJumpsMonitor(_gnssManager, monitorInfo, loopRangeSeconds),
                GnssIssue.InsufficientAccuracy => new InsufficientAccuracyMonitor(_gnssManager, monitorInfo, loopRangeSeconds),
                GnssIssue.DirectionInsufficientAccuracy => new InsufficientDirectionMonitor(_gnssManager, monitorInfo, loopRangeSeconds),
                GnssIssue.DataCollectionInsufficientAccuracy => new DataCollectionAccuracyMonitor(_gnssManager, monitorInfo, loopRangeSeconds),
                _ => throw new ArgumentOutOfRangeException(nameof(monitorInfo.IssueType), monitorInfo.IssueType, null)
            };
        }
    }
}