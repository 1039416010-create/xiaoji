using System;

namespace GroundChickenKing.Race
{
    [Serializable]
    public readonly struct RaceSegment
    {
        public RaceSegment(float startTime, float endTime, float startProgress, float endProgress, RaceInterpolation interpolation, bool isFacingForward)
        {
            StartTime = startTime; EndTime = endTime; StartProgress = startProgress; EndProgress = endProgress;
            Interpolation = interpolation; IsFacingForward = isFacingForward;
        }
        public float StartTime { get; }
        public float EndTime { get; }
        public float StartProgress { get; }
        public float EndProgress { get; }
        public RaceInterpolation Interpolation { get; }
        public bool IsFacingForward { get; }
    }
}
