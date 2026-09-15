using System;
using System.Collections.Generic;

namespace GroundChickenKing.Race
{
    public sealed class RacePlanValidator
    {
        public RacePlanValidationResult Validate(RacePlan plan, RaceRules rules)
        {
            var errors = new List<string>();
            if (plan == null) { errors.Add("Plan is null."); return new RacePlanValidationResult(errors); }
            if (plan.Round <= 0) errors.Add("Round must be positive.");
            if (plan.ConfigHash != rules.ConfigHash) errors.Add("Config hash mismatch.");
            if (plan.TotalDuration < rules.MinimumDuration || plan.TotalDuration > rules.MaximumDuration || plan.TotalDuration > rules.HardTimeout)
                errors.Add("Total duration is outside configured limits.");
            if (plan.Chickens.Count != rules.LaneCount) errors.Add("Plan must contain five chickens.");

            var ids = new HashSet<string>(StringComparer.Ordinal);
            var lanes = new HashSet<int>();
            var finishers = 0;
            foreach (var chicken in plan.Chickens)
            {
                if (chicken == null) { errors.Add("Chicken plan is null."); continue; }
                if (!ids.Add(chicken.ChickenId)) errors.Add("Chicken IDs must be unique.");
                if (chicken.LaneIndex < 0 || chicken.LaneIndex >= rules.LaneCount || !lanes.Add(chicken.LaneIndex)) errors.Add("Lanes must be unique and in range.");
                if (chicken.IsFinisher)
                {
                    finishers++;
                    if (chicken.ChickenId != plan.ChampionId) errors.Add("Finisher does not match champion.");
                    if (Math.Abs(chicken.ExpectedEndProgress - rules.FinishProgress) > rules.FinishTolerance) errors.Add("Champion does not reach finish.");
                    if (chicken.FinishTime < 0f || chicken.FinishTime > plan.TotalDuration) errors.Add("Champion finish time is invalid.");
                }
                else if (chicken.ExpectedEndProgress >= rules.FinishProgress - rules.FinishTolerance)
                    errors.Add("Non-champion reaches finish boundary.");

                ValidateSegments(chicken, plan.TotalDuration, rules.StartProgress, errors);
                ValidateEvents(chicken, plan.TotalDuration, rules.LaneCount, rules.MinimumEventInterval, errors);
            }
            if (finishers != 1) errors.Add("Exactly one finisher is required.");
            if (!ids.Contains(plan.ChampionId)) errors.Add("Champion is not in roster.");
            return new RacePlanValidationResult(errors);
        }

        private static void ValidateSegments(ChickenRacePlan chicken, float duration, float configuredStartProgress, ICollection<string> errors)
        {
            if (chicken.Segments.Count == 0) { errors.Add("Each chicken needs movement segments."); return; }
            var previousEnd = 0f;
            var previousProgress = configuredStartProgress;
            for (var i = 0; i < chicken.Segments.Count; i++)
            {
                var segment = chicken.Segments[i];
                if (segment.StartTime < 0f || segment.EndTime <= segment.StartTime || segment.EndTime > duration) errors.Add("Segment time is invalid.");
                if (Math.Abs(segment.StartTime - previousEnd) > 0.001f) errors.Add("Segment times are not contiguous from zero.");
                if (Math.Abs(segment.StartProgress - previousProgress) > 0.001f) errors.Add("Segment progress is not contiguous from configured start.");
                if (segment.StartProgress < 0f || segment.StartProgress > 1f || segment.EndProgress < 0f || segment.EndProgress > 1f) errors.Add("Segment progress is outside [0,1].");
                previousEnd = segment.EndTime;
                previousProgress = segment.EndProgress;
            }
            if (Math.Abs(previousEnd - duration) > 0.001f) errors.Add("Segments do not cover total duration.");
            if (Math.Abs(chicken.Segments[chicken.Segments.Count - 1].EndProgress - chicken.ExpectedEndProgress) > 0.001f) errors.Add("Expected end progress mismatch.");
        }

        private static void ValidateEvents(ChickenRacePlan chicken, float duration, int laneCount, float minimumEventInterval, ICollection<string> errors)
        {
            var previousStart = -1f;
            foreach (var raceEvent in chicken.Events)
            {
                if (raceEvent.StartTime < 0f || raceEvent.EndTime < raceEvent.StartTime || raceEvent.EndTime > duration || raceEvent.StartTime < previousStart)
                    errors.Add("Race event time is invalid or unsorted.");
                if (previousStart >= 0f && raceEvent.StartTime - previousStart < minimumEventInterval)
                    errors.Add("Race events violate the minimum interval.");
                if (raceEvent.TargetLane.HasValue && (raceEvent.TargetLane < 0 || raceEvent.TargetLane >= laneCount || Math.Abs(raceEvent.TargetLane.Value - chicken.LaneIndex) != 1))
                    errors.Add("Event target lane must be adjacent and in range.");
                previousStart = raceEvent.StartTime;
            }
        }
    }
}
