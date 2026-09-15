using System;

namespace GroundChickenKing.Race
{
    [Serializable]
    public readonly struct RaceEvent
    {
        public RaceEvent(RaceEventType type, float startTime, float endTime, float strength, int? targetLane)
        {
            Type = type; StartTime = startTime; EndTime = endTime; Strength = strength; TargetLane = targetLane;
        }
        public RaceEventType Type { get; }
        public float StartTime { get; }
        public float EndTime { get; }
        public float Strength { get; }
        public int? TargetLane { get; }
    }
}
