namespace GroundChickenKing.Flow
{
    public readonly struct GameFlowTransition
    {
        public GameFlowTransition(GameFlowState from, GameFlowState to, string reason)
        {
            From = from;
            To = to;
            Reason = reason;
        }

        public GameFlowState From { get; }
        public GameFlowState To { get; }
        public string Reason { get; }
    }
}
