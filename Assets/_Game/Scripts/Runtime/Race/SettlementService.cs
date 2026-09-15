using System;
using System.Collections.Generic;
using GroundChickenKing.Betting;
using GroundChickenKing.Players;

namespace GroundChickenKing.Race
{
    public sealed class SettlementService
    {
        private readonly object _gate = new();
        private readonly PlayerManager _players;
        private readonly Dictionary<string, SettlementRecord> _records = new(StringComparer.Ordinal);
        public SettlementService(PlayerManager players) => _players = players ?? throw new ArgumentNullException(nameof(players));

        public bool TrySettle(int round, RaceResult result, IReadOnlyList<BetSnapshot> bets, out SettlementRecord record)
        {
            if (round <= 0) throw new ArgumentOutOfRangeException(nameof(round));
            if (string.IsNullOrWhiteSpace(result.PlanId) || string.IsNullOrWhiteSpace(result.ChampionId)) throw new ArgumentException("Race result identity is required.", nameof(result));
            if (bets == null || bets.Count == 0) throw new ArgumentException("At least one locked bet is required.", nameof(bets));
            var key = $"{round}:{result.PlanId}";
            lock (_gate)
            {
                if (_records.TryGetValue(key, out record)) return false;
                for (var i = 0; i < bets.Count; i++) if (!bets[i].IsLocked) throw new ArgumentException("Only locked bets can be settled.", nameof(bets));
                var details = new PlayerSettlementDetail[bets.Count];
                for (var i = 0; i < bets.Count; i++)
                {
                    var bet = bets[i]; var didWin = bet.ChickenId == result.ChampionId; var returned = didWin ? bet.Stake * 2 : 0;
                    if (returned > 0 && !_players.TryCredit(bet.Seat, returned, round, BalanceChangeReason.BetPayout, out _))
                        throw new InvalidOperationException($"Unable to credit payout for {bet.Seat}.");
                    details[i] = new PlayerSettlementDetail(bet.Seat, bet.ChickenId, bet.Stake, didWin, returned, _players.GetSnapshot(bet.Seat).Coins);
                }
                record = new SettlementRecord(round, result.PlanId, result.ChampionId, details); _records.Add(key, record); return true;
            }
        }
    }
}
