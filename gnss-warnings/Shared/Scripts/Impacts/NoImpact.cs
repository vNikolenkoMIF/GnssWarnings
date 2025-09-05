using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Impacts
{
    public class NoImpact : IGnssIssueImpact
    {
        public bool AlreadyMakingImpact => true;

        public bool TryMakeImpact(GnssIssue causeByIssue)
        {
            return false;
        }

        public bool TryRestoreSystem(GnssIssue resolvedIssue)
        {
            return false;
        }

        public void Reset()
        {

        }
    }
}