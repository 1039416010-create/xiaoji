using GroundChickenKing.Audio;
using GroundChickenKing.Persistence;
using UnityEngine;
using UnityEngine.UI;

namespace GroundChickenKing.UI
{
    public sealed class ExhibitSettingsController : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _open, _volumeDown, _volumeUp, _intensityDown, _intensityUp, _roundsDown, _roundsUp, _save, _back;
        [SerializeField] private Text _volumeValue, _intensityValue, _roundsValue;
        [SerializeField] private Image _dimmingOverlay;
        [SerializeField] private Button _fullscreenToggle;
        [SerializeField] private Text _fullscreenValue;
        [SerializeField] private GameSessionPresenter _session;
        [SerializeField] private ExhibitAudioController _audio;
        private readonly ExhibitSettingsService _service = new();
        private ExhibitSettings _settings;

        public void Configure(GameObject panel, Button open, Button volumeDown, Button volumeUp, Button intensityDown, Button intensityUp, Button roundsDown, Button roundsUp,
            Button save, Button back, Text volumeValue, Text intensityValue, Text roundsValue, Image dimmingOverlay, GameSessionPresenter session, ExhibitAudioController audio)
        {
            _panel = panel; _open = open; _volumeDown = volumeDown; _volumeUp = volumeUp; _intensityDown = intensityDown; _intensityUp = intensityUp;
            _roundsDown = roundsDown; _roundsUp = roundsUp; _save = save; _back = back; _volumeValue = volumeValue; _intensityValue = intensityValue;
            _roundsValue = roundsValue; _dimmingOverlay = dimmingOverlay; _session = session; _audio = audio;
        }

        public void ConfigureFullscreen(Button fullscreenToggle, Text fullscreenValue)
        {
            _fullscreenToggle = fullscreenToggle;
            _fullscreenValue = fullscreenValue;
        }

        private void Awake()
        {
            _settings = _service.Load(); Apply(); _panel.SetActive(false);
            _open.onClick.AddListener(Open); _back.onClick.AddListener(Close); _save.onClick.AddListener(Save);
            _volumeDown.onClick.AddListener(() => ChangeVolume(-0.1f)); _volumeUp.onClick.AddListener(() => ChangeVolume(0.1f));
            _intensityDown.onClick.AddListener(() => ChangeIntensity(-0.05f)); _intensityUp.onClick.AddListener(() => ChangeIntensity(0.05f));
            _roundsDown.onClick.AddListener(() => ChangeRounds(-1)); _roundsUp.onClick.AddListener(() => ChangeRounds(1));
            _fullscreenToggle?.onClick.AddListener(ToggleFullscreen);
        }
        private void Open() { _settings = _service.Load(); Apply(); _panel.SetActive(true); _panel.transform.SetAsLastSibling(); }
        private void Close() { _settings = _service.Load(); Apply(); _panel.SetActive(false); }
        private void Save() { _service.Save(_settings); Apply(); _panel.SetActive(false); }
        private void ChangeVolume(float delta) { _settings.MasterVolume += delta; _settings.Sanitize(); Apply(); }
        private void ChangeIntensity(float delta) { _settings.VisualIntensity += delta; _settings.Sanitize(); Apply(); }
        private void ChangeRounds(int delta) { _settings.MaximumRounds += delta; _settings.Sanitize(); Apply(); }
        private void ToggleFullscreen() { _settings.Fullscreen = !_settings.Fullscreen; Apply(); }
        private void Apply()
        {
            _settings.Sanitize(); _audio?.SetMasterVolume(_settings.MasterVolume); _session?.ApplyMaximumRounds(_settings.MaximumRounds);
            ApplyDisplayMode();
            if (_dimmingOverlay != null) { var color = _dimmingOverlay.color; color.a = (1f - _settings.VisualIntensity) * 0.65f; _dimmingOverlay.color = color; }
            _volumeValue.text = UiTextCatalog.Volume(_settings.MasterVolume); _intensityValue.text = UiTextCatalog.Intensity(_settings.VisualIntensity); _roundsValue.text = UiTextCatalog.MaximumRounds(_settings.MaximumRounds);
            if (_fullscreenValue != null) _fullscreenValue.text = _settings.Fullscreen ? "显示模式：全屏" : "显示模式：窗口";
        }

        private void ApplyDisplayMode()
        {
            var mode = _settings.Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            if (Screen.fullScreenMode == mode && Screen.fullScreen == _settings.Fullscreen) return;
            Screen.SetResolution(1920, 960, mode);
        }
    }
}
