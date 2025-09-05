using System;
using System.Collections.Generic;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts
{
    internal sealed class MonitoringEventsManger
    {
        public int ListenersCount => _listeners.Count;

        private HashSet<IMonitoringLifecycleListener> _listeners = new HashSet<IMonitoringLifecycleListener>();

        public bool AddListener(IMonitoringLifecycleListener listener)
        {
            if (listener == null) {
                throw new ArgumentNullException(nameof(listener));
            }

            return _listeners.Add(listener);
        }

        public bool RemoveListener(IMonitoringLifecycleListener listener)
        {
            if (listener == null) {
                throw new ArgumentNullException(nameof(listener));
            }

            return _listeners.Remove(listener);
        }

        public void ClearAllListeners()
        {
            _listeners.Clear();
        }

        public void DoBeforeRangeStart()
        {
            foreach (var listener in _listeners)
            {
                listener.BeforeRangeStart();
            }
        }

        public void DoAfterStep()
        {
            foreach (var listener in _listeners)
            {
                listener.AfterStep();
            }
        }

        public void DoAfterRangeEnd()
        {
            foreach (var listener in _listeners)
            {
                listener.AfterRangeEnd();
            }
        }

        public void Reset()
        {
            foreach (var listener in _listeners)
            {
                listener.Reset();
            }
        }
    }
}