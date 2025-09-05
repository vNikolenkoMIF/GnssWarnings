using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Messages
{
    public interface IGnssIssuesNotifier
    {
        public void NotifyAboutIssue(GnssIssue gnssIssue);

        public void StopNotify(GnssIssue gnssIssue);

        public bool IsMessageActive(GnssIssue gnssIssue);

        public void SetNotifierActive(bool isActive);

        public void ClearAllMessages();
    }
}
