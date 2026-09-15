using System;
using UnityEngine;
using UnityEngine.UI;

namespace GroundChickenKing.UI
{
    public sealed class ConfirmationDialog : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _message;
        [SerializeField] private Text _mirroredMessage;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;
        private Action _pending;

        public bool IsOpen => _panel != null && _panel.activeSelf;

        public void Configure(GameObject panel, Text message, Text mirroredMessage, Button confirmButton, Button cancelButton)
        {
            _panel = panel; _message = message; _mirroredMessage = mirroredMessage; _confirmButton = confirmButton; _cancelButton = cancelButton;
        }

        private void Awake()
        {
            _confirmButton.onClick.AddListener(Confirm);
            _cancelButton.onClick.AddListener(Cancel);
        }

        public void Show(string message, Action confirmed)
        {
            if (confirmed == null) throw new ArgumentNullException(nameof(confirmed));
            _pending = confirmed; _message.text = message; if (_mirroredMessage != null) _mirroredMessage.text = message;
            _panel.SetActive(true); _panel.transform.SetAsLastSibling();
        }

        public void Confirm()
        {
            if (!IsOpen) return;
            var action = _pending; _pending = null; _panel.SetActive(false); action?.Invoke();
        }

        public void Cancel()
        {
            _pending = null; if (_panel != null) _panel.SetActive(false);
        }
    }
}
