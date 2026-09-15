using System;
using System.Collections.Generic;
using GroundChickenKing.Diagnostics;

namespace GroundChickenKing.Race
{
    public sealed class RaceDirector
    {
        private readonly IReadOnlyList<string> _roster;
        private readonly RaceRules _rules;
        private readonly IRacePlanFactory _generator;
        private readonly IRacePlanFactory _fallback;
        private readonly RacePlanValidator _validator;
        private bool _betsLocked;

        public RaceDirector(IReadOnlyList<string> roster, RaceRules rules, IRacePlanFactory generator = null, IRacePlanFactory fallback = null, RacePlanValidator validator = null)
        {
            if (roster == null || roster.Count != 5) throw new ArgumentException("Exactly five chickens are required.", nameof(roster));
            var copy = new string[roster.Count];
            var unique = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < copy.Length; i++) { copy[i] = roster[i]; if (string.IsNullOrWhiteSpace(copy[i]) || !unique.Add(copy[i])) throw new ArgumentException("Chicken IDs must be non-empty and unique.", nameof(roster)); }
            _roster = Array.AsReadOnly(copy); _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _generator = generator ?? new RacePlanGenerator(); _fallback = fallback ?? new FallbackRacePlanFactory(); _validator = validator ?? new RacePlanValidator();
        }

        public event Action<RacePlan> PlanGenerated;
        public RacePlan CurrentPlan { get; private set; }
        public bool AreBetsLocked => _betsLocked;

        public bool NotifyAllBetsLocked()
        {
            if (_betsLocked) return false;
            _betsLocked = true;
            return true;
        }

        public bool TryGeneratePlan(int round, RaceSeed seed, out RacePlan plan, out string error)
        {
            plan = null; error = null;
            if (!_betsLocked) { error = "All bets must be locked before generating a race plan."; return false; }
            if (CurrentPlan != null) { error = "A race plan has already been generated."; return false; }
            for (var attempt = 0; attempt < _rules.RetryLimit; attempt++)
            {
                var attemptSeed = new RaceSeed(unchecked(seed.Value + attempt * 7919));
                var candidate = _generator.Create(round, attemptSeed, _roster, _rules);
                if (!_validator.Validate(candidate, _rules).IsValid) continue;
                CurrentPlan = candidate; plan = candidate;
                PlanGenerated?.Invoke(candidate);
                return true;
            }
            var safePlan = _fallback.Create(round, seed, _roster, _rules);
            var validation = _validator.Validate(safePlan, _rules);
            if (!validation.IsValid) { error = "Fallback race plan failed validation."; return false; }
            GameLog.Error("Race", $"Generator exhausted {_rules.RetryLimit} attempts; validated fallback used. seed={seed.Value} config={_rules.ConfigHash}");
            CurrentPlan = safePlan; plan = safePlan;
            PlanGenerated?.Invoke(safePlan);
            return true;
        }
    }
}
