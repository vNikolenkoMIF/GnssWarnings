using System;
using System.Collections.Generic;
using System.Linq;
using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts;
using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Factories;
using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Impacts;
using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Messages;
using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Monitors;
using Assets._vGISComponents.Components.Gnss.Shared;
using Assets._vGISComponents.Core.Shared.Scripts.Extensions;
using Assets._vGISComponents.Core.Shared.Scripts.WebAPI.vGISCompaniesAPI.ViewModels.ApplicationOptions;
using Meemim.VGis.Unity.Plugins.Interfaces.Log;
using UnityEngine;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts
{
    public class GnssIssueMonitoringSystem : MonoBehaviour
    {
        #region Constants
        private const int MONITORING_LOOP_RANGE_SEC = 5;
        private const int UPDATE_FREQUENCY_SEC = 5;
        #endregion Constants

        #region Injections
        [SerializeField]
        private WarningsTypeRegistry[] _registeredMonitors = new WarningsTypeRegistry[Enum.GetNames(typeof(GnssWarningsRegistry)).Length];

        [SerializeField]
        private ImpactRelation[] _issuesImpactsMapping = new ImpactRelation[Enum.GetNames(typeof(GnssIssue)).Length];
        #endregion Injections

        #region Properties
        public bool IsRunning { get; private set; }

        public GnssIssue[] MonitoringIssues
        {
            get
            {
                var register = _registeredMonitors.FirstOrDefault(r => r.RegisteredForType == _activeType);
                return register == null 
                    ? Array.Empty<GnssIssue>() 
                    : register.MonitoringIssues.Select(issue => issue.IssueType).ToArray();
            }
        }

        public IReadOnlyDictionary<GnssIssue, bool> LastIssuesStatuses => _lastIssuesStates;
        #endregion Properties

        #region Events
        public event Action IssuesChanged;
        #endregion Events

        #region Private scope
        #region Fields
        private IGnssMonitorsFactory _gnssMonitorsFactory;
        private IssueMonitoringWorker _monitoringWorker;
        private ILog _log;
        #endregion Fields

        #region Collections
        private GnssWarningsRegistry _activeType = GnssWarningsRegistry.None;

        private Dictionary<int, List<IGnssIssueMonitor>> _activeMonitors;
        private Dictionary<GnssIssue, bool> _lastIssuesStates = new Dictionary<GnssIssue, bool>();
        private Dictionary<GnssIssue, IGnssIssueImpact> _issueImpacts;

        private readonly HashSet<IGnssIssuesNotifier> _notifiers = new HashSet<IGnssIssuesNotifier>();
        #endregion Collections
        #endregion Private scope

        #region Initialization
        public void Initialize(IGnssManager gnssManager)
        {
            _gnssMonitorsFactory = new GnssMonitorFactory(gnssManager);
            _monitoringWorker = new IssueMonitoringWorker(MONITORING_LOOP_RANGE_SEC, UPDATE_FREQUENCY_SEC);

            _log = LogManager.GetLogger(nameof(GnssIssueMonitoringSystem));

            var impactsFactory = new IssueImpactFactory();

            _issueImpacts = GetImpacts(impactsFactory);

            SetActiveMonitors(_activeType);
        }
        #endregion Initialization

        #region Public API
        public void SwitchMonitoringRegistry(GnssWarningsRegistry newType, bool continueMonitoring)
        {
            if (_activeType == newType) {
                return;
            }
            
            if (IsRunning) {
                StopMonitoringInternal();
            }

            SetActiveMonitors(newType);
            SetNotifiersActive(newType);

            if (continueMonitoring) {
                StartMonitoringInternal();
            }
        }

        public void StartMonitoring()
        {
            StartMonitoringInternal();
        }

        public void StopMonitoring()
        {
            StopMonitoringInternal();
        }

        public bool IsIssueActive(GnssIssue gnssIssue)
        {
            return _lastIssuesStates[gnssIssue];
        }

        #region Notifiers register/unregister
        public bool RegisterNotifier(IGnssIssuesNotifier notifier)
        {
            if (notifier == null) {
                throw new ArgumentNullException(nameof(notifier));
            }

            return _notifiers.Add(notifier);
        }

        public bool Unregister(IGnssIssuesNotifier notifier)
        {
            if (notifier == null) {
                throw new ArgumentNullException(nameof(notifier));
            }

            return _notifiers.Remove(notifier);
        }
        #endregion Notifiers register/unregister
        #endregion Public API

        #region Private methods
        #region Monitors
        private void StartMonitoringInternal()
        {
            if (_activeMonitors == null) {
                _log.LogError("There are no monitors were initialized. Initialize it before start.");
                return;
            }

            if (_activeMonitors.Count == 0) {
                return;
            }

            _monitoringWorker.Start();
            _monitoringWorker.TimeRangeElapsed += CheckMonitors;

            IsRunning = true;
        }

        private void StopMonitoringInternal()
        {
            if (_activeMonitors == null) {
                _log.LogError("There are no monitors were initialized. Initialize it before stop.");
                return;
            }

            if (_activeMonitors.Count == 0) {
                return;
            }
            
            _monitoringWorker.Stop();
            _monitoringWorker.TimeRangeElapsed -= CheckMonitors;
            IsRunning = false;
        }

        private void SetActiveMonitors(GnssWarningsRegistry warningsType)
        {
            if (IsRunning) {
                _log.LogError("Stop running to switch active monitors");
                return;
            }

            ClearMonitors();

            _activeType = warningsType;

            var monitors = GetMonitors(warningsType);

            _activeMonitors = ArrangeMonitorsByPriority(monitors.Values);

            _lastIssuesStates = monitors.
                ToDictionary(x => x.Key, x => x.Value.IssueDetected);

            foreach (var monitor in monitors.Values)
            {
                _monitoringWorker.AddListener(monitor);
            }
        }

        private void ClearMonitors()
        {
            _monitoringWorker.Reset();

            foreach (var impact in _issueImpacts.Values)
            {
                impact.Reset();
            }

            _activeMonitors = null;
            _lastIssuesStates.Clear();
        }

        private Dictionary<GnssIssue, IGnssIssueMonitor> GetMonitors(GnssWarningsRegistry warningsType)
        {
            var suitableRegister = _registeredMonitors.FirstOrDefault(r => r.RegisteredForType == warningsType);

            if (suitableRegister == null) {
                _log.LogError($"The type {warningsType} is not registered for monitoring. No monitors was returned");
                return new Dictionary<GnssIssue, IGnssIssueMonitor>();
            }

            if (suitableRegister.MonitoringIssues == null) {
                _log.LogError($"The type {warningsType} is registered but list of monitors is null. No monitors was returned");
                return new Dictionary<GnssIssue, IGnssIssueMonitor>();
            }

            return suitableRegister.GetMonitors(_gnssMonitorsFactory);
        }

        private void CheckMonitors()
        {
            var needToUpdate = false;

            for (var currentPriority = 0; currentPriority < _activeMonitors.Count; currentPriority++) {
                var monitorsHaveChanges = UpdateMonitors(_activeMonitors[currentPriority], out var interruptChecking);

                if (!needToUpdate) {
                    needToUpdate = monitorsHaveChanges;
                }

                if (interruptChecking) {
                    break;
                }
            }

            if (needToUpdate) {
                RunRegisteredNotifiers(_lastIssuesStates);
                IssuesChanged?.Invoke();
            }
        }

        private static Dictionary<int, List<IGnssIssueMonitor>> ArrangeMonitorsByPriority(IEnumerable<IGnssIssueMonitor> monitors)
        {
            if (monitors.IsNullOrEmpty())
            {
                return new Dictionary<int, List<IGnssIssueMonitor>>();
            }

            return monitors
                .GroupBy(m => m.MonitorInfo.PriorityLayer)
                .ToDictionary(x => x.Key, x => x.ToList());
        }

        private bool UpdateMonitors(IEnumerable<IGnssIssueMonitor> monitors, out bool interrupt)
        {
            var hasChanges = false;
            interrupt = false;

            foreach (var monitor in monitors)
            {
                interrupt = ShouldInterrupt(monitor);

                var monitorIssueType = monitor.MonitorInfo.IssueType;

                var previousState = _lastIssuesStates[monitorIssueType];

                var issueDetected = monitor.IssueDetected;

                if (previousState == issueDetected) {

                    if (interrupt) {
                        break;
                    }

                    continue;
                }

                _lastIssuesStates[monitorIssueType] = issueDetected;

                if (issueDetected) {
                    ImpactOnSystemIfPossible(monitorIssueType);
                }
                else {
                    RestoreSystemIfPossible(monitorIssueType);
                }

                hasChanges = true;

                if (interrupt) {
                    break;
                }
            }

            return hasChanges;
        }

        private bool ShouldInterrupt(IGnssIssueMonitor monitor)
        {
            return monitor.MonitorInfo.InterruptNextLayers && monitor.IssueDetected;
        }
        #endregion Monitors

        #region Impacts

        private Dictionary<GnssIssue, IGnssIssueImpact> GetImpacts(IssueImpactFactory factory)
        {
            if (_issuesImpactsMapping.IsNullOrEmpty()) {
                _log.LogError("System doesn't have configured impacts, it's not allowed");
                return new Dictionary<GnssIssue, IGnssIssueImpact>();
            }

            return _issuesImpactsMapping
                .ToDictionary(x => x.ReferToIssue, x => factory.GetImpactType(x.ImpactType));
        }

        private void ImpactOnSystemIfPossible(GnssIssue issueType)
        {
            if (!_issueImpacts.ContainsKey(issueType))
            {
                _log.LogError($"Impact feedback for issue {issueType} is not configured");
                return;
            }

            var impact = _issueImpacts[issueType];
            impact.TryMakeImpact(issueType);
        }

        private void RestoreSystemIfPossible(GnssIssue issueType)
        {
            if (!_issueImpacts.ContainsKey(issueType)) {
                _log.LogError($"System restoring for issue {issueType} was not configured");
                return;
            }

            var impact = _issueImpacts[issueType];
            impact.TryRestoreSystem(issueType);
        }

        #endregion Impacts

        #region Notifiers
        private void RunRegisteredNotifiers(Dictionary<GnssIssue, bool> activeIssues)
        {
            foreach (var notifier in _notifiers) 
            {
                ApplyIssuesChanges(notifier, activeIssues);
            }
        }

        private void SetNotifiersActive(GnssWarningsRegistry warningsType)
        {
            foreach (var notifier in _notifiers) {
                notifier.ClearAllMessages();
                notifier.SetNotifierActive(warningsType != GnssWarningsRegistry.None);
            }
        }
        private static void ApplyIssuesChanges(IGnssIssuesNotifier notifier, Dictionary<GnssIssue, bool> activeIssues)
        {
            var newIssues = activeIssues.Where(i => i.Value != notifier.IsMessageActive(i.Key));

            foreach (var newIssue in newIssues)
            {
                var isActiveIssue = newIssue.Value;

                if (isActiveIssue) {
                    notifier.NotifyAboutIssue(newIssue.Key);
                }
                else {
                    notifier.StopNotify(newIssue.Key);
                }
            }
        }
        #endregion Notifiers
        #endregion Private methods
    }
}