using System;
using System.Collections.Generic;

namespace GroundChickenKing.Race
{
    public sealed class RosterService
    {
        private readonly string[] _catalog;
        private string[] _current;
        public RosterService(IReadOnlyList<string> catalog, IReadOnlyList<string> initialRoster)
        {
            if (catalog == null || catalog.Count < 9) throw new ArgumentException("Roster catalog requires at least nine unique chickens.", nameof(catalog));
            _catalog = CopyUnique(catalog, "Catalog");
            if (initialRoster == null || initialRoster.Count != 5) throw new ArgumentException("Initial roster requires five chickens.", nameof(initialRoster));
            _current = CopyUnique(initialRoster, "Roster");
            var available = new HashSet<string>(_catalog, StringComparer.Ordinal); foreach (var id in _current) if (!available.Contains(id)) throw new ArgumentException("Initial roster must belong to catalog.", nameof(initialRoster));
        }
        public IReadOnlyList<string> CurrentRoster => Array.AsReadOnly((string[])_current.Clone());
        public RosterRefreshResult Refresh(string championId, RaceSeed seed)
        {
            var championLane = Array.IndexOf(_current, championId); if (championLane < 0) throw new ArgumentException("Champion is not in current roster.", nameof(championId));
            var previous = (string[])_current.Clone(); var excluded = new HashSet<string>(_current, StringComparer.Ordinal); var candidates = new List<string>();
            foreach (var id in _catalog) if (!excluded.Contains(id)) candidates.Add(id);
            if (candidates.Count < 4) throw new InvalidOperationException("Catalog cannot provide four new unique replacements.");
            var random = new SeededRandomSource(seed);
            for (var i = candidates.Count - 1; i > 0; i--) { var j = random.NextInt(0, i + 1); (candidates[i], candidates[j]) = (candidates[j], candidates[i]); }
            var next = new string[5]; var replacement = 0;
            for (var lane = 0; lane < 5; lane++) next[lane] = lane == championLane ? championId : candidates[replacement++];
            _current = next; return new RosterRefreshResult(championId, previous, next);
        }
        public void Reset(IReadOnlyList<string> roster) { if (roster == null || roster.Count != 5) throw new ArgumentException("Roster requires five chickens.", nameof(roster)); _current = CopyUnique(roster, "Roster"); }
        private static string[] CopyUnique(IReadOnlyList<string> source, string label)
        {
            var copy = new string[source.Count]; var unique = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < source.Count; i++) { copy[i] = source[i]; if (string.IsNullOrWhiteSpace(copy[i]) || !unique.Add(copy[i])) throw new ArgumentException($"{label} IDs must be non-empty and unique."); }
            return copy;
        }
    }
}
