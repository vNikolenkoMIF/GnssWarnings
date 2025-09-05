using System;
using System.Collections.Generic;
using System.Linq;
using Assets._Project.Components.GnssMonitoring.Mobile.Scripts.UI;
using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Contracts;
using Assets._Project.Components.GnssMonitoring.Shared.Scripts.Messages;
using Assets._vGISComponents.Core.Shared.Scripts.Utilities;
using Meemim.VGis.Unity.Plugins.Interfaces.Log;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets._Project.Components.GnssMonitoring.Mobile.Scripts.Messages
{
    public class GnssWarningsManager : MonoSingleton<GnssWarningsManager>, IGnssIssuesNotifier, 
        IWarningMessagesViewport
    {
        private const int RESTORE_AFTER_CLOSE_SEC = 30;
        
        [SerializeField]
        private MessageInfo[] _messages;

        [SerializeField]
        private WarningMessage _messagePrefab;

        [SerializeField]
        private Transform _container;

        [SerializeField]
        private ContentTransform _contentTransform;

        public bool IsViewportLocked { get; set; }

        public event Action<IEnumerable<AffectedIndicator>> IndicatorsAffected;

        private ILog _log;
        
        private Dictionary<GnssIssue, MessageInfo> _supportedMessages;
        
        private Dictionary<GnssIssue, IWarningMessage> _onScreenMessages = new Dictionary<GnssIssue, IWarningMessage>();
        private HashSet<IWarningMessage> _disabledMessages = new HashSet<IWarningMessage>(new MessagesEqualityComparer());

        protected override void Awake()
        {
            base.Awake();

            Assert.IsNotNull(_messages, "There is no configured messages, please add at least one");
            Assert.IsNotNull(_messagePrefab);
            Assert.IsNotNull(_container);
            Assert.IsNotNull(_contentTransform);

            _log = LogManager.GetLogger(nameof(GnssWarningsManager));
            
            _supportedMessages = _messages.ToDictionary(x => x.RelatedToIssue, x => x);
        }

        public void NotifyAboutIssue(GnssIssue issue)
        {
            if (_onScreenMessages.ContainsKey(issue)) {
                return;
            }

            var message = CreateMessage(issue);
            message.CloseMessage += DisableMessage;
            _onScreenMessages.Add(issue, message);
            UpdateIndicatorsState();
        }
        
        public void StopNotify(GnssIssue issueType)
        {
            if (!_onScreenMessages.ContainsKey(issueType)) {
                _log.LogError($"Can't delete message for issue {issueType} because it doesn't exists");
                return;
            }

            var message = _onScreenMessages[issueType];
            message.CloseMessage -= DisableMessage;
            message.Dispose();

            _onScreenMessages.Remove(issueType);
            
            _disabledMessages.Remove(message);
            StopCountdownIfEmpty();
            UpdateIndicatorsState();
        }

        public bool IsMessageActive(GnssIssue gnssIssue)
        {
            return _onScreenMessages.ContainsKey(gnssIssue);
        }

        public void ShowMessagesRoot()
        {
            _container.gameObject.SetActive(true);
        }
        
        public void HideMessagesRoot()
        {
            _container.gameObject.SetActive(false);
        }

        public void SetNotifierActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void ClearAllMessages()
        {
            foreach (var message in _onScreenMessages.Values)
            {
                message.Dispose();
            }
            
            _onScreenMessages.Clear();
            
            _disabledMessages.Clear();
            StopCountdown();
            
            IndicatorsAffected?.Invoke(Array.Empty<AffectedIndicator>());
        }

        public void SnapMessagesViewportTo(RectTransform targetRect, Place messagesPlace, float offset)
        {
            if (IsViewportLocked) {
                return;
            }

            _contentTransform.SnapToRect(targetRect, messagesPlace, offset);
        }

        private IWarningMessage CreateMessage(GnssIssue issueType)
        {
            if (!_supportedMessages.ContainsKey(issueType)) {
                _log.LogError($"Message for issue {issueType} was not configured.");
                return null;
            }
            
            //Will be better to use an object pooling here
            // But I decided to skip it for now because I switch off a message 
            // when a user taps on close button.
            var messageInstance = Instantiate(_messagePrefab, _container, false);
            messageInstance.SetWarningInfo(_supportedMessages[issueType]);

            return messageInstance;
        }

        private void UpdateIndicatorsState()
        {
            var shouldAffectOnIndicators = _onScreenMessages.Values
                .SelectMany(m => m.AffectsOnIndicators)
                .Distinct();
            
            IndicatorsAffected?.Invoke(shouldAffectOnIndicators);
        }

        private void DisableMessage(GnssIssue issueType)
        {
            var disabledMessage = _onScreenMessages[issueType];
            disabledMessage.SetActive(false);
            _disabledMessages.Add(disabledMessage);
            
            RunCountdownToEnable();
        }


        private void RunCountdownToEnable()
        {
            if (!IsInvoking(nameof(CheckEverySecondToEnable))) {
                InvokeRepeating(nameof(CheckEverySecondToEnable), 0, 1);
            }
        }

        private void CheckEverySecondToEnable()
        {
            var messagesToEnable = _disabledMessages
                .Where(m => (DateTime.UtcNow - m.LastDisabledTimeUtc.Value).Seconds > RESTORE_AFTER_CLOSE_SEC);

            foreach (var message in messagesToEnable)
            {
                message.SetActive(true);
            }
            
            _disabledMessages
                .RemoveWhere(m => (DateTime.UtcNow - m.LastDisabledTimeUtc.Value).Seconds > RESTORE_AFTER_CLOSE_SEC);

            StopCountdownIfEmpty();
        }

        private void StopCountdownIfEmpty()
        {
            if (_disabledMessages.Count == 0) {
                StopCountdown();
            }
        }

        private void StopCountdown()
        {
            CancelInvoke(nameof(CheckEverySecondToEnable));
        }
    }
}
