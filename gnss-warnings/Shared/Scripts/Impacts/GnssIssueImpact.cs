using System.Collections.Generic;
using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Impacts
{
    public abstract class GnssIssueImpact : IGnssIssueImpact
    {
        public bool AlreadyMakingImpact { get; private set; }

        private readonly HashSet<GnssIssue> _registeredIssues = new HashSet<GnssIssue>();

        public bool TryMakeImpact(GnssIssue causeByIssue)
        {
            if (!RegisterIssue(causeByIssue)) {
                return false;
            }

            if (AlreadyMakingImpact) {
                return false;
            }

            MakeImpact();
            AlreadyMakingImpact = true;

            return true;
        }

        public bool TryRestoreSystem(GnssIssue resolvedIssue)
        {
            UnregisterIssue(resolvedIssue);

            if (!HasIssues()) {
                StopImpact();
                AlreadyMakingImpact = false;
                return true;
            } 
            else {
                return false;
            }
        }

        public void Reset()
        {
            StopImpact();
            _registeredIssues.Clear();
            AlreadyMakingImpact = false;
        }

        protected abstract void MakeImpact();

        protected abstract void StopImpact();

        private bool RegisterIssue(GnssIssue gnssIssue)
        {
            return _registeredIssues.Add(gnssIssue);
        }

        private bool UnregisterIssue(GnssIssue gnssIssue)
        {
            return _registeredIssues.Remove(gnssIssue);
        }

        private bool HasIssues()
        {
            return _registeredIssues.Count != 0;
        }
    }
}