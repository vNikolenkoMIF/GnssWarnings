using System;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts
{
    [Serializable]
    public class MessageInfo
    {
        public GnssIssue RelatedToIssue;

        public string MessageOrTerm;

        public AffectedIndicator[] AffectsOnIndicators;
    }
}