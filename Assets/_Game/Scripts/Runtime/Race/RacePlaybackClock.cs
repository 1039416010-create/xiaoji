using System;

namespace GroundChickenKing.Race
{
    public sealed class RacePlaybackClock
    {
        private readonly float _hardTimeout;
        private RacePlan _plan;
        private float _elapsed;
        private float _plannedFinishTime;
        public RacePlaybackClock(float hardTimeout) { if (hardTimeout <= 0f) throw new ArgumentOutOfRangeException(nameof(hardTimeout)); _hardTimeout = hardTimeout; }
        public event Action<RaceResult> RaceCompleted;
        public bool IsComplete { get; private set; }
        public float Elapsed => _elapsed;
        public void Start(RacePlan plan)
        {
            _plan = plan ?? throw new ArgumentNullException(nameof(plan)); _elapsed = 0f; IsComplete = false;
            _plannedFinishTime = -1f;
            for (var i = 0; i < plan.Chickens.Count; i++) if (plan.Chickens[i].IsFinisher) { _plannedFinishTime = plan.Chickens[i].FinishTime; break; }
            if (_plannedFinishTime < 0f) throw new ArgumentException("Race plan has no finisher.", nameof(plan));
        }
        public bool Tick(float unscaledDeltaTime, bool presentationHealthy = true)
        {
            if (unscaledDeltaTime < 0f) throw new ArgumentOutOfRangeException(nameof(unscaledDeltaTime));
            if (_plan == null || IsComplete) return false;
            _elapsed += unscaledDeltaTime;
            if (presentationHealthy && _elapsed >= _plannedFinishTime) return Complete(_plannedFinishTime, RaceCompletionReason.PlannedFinish);
            if (_elapsed >= _hardTimeout) return Complete(_hardTimeout, RaceCompletionReason.HardTimeout);
            return false;
        }
        private bool Complete(float time, RaceCompletionReason reason)
        {
            IsComplete = true;
            RaceCompleted?.Invoke(new RaceResult(_plan.ChampionId, time, reason, _plan.PlanId));
            return true;
        }
    }
}
