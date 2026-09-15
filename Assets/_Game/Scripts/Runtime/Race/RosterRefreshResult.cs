using System;
using System.Collections.Generic;

namespace GroundChickenKing.Race
{
    public sealed class RosterRefreshResult
    {
        public RosterRefreshResult(string championId, IReadOnlyList<string> previous, IReadOnlyList<string> current)
        {
            ChampionId = championId; PreviousRoster = Copy(previous); CurrentRoster = Copy(current);
        }
        public string ChampionId { get; }
        public IReadOnlyList<string> PreviousRoster { get; }
        public IReadOnlyList<string> CurrentRoster { get; }
        private static IReadOnlyList<string> Copy(IReadOnlyList<string> source) { var copy = new string[source.Count]; for (var i = 0; i < copy.Length; i++) copy[i] = source[i]; return Array.AsReadOnly(copy); }
    }
}
