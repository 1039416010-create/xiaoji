using GroundChickenKing.UI;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace GroundChickenKing.Diagnostics
{
    public sealed class ExhibitDiagnosticsPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _summary;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameSessionPresenter _session;

        public void Configure(GameObject panel, Text summary, Button openButton, Button closeButton, GameSessionPresenter session)
        { _panel = panel; _summary = summary; _openButton = openButton; _closeButton = closeButton; _session = session; }

        private void Awake()
        {
            EnhancedTouchSupport.Enable(); _openButton.onClick.AddListener(Open); _closeButton.onClick.AddListener(Close); _panel.SetActive(false);
        }
        private void OnDestroy() { if (EnhancedTouchSupport.enabled) EnhancedTouchSupport.Disable(); }
        private void Update() { if (_panel.activeSelf) Refresh(); }
        public void Open() { _panel.SetActive(true); _panel.transform.SetAsLastSibling(); Refresh(); }
        public void Close() => _panel.SetActive(false);
        private void Refresh()
        {
            var seed = _session?.CurrentRacePlan?.Seed.Value.ToString() ?? UiTextCatalog.SeedUnavailable;
            _summary.text = UiTextCatalog.Diagnostics(BuildInfo.Summary, Screen.width, Screen.height, Touch.activeTouches.Count, seed, BoundedLogWriter.MostRecentError, BoundedLogWriter.LogPath);
        }
    }
}
