using System;
using System.Collections.Generic;

namespace GroundChickenKing.Race
{
    public sealed class SettlementRecord
    {
        public SettlementRecord(int round, string planId, string championId, IReadOnlyList<PlayerSettlementDetail> details)
        {
            Round = round; PlanId = planId; ChampionId = championId;
            var copy = new PlayerSettlementDetail[details.Count]; for (var i = 0; i < copy.Length; i++) copy[i] = details[i];
            Details = Array.AsReadOnly(copy);
        }
        public int Round { get; }
        public string PlanId { get; }
        public string ChampionId { get; }
        public IReadOnlyList<PlayerSettlementDetail> Details { get; }
    }
}
