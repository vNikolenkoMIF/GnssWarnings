using System;
using Assets._vGISComponents.Core.Shared.Scripts.Extensions;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets._Project.Components.GnssMonitoring.Mobile.Scripts.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class ContentTransform : MonoBehaviour
    {
        [SerializeField]
        private Canvas _parentCanvas;

        private RectTransform _rect;
        private RectTransform _canvasRect;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();

            Assert.IsNotNull(_parentCanvas);
            _canvasRect = _parentCanvas.transform as RectTransform;
        }

        public void SetPositionInResolutionCoordinates(Vector2 position)
        {
            position = ClampToRectSize(position);
            _rect.anchoredPosition = position;
        }

        public void SnapToRect(RectTransform target, Place place, float offset = 0)
        {
            Vector2 anchoredPosition;

            switch (place) {
                case Place.BottomLeft:
                    SetLeftTopAnchors();
                    var targetWorldSpaceRect = target.GetRectSizeAndLeftBottomPoint();
                    anchoredPosition = RectTransformExtensions.GetAnchoredPosition(targetWorldSpaceRect.position, _parentCanvas);
                    anchoredPosition.y -= _canvasRect.rect.size.y + offset;
                    break;
                case Place.TopCenter:
                    SetBottomCenterAnchors();
                    var corners = new Vector3[4];
                    target.GetWorldCorners(corners);
                    // Get point between top left and top right point
                    var position = (corners[1] + corners[2]) / 2;

                    anchoredPosition = RectTransformExtensions.GetAnchoredPosition(position, _parentCanvas);
                    anchoredPosition.x = 0;
                    anchoredPosition.y += offset;

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(place), place, null);
            }

            _rect.anchoredPosition = ClampToRectSize(anchoredPosition);
        }

        private Vector2 ClampToRectSize(Vector2 position)
        {
            var rectSize = _canvasRect.rect.size;

            var x = Mathf.Clamp(position.x, -rectSize.x, rectSize.x);
            var y = Mathf.Clamp(position.y, -rectSize.y, rectSize.y);
            return new Vector2(x, y);
        }

        private void SetLeftTopAnchors()
        {
            // Left top side config
            // ⦿‾‾‾‾‾‾‾‾‾|
            // |          |
            // |          |
            // |          |
            //  ‾‾‾‾‾‾‾‾‾‾
            _rect.pivot = Vector2.up;
            _rect.anchorMin = Vector2.up;
            _rect.anchorMax = Vector2.up;
        }

        private void SetBottomCenterAnchors()
        {
            // Bottom center config
            // |‾‾‾‾‾‾‾‾‾‾|
            // |          |
            // |          |
            // |          |
            //  ‾‾‾‾⦿‾‾‾‾‾
            var halfOne = new Vector2(0.5f, 0);

            _rect.pivot = halfOne;
            _rect.anchorMin = halfOne;
            _rect.anchorMax = halfOne;
        }
    }
}
