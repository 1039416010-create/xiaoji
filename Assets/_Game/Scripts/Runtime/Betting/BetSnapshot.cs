using GroundChickenKing.Players;

namespace GroundChickenKing.Betting
{
    public readonly struct BetSnapshot
    {
        public BetSnapshot(PlayerSeat seat, string chickenId, int stake, bool isLocked)
        {
            Seat = seat;
            ChickenId = chickenId;
            Stake = stake;
            IsLocked = isLocked;
        }

        public PlayerSeat Seat { get; }
        public string ChickenId { get; }
        public int Stake { get; }
        public bool IsLocked { get; }
        public bool HasChicken => !string.IsNullOrWhiteSpace(ChickenId);
        public bool HasStake => Stake > 0;
    }
}
