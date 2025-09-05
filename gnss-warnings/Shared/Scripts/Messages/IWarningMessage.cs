using System;
using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Messages
{
    public interface IWarningMessage
    {
        public GnssIssue IssueType { get; }

        public DateTime? LastDisabledTimeUtc { get; set; }

        public AffectedIndicator[] AffectsOnIndicators { get; }

        public event Action<GnssIssue> CloseMessage;

        public void SetWarningInfo(MessageInfo messageInfo);

        public void SetActive(bool isActive);

        public void Dispose();
    }
}
