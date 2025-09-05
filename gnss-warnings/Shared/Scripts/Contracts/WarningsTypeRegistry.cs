using System;
using System.Collections.Generic;
using System.Linq;
using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Factories;
using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Monitors;
using Assets._vGISComponents.Core.Shared.Scripts.Extensions;
using Assets._vGISComponents.Core.Shared.Scripts.WebAPI.vGISCompaniesAPI.ViewModels.ApplicationOptions;
using UnityEngine;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts
{
    [Serializable]
    public class IssueMonitorInfo
    {
        public GnssIssue IssueType;

        [Tooltip("Configures execution queue of the monitor. The smallest number will have bigger priority")]
        public int PriorityLayer;

        [Tooltip("When true - this monitor will be the last updated monitor. Next monitors after this will be ignored")]
        public bool InterruptNextLayers;
    }

    [Serializable]
    public class WarningsTypeRegistry
    {
        public GnssWarningsRegistry RegisteredForType;

        public IssueMonitorInfo[] MonitoringIssues;

        public int RunningLoopRange = 5;

        public Dictionary<GnssIssue, IGnssIssueMonitor> GetMonitors(IGnssMonitorsFactory factory)
        {
            if (factory == null) {
                throw new ArgumentNullException(nameof(factory));
            }

            return MonitoringIssues.IsNullOrEmpty()
                ? new Dictionary<GnssIssue, IGnssIssueMonitor>()
                : MonitoringIssues.ToDictionary(m => m.IssueType, m => factory.Create(m, RunningLoopRange));
        }
    }
}