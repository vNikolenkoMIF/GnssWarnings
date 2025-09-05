using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Impacts
{
    public interface IGnssIssueImpact
    {
        public bool AlreadyMakingImpact { get; }

        public bool TryMakeImpact(GnssIssue causeByIssue);

        public bool TryRestoreSystem(GnssIssue resolvedIssue);

        public void Reset();
    }
}