namespace GroundChickenKing.Players
{
    public readonly struct BalanceTransaction
    {
        public BalanceTransaction(PlayerSeat seat, int before, int delta, int after, int round, BalanceChangeReason reason)
        {
            Seat = seat;
            Before = before;
            Delta = delta;
            After = after;
            Round = round;
            Reason = reason;
        }

        public PlayerSeat Seat { get; }
        public int Before { get; }
        public int Delta { get; }
        public int After { get; }
        public int Round { get; }
        public BalanceChangeReason Reason { get; }
    }
}
