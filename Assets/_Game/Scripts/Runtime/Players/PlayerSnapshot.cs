namespace GroundChickenKing.Players
{
    public readonly struct PlayerSnapshot
    {
        public PlayerSnapshot(PlayerSeatDefinition definition, bool isJoined, int coins)
        {
            Definition = definition;
            IsJoined = isJoined;
            Coins = coins;
        }

        public PlayerSeatDefinition Definition { get; }
        public bool IsJoined { get; }
        public int Coins { get; }
        public bool IsBankrupt => IsJoined && Coins == 0;
    }
}
