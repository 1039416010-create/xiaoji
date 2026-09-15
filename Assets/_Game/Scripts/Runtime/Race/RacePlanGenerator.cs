using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GroundChickenKing.Race
{
    public sealed class RacePlanGenerator : IRacePlanFactory
    {
        public const string PlanVersion = "race-plan-v1";

        public RacePlan Create(int round, RaceSeed seed, IReadOnlyList<string> chickenIds, RaceRules rules)
        {
            if (round <= 0) throw new ArgumentOutOfRangeException(nameof(round));
            if (chickenIds == null || chickenIds.Count != rules.LaneCount) throw new ArgumentException("Exactly five chickens are required.", nameof(chickenIds));
            var random = new SeededRandomSource(seed);
            var championLane = random.NextInt(0, chickenIds.Count);
            var duration = random.NextFloat(rules.MinimumDuration, rules.MaximumDuration);
            var plans = new ChickenRacePlan[chickenIds.Count];
            for (var lane = 0; lane < chickenIds.Count; lane++)
            {
                var isChampion = lane == championLane;
                var middleOne = random.NextFloat(0.18f, 0.38f);
                var middleTwo = random.NextFloat(0.48f, 0.74f);
                var end = isChampion ? rules.FinishProgress : random.NextFloat(0.78f, rules.FinishProgress - rules.FinishTolerance - 0.01f);
                var t1 = duration * random.NextFloat(0.25f, 0.35f);
                var t2 = duration * random.NextFloat(0.58f, 0.72f);
                var segments = new[]
                {
                    new RaceSegment(0f, t1, rules.StartProgress, middleOne, RaceInterpolation.EaseInOut, true),
                    new RaceSegment(t1, t2, middleOne, middleTwo, RaceInterpolation.Linear, true),
                    new RaceSegment(t2, duration, middleTwo, end, RaceInterpolation.EaseInOut, true),
                };
                var eventStart = duration * 0.22f;
                var eventEnd = eventStart + Math.Max(0.35f, rules.MinimumEventInterval * 0.75f);
                var type = (RaceEventType)lane;
                RaceEvent[] events;
                if (lane == 0)
                {
                    var interfereStart = duration * 0.62f;
                    events = new[]
                    {
                        new RaceEvent(type, eventStart, eventEnd, random.NextFloat(0.2f, 1f), null),
                        new RaceEvent(RaceEventType.Interfere, interfereStart, interfereStart + 0.45f, random.NextFloat(0.2f, 1f), 1),
                    };
                }
                else
                {
                    events = new[] { new RaceEvent(type, eventStart, eventEnd, random.NextFloat(0.2f, 1f), null) };
                }
                plans[lane] = new ChickenRacePlan(chickenIds[lane], lane, isChampion, isChampion ? duration : -1f, end, segments, events);
            }

            var champion = chickenIds[championLane];
            var identity = new StringBuilder().Append(PlanVersion).Append('|').Append(rules.ConfigHash).Append('|').Append(round).Append('|')
                .Append(seed.Value).Append('|').Append(champion).Append('|').Append(duration.ToString("R", CultureInfo.InvariantCulture));
            foreach (var chicken in plans)
            {
                identity.Append('|').Append(chicken.ChickenId).Append(':').Append(chicken.LaneIndex).Append(':').Append(chicken.IsFinisher)
                    .Append(':').Append(chicken.ExpectedEndProgress.ToString("R", CultureInfo.InvariantCulture));
                foreach (var segment in chicken.Segments)
                    identity.Append(':').Append(segment.StartTime.ToString("R", CultureInfo.InvariantCulture)).Append(',').Append(segment.EndTime.ToString("R", CultureInfo.InvariantCulture))
                        .Append(',').Append(segment.StartProgress.ToString("R", CultureInfo.InvariantCulture)).Append(',').Append(segment.EndProgress.ToString("R", CultureInfo.InvariantCulture));
                foreach (var raceEvent in chicken.Events)
                    identity.Append(':').Append((int)raceEvent.Type).Append(',').Append(raceEvent.StartTime.ToString("R", CultureInfo.InvariantCulture))
                        .Append(',').Append(raceEvent.EndTime.ToString("R", CultureInfo.InvariantCulture)).Append(',').Append(raceEvent.Strength.ToString("R", CultureInfo.InvariantCulture));
            }
            return new RacePlan(PlanVersion, rules.ConfigHash, round, seed, champion, duration, plans, RaceRules.StableHash(identity.ToString()));
        }
    }
}
