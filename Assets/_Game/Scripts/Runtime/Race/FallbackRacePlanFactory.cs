using System.Collections.Generic;

namespace GroundChickenKing.Race
{
    public sealed class FallbackRacePlanFactory : IRacePlanFactory
    {
        public RacePlan Create(int round, RaceSeed seed, IReadOnlyList<string> chickenIds, RaceRules rules)
        {
            var duration = rules.MinimumDuration;
            var plans = new ChickenRacePlan[chickenIds.Count];
            var championLane = PositiveModulo(seed.Value, chickenIds.Count);
            for (var lane = 0; lane < chickenIds.Count; lane++)
            {
                var wins = lane == championLane;
                var end = wins ? rules.FinishProgress : rules.FinishProgress - rules.FinishTolerance - 0.05f;
                plans[lane] = new ChickenRacePlan(chickenIds[lane], lane, wins, wins ? duration : -1f, end,
                    new[] { new RaceSegment(0f, duration, rules.StartProgress, end, RaceInterpolation.Linear, true) },
                    new RaceEvent[0]);
            }
            var champion = chickenIds[championLane];
            return new RacePlan(RacePlanGenerator.PlanVersion, rules.ConfigHash, round, seed, champion, duration, plans,
                RaceRules.StableHash($"fallback|{rules.ConfigHash}|{round}|{seed.Value}|{string.Join(",", chickenIds)}|{champion}"));
        }

        private static int PositiveModulo(int value, int modulus)
        {
            var result = value % modulus;
            return result < 0 ? result + modulus : result;
        }
    }
}
