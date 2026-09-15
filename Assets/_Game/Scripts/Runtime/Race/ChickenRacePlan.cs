using System;
using System.Collections.Generic;

namespace GroundChickenKing.Race
{
    public sealed class ChickenRacePlan
    {
        public ChickenRacePlan(string chickenId, int laneIndex, bool isFinisher, float finishTime, float expectedEndProgress, IReadOnlyList<RaceSegment> segments, IReadOnlyList<RaceEvent> events)
        {
            ChickenId = chickenId ?? throw new ArgumentNullException(nameof(chickenId));
            LaneIndex = laneIndex; IsFinisher = isFinisher; FinishTime = finishTime; ExpectedEndProgress = expectedEndProgress;
            Segments = Array.AsReadOnly(Copy(segments)); Events = Array.AsReadOnly(Copy(events));
        }
        public string ChickenId { get; }
        public int LaneIndex { get; }
        public bool IsFinisher { get; }
        public float FinishTime { get; }
        public float ExpectedEndProgress { get; }
        public IReadOnlyList<RaceSegment> Segments { get; }
        public IReadOnlyList<RaceEvent> Events { get; }

        private static T[] Copy<T>(IReadOnlyList<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var copy = new T[source.Count];
            for (var i = 0; i < source.Count; i++) copy[i] = source[i];
            return copy;
        }
    }
}
