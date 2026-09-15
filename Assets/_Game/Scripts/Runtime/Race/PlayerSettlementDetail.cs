using GroundChickenKing.Players;

namespace GroundChickenKing.Race
{
    public readonly struct PlayerSettlementDetail
    {
        public PlayerSettlementDetail(PlayerSeat seat, string chickenId, int stake, bool didWin, int returnAmount, int balanceAfter)
        { Seat = seat; ChickenId = chickenId; Stake = stake; DidWin = didWin; ReturnAmount = returnAmount; BalanceAfter = balanceAfter; }
        public PlayerSeat Seat { get; }
        public string ChickenId { get; }
        public int Stake { get; }
        public bool DidWin { get; }
        public int ReturnAmount { get; }
        public int BalanceAfter { get; }
    }
}
