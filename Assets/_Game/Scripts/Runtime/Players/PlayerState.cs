namespace GroundChickenKing.Players
{
    public sealed class PlayerState
    {
        public PlayerState(PlayerSeatDefinition definition)
        {
            Definition = definition;
        }

        public PlayerSeatDefinition Definition { get; }
        public bool IsJoined { get; private set; }
        public int Coins { get; private set; }

        public bool TryJoin(int initialCoins)
        {
            if (IsJoined || initialCoins < 0)
                return false;

            IsJoined = true;
            Coins = initialCoins;
            return true;
        }

        public bool TryLeave()
        {
            if (!IsJoined)
                return false;

            IsJoined = false;
            Coins = 0;
            return true;
        }

        public bool TryDebit(int amount, out int before, out int after)
        {
            before = Coins;
            after = Coins;
            if (!IsJoined || amount <= 0 || amount > Coins)
                return false;

            Coins -= amount;
            after = Coins;
            return true;
        }

        public bool TryCredit(int amount, out int before, out int after)
        {
            before = Coins; after = Coins;
            if (!IsJoined || amount <= 0 || Coins > int.MaxValue - amount) return false;
            Coins += amount; after = Coins; return true;
        }

        public bool TryResetCoins(int coins, out int before, out int after)
        {
            before = Coins; after = Coins;
            if (!IsJoined || coins < 0) return false;
            Coins = coins; after = Coins; return true;
        }

        public PlayerSnapshot ToSnapshot()
        {
            return new PlayerSnapshot(Definition, IsJoined, Coins);
        }
    }
}
