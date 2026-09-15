namespace GroundChickenKing.Race
{
    public readonly struct RaceResult
    {
        public RaceResult(string championId, float finishTime, RaceCompletionReason reason, string planId)
        { ChampionId = championId; FinishTime = finishTime; Reason = reason; PlanId = planId; }
        public string ChampionId { get; }
        public float FinishTime { get; }
        public RaceCompletionReason Reason { get; }
        public string PlanId { get; }
    }
}
