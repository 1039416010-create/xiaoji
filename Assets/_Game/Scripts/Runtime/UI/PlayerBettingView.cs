using System;
using GroundChickenKing.Betting;
using GroundChickenKing.Players;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace GroundChickenKing.UI
{
    public sealed class PlayerBettingView : MonoBehaviour
    {
        [SerializeField] private PlayerSeat _seat;
        [SerializeField] private string[] _chickenIds;
        [SerializeField] private string[] _chickenNames;
        [SerializeField] private Button[] _chickenButtons;
        [SerializeField] private int[] _stakes;
        [SerializeField] private Button[] _stakeButtons;
        [SerializeField] private Button _lockButton;
        [SerializeField] private Text _selectionText;
        [SerializeField] private Text _balanceText;
        [SerializeField] private Text _lockButtonText;

        private Action<PlayerSeat, string> _chickenSelected;
        private Action<PlayerSeat, int> _stakeSelected;
        private Action<PlayerSeat> _lockRequested;

        public PlayerSeat Seat => _seat;
        public Button LockButton => _lockButton;

        public void Configure(
            PlayerSeat seat,
            string[] chickenIds,
            Button[] chickenButtons,
            int[] stakes,
            Button[] stakeButtons,
            Button lockButton,
            Text selectionText,
            Text balanceText,
            Text lockButtonText)
        {
            _seat = seat;
            _chickenIds = chickenIds;
            _chickenButtons = chickenButtons;
            _stakes = stakes;
            _stakeButtons = stakeButtons;
            _lockButton = lockButton;
            _selectionText = selectionText;
            _balanceText = balanceText;
            _lockButtonText = lockButtonText;
        }

        public void Bind(Action<PlayerSeat, string> chickenSelected, Action<PlayerSeat, int> stakeSelected, Action<PlayerSeat> lockRequested)
        {
            _chickenSelected = chickenSelected;
            _stakeSelected = stakeSelected;
            _lockRequested = lockRequested;

            for (var index = 0; index < _chickenButtons.Length; index++)
            {
                var capturedId = _chickenIds[index];
                _chickenButtons[index].onClick.RemoveAllListeners();
                _chickenButtons[index].onClick.AddListener(() => _chickenSelected?.Invoke(_seat, capturedId));
            }

            for (var index = 0; index < _stakeButtons.Length; index++)
            {
                var capturedStake = _stakes[index];
                _stakeButtons[index].onClick.RemoveAllListeners();
                _stakeButtons[index].onClick.AddListener(() => _stakeSelected?.Invoke(_seat, capturedStake));
            }

            _lockButton.onClick.RemoveAllListeners();
            _lockButton.onClick.AddListener(() => _lockRequested?.Invoke(_seat));
        }

        public void ApplyChickenRoster(IReadOnlyList<string> chickenIds, IReadOnlyList<string> chickenNames)
        {
            if (chickenIds == null || chickenNames == null || chickenIds.Count != _chickenButtons.Length || chickenNames.Count != _chickenButtons.Length)
                throw new ArgumentException("Betting roster must match the five chicken buttons.");
            _chickenIds = new string[chickenIds.Count]; _chickenNames = new string[chickenNames.Count];
            for (var index = 0; index < chickenIds.Count; index++)
            {
                _chickenIds[index] = chickenIds[index]; _chickenNames[index] = chickenNames[index];
                var label = _chickenButtons[index].GetComponentInChildren<Text>(true); if (label != null) label.text = $"{index + 1} · {_chickenNames[index]}";
            }
            if (_chickenSelected != null) Bind(_chickenSelected, _stakeSelected, _lockRequested);
        }

        public void Render(BetSnapshot bet, int balance)
        {
            _selectionText.text = UiTextCatalog.BetSelection(bet, DisplayChicken(bet.ChickenId));
            _balanceText.text = $"余额：{balance}";
            _lockButtonText.text = bet.IsLocked ? "已锁定" : "确认下注";

            for (var index = 0; index < _chickenButtons.Length; index++)
            {
                _chickenButtons[index].interactable = !bet.IsLocked;
                SetSelectedColor(_chickenButtons[index], string.Equals(_chickenIds[index], bet.ChickenId, StringComparison.Ordinal));
            }

            for (var index = 0; index < _stakeButtons.Length; index++)
            {
                _stakeButtons[index].interactable = !bet.IsLocked && _stakes[index] <= balance;
                SetSelectedColor(_stakeButtons[index], _stakes[index] == bet.Stake);
            }

            _lockButton.interactable = !bet.IsLocked && bet.HasChicken && bet.HasStake && bet.Stake <= balance;
        }

        private static void SetSelectedColor(Button button, bool isSelected)
        {
            var colors = button.colors;
            colors.normalColor = isSelected ? new Color(1f, 0.82f, 0.2f, 1f) : Color.white;
            button.colors = colors;
        }

        private string DisplayChicken(string chickenId)
        {
            if (string.IsNullOrWhiteSpace(chickenId)) return UiTextCatalog.ChickenName(null);
            var index = Array.IndexOf(_chickenIds, chickenId); return index >= 0 && _chickenNames != null && index < _chickenNames.Length ? _chickenNames[index] : UiTextCatalog.ChickenName(chickenId);
        }

    }
}
