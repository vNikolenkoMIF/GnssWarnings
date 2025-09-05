using System;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts
{
    [Serializable]
    public class ImpactRelation
    {
        public GnssIssue ReferToIssue;

        public ImpactType ImpactType;
    }
}
