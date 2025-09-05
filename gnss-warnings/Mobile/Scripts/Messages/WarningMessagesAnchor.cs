using System.Linq;
using Assets._Project.Components.GnssMonitoring.Mobile.Scripts.UI;
using UnityEngine;

namespace Assets._Project.Components.GnssMonitoring.Mobile.Scripts.Messages
{
    [RequireComponent(typeof(RectTransform))]
    public class WarningMessagesAnchor : MonoBehaviour
    {
        [field: SerializeField] 
        public float Offset { get; set; }
        
        [field: SerializeField] 
        public Place Place { get; set; }

        [field: SerializeField] 
        public bool IsLocked { get; set; } = false;
        
        private IWarningMessagesViewport _warningMessagesViewport;
        
        private void Awake()
        {
            _warningMessagesViewport = GnssWarningsManager.Instance;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            var root = gameObject.transform.root;
            var anchors = root.GetComponentsInChildren<WarningMessagesAnchor>();

            if (anchors.Length > 1 && anchors.Count(a => a.gameObject.activeSelf) > 1)
            {
                Debug.LogWarning($"{nameof(WarningMessagesAnchor)}: Two or more components active at the same time. " +
                                 $"Possibly it will cause unpredictable position of the viewport, are you sure?");
            }
        }
 #endif       

        private void OnEnable()
        {
            _warningMessagesViewport.SnapMessagesViewportTo(transform as RectTransform, Place, Offset);
            _warningMessagesViewport.IsViewportLocked = IsLocked;
        }

        private void OnDisable()
        {
            _warningMessagesViewport.IsViewportLocked = false;
        }
    }
}
