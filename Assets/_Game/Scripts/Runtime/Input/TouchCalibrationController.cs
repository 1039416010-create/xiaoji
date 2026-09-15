using UnityEngine;
using UnityEngine.UI;

namespace GroundChickenKing.Input
{
    public sealed class TouchCalibrationController : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button[] _targets;
        [SerializeField] private Text _status;
        private bool[] _passed;

        public int PassedCount { get; private set; }

        public void Configure(GameObject panel, Button openButton, Button closeButton, Button[] targets, Text status)
        { _panel = panel; _openButton = openButton; _closeButton = closeButton; _targets = targets; _status = status; }

        private void Awake()
        {
            _passed = new bool[_targets.Length]; _openButton.onClick.AddListener(Open); _closeButton.onClick.AddListener(Close);
            for (var index = 0; index < _targets.Length; index++) { var captured = index; _targets[index].onClick.AddListener(() => MarkPassed(captured)); }
            _panel.SetActive(false);
        }

        public void Open()
        {
            PassedCount = 0; System.Array.Clear(_passed, 0, _passed.Length);
            foreach (var target in _targets) { target.interactable = true; SetColor(target, new Color(1f, .48f, .04f, 1f)); }
            _status.text = "依次触摸四角与中心的目标"; _panel.SetActive(true); _panel.transform.SetAsLastSibling();
        }

        public void Close() => _panel.SetActive(false);

        private void MarkPassed(int index)
        {
            if (_passed[index]) return; _passed[index] = true; PassedCount++; _targets[index].interactable = false; SetColor(_targets[index], new Color(.12f, .72f, .32f, 1f));
            _status.text = PassedCount == _targets.Length ? "五点触控覆盖通过 · 请记录设备与投影参数" : $"已通过 {PassedCount} / {_targets.Length}";
        }

        private static void SetColor(Button button, Color color) { button.GetComponent<Image>().color = color; }
    }
}
