using System.Collections.Generic;

namespace Assets._Project.Components.GnssMonitoring.Shared.Scripts.Messages
{
    public class MessagesEqualityComparer : IEqualityComparer<IWarningMessage>
    {
        public bool Equals(IWarningMessage x, IWarningMessage y)
        {
            if (ReferenceEquals(null, x)) return false;
            if (ReferenceEquals(null, y)) return false;

            return x.IssueType.Equals(y.IssueType);
        }

        public int GetHashCode(IWarningMessage obj)
        {
            var hashCode = 102335678;
            hashCode = hashCode * -12243580 + base.GetHashCode();
            hashCode += obj.IssueType.GetHashCode();
            return hashCode;
        }
    }
}