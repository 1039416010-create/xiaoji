using System;
using GroundChickenKing.Players;
using UnityEngine;
using UnityEngine.UI;

namespace GroundChickenKing.UI
{
    public sealed class PlayerSeatView : MonoBehaviour
    {
        [SerializeField] private PlayerSeat _seat;
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _statusText;
        [SerializeField] private Text _coinsText;
        [SerializeField] private Text _joinButtonText;
        [SerializeField] private Button _joinButton;

        private Action<PlayerSeat> _joinRequested;

        public PlayerSeat Seat => _seat;
        public Button JoinButton => _joinButton;

        public void Configure(PlayerSeat seat, Text titleText, Text statusText, Text coinsText, Text joinButtonText, Button joinButton)
        {
            _seat = seat;
            _titleText = titleText;
            _statusText = statusText;
            _coinsText = coinsText;
            _joinButtonText = joinButtonText;
            _joinButton = joinButton;
        }

        public void Bind(Action<PlayerSeat> joinRequested)
        {
            _joinRequested = joinRequested;
            _joinButton.onClick.RemoveListener(HandleJoinClicked);
            _joinButton.onClick.AddListener(HandleJoinClicked);
        }

        public void Render(PlayerSnapshot snapshot)
        {
            _titleText.text = $"{snapshot.Definition.Icon} {snapshot.Definition.DisplayName}";
            _statusText.text = snapshot.IsJoined ? "准备完成" : "等待加入";
            _coinsText.text = snapshot.IsJoined ? $"金币：{snapshot.Coins}" : "初始金币：50";
            _joinButtonText.text = snapshot.IsJoined ? "已加入" : "点击加入";
            _joinButton.interactable = !snapshot.IsJoined;
        }

        private void HandleJoinClicked()
        {
            _joinRequested?.Invoke(_seat);
        }
    }
}
