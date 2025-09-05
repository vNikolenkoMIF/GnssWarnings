using System;
using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts;
using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Messages;
using Assets._Project.Shared.Scripts.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Assets._Project.Components.GnssMonitoring.Mobile.Scripts.Messages
{
    public class WarningMessage : MonoBehaviour, IWarningMessage
    {
        [SerializeField]
        private TMP_Text _textArea;

        [SerializeField]
        private Button _closeButton;

        private GnssIssue _issueType;

        public GnssIssue IssueType { get; private set; }
        
        public DateTime? LastDisabledTimeUtc { get; set; }
        public AffectedIndicator[] AffectsOnIndicators { get; private set;  }

        public event Action<GnssIssue> CloseMessage;

        private void Awake()
        {
            Assert.IsNotNull(_textArea);
            Assert.IsNotNull(_closeButton);
        }

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(TriggerMessageClose);
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(TriggerMessageClose);
        }

        public void SetWarningInfo(MessageInfo messageInfo)
        {
            AffectsOnIndicators = messageInfo.AffectsOnIndicators;
            IssueType = messageInfo.RelatedToIssue;

            _issueType = messageInfo.RelatedToIssue;
            _textArea.text = LocalizationUtils.GetTermOrText(messageInfo.MessageOrTerm);
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);

            if (!isActive) {
                LastDisabledTimeUtc = DateTime.UtcNow;
            }
        }

        public void Dispose()
        {
            Destroy(gameObject);
        }

        private void TriggerMessageClose()
        {
            CloseMessage?.Invoke(_issueType);
        }
    }
}
