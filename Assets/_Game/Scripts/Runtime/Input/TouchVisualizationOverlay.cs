using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace GroundChickenKing.Input
{
    public sealed class TouchVisualizationOverlay : MonoBehaviour
    {
        [SerializeField] private RectTransform _canvasRoot;
        [SerializeField] private Button _toggleButton;
        [SerializeField] private Text _toggleLabel;
        private RectTransform[] _markers;
        private bool _isVisible;

        public void Configure(RectTransform canvasRoot, Button toggleButton, Text toggleLabel)
        { _canvasRoot = canvasRoot; _toggleButton = toggleButton; _toggleLabel = toggleLabel; }

        private void Awake()
        {
            _markers = new RectTransform[10];
            for (var i = 0; i < _markers.Length; i++)
            {
                var marker = new GameObject($"UI_Image_TouchMarker_{i + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); marker.transform.SetParent(_canvasRoot, false);
                var rect = marker.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.sizeDelta = new Vector2(70, 70);
                var image = marker.GetComponent<Image>(); image.color = new Color(0f, 1f, .72f, .7f); image.raycastTarget = false; marker.SetActive(false); _markers[i] = rect;
            }
            _toggleButton.onClick.AddListener(Toggle);
            if (!Debug.isDebugBuild) { _toggleButton.interactable = false; _toggleLabel.text = "触点显示（开发版）"; }
        }

        private void Update()
        {
            if (!_isVisible) return;
            var touches = Touch.activeTouches;
            for (var i = 0; i < _markers.Length; i++)
            {
                var active = i < touches.Count; _markers[i].gameObject.SetActive(active);
                if (!active) continue;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRoot, touches[i].screenPosition, null, out var local); _markers[i].anchoredPosition = local;
            }
        }

        private void Toggle()
        {
            if (!Debug.isDebugBuild) return; _isVisible = !_isVisible; _toggleLabel.text = _isVisible ? "关闭触点显示" : "开启触点显示";
            if (!_isVisible) foreach (var marker in _markers) marker.gameObject.SetActive(false);
        }
    }
}
