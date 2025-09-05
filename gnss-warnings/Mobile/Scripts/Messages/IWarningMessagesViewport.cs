using Assets._Project.Components.GnssMonitoring.Mobile.Scripts.UI;
using UnityEngine;

namespace Assets._Project.Components.GnssMonitoring.Mobile.Scripts.Messages
{
    public interface IWarningMessagesViewport
    {
        public bool IsViewportLocked { get; set; }

        public void SnapMessagesViewportTo(RectTransform targetRect, Place messagesPlace, float offset);
    }
}
