using System;

namespace GroundChickenKing.Race
{
    public sealed class RaceCountdownClock
    {
        private float _elapsed;
        public RaceCountdownClock(float durationSeconds = 4f) { if (durationSeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(durationSeconds)); DurationSeconds = durationSeconds; }
        public float DurationSeconds { get; }
        public bool IsRunning { get; private set; }
        public bool IsComplete { get; private set; }
        public string DisplayText => !IsRunning && !IsComplete ? string.Empty : _elapsed < 1f ? "3" : _elapsed < 2f ? "2" : _elapsed < 3f ? "1" : "GO!";
        public void Start(RacePlan lockedPlan) { if (lockedPlan == null) throw new ArgumentNullException(nameof(lockedPlan)); _elapsed = 0f; IsRunning = true; IsComplete = false; }
        public bool Tick(float unscaledDeltaTime)
        {
            if (unscaledDeltaTime < 0f) throw new ArgumentOutOfRangeException(nameof(unscaledDeltaTime));
            if (!IsRunning) return false;
            _elapsed += unscaledDeltaTime;
            if (_elapsed < DurationSeconds) return false;
            IsRunning = false; IsComplete = true; return true;
        }
    }
}
