using System;
using System.Collections.Generic;

namespace GroundChickenKing.Race
{
    public sealed class RacePlan
    {
        public RacePlan(string version, string configHash, int round, RaceSeed seed, string championId, float totalDuration, IReadOnlyList<ChickenRacePlan> chickens, string planId)
        {
            Version = version ?? throw new ArgumentNullException(nameof(version)); ConfigHash = configHash ?? throw new ArgumentNullException(nameof(configHash));
            Round = round; Seed = seed; ChampionId = championId ?? throw new ArgumentNullException(nameof(championId)); TotalDuration = totalDuration;
            var copy = new ChickenRacePlan[chickens?.Count ?? throw new ArgumentNullException(nameof(chickens))];
            for (var i = 0; i < copy.Length; i++) copy[i] = chickens[i];
            Chickens = Array.AsReadOnly(copy); PlanId = planId ?? throw new ArgumentNullException(nameof(planId));
        }
        public string Version { get; }
        public string ConfigHash { get; }
        public int Round { get; }
        public RaceSeed Seed { get; }
        public string ChampionId { get; }
        public float TotalDuration { get; }
        public IReadOnlyList<ChickenRacePlan> Chickens { get; }
        public string PlanId { get; }
    }
}
