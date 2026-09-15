using GroundChickenKing.Players;

namespace GroundChickenKing.Betting
{
    public sealed class BetState
    {
        public BetState(PlayerSeat seat)
        {
            Seat = seat;
        }

        public PlayerSeat Seat { get; }
        public string ChickenId { get; private set; }
        public int Stake { get; private set; }
        public bool IsLocked { get; private set; }

        public void SelectChicken(string chickenId)
        {
            ChickenId = chickenId;
        }

        public void SelectStake(int stake)
        {
            Stake = stake;
        }

        public void Lock()
        {
            IsLocked = true;
        }

        public BetSnapshot ToSnapshot()
        {
            return new BetSnapshot(Seat, ChickenId, Stake, IsLocked);
        }
    }
}
