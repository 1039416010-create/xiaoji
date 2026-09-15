using System;
using System.Collections.Generic;
using System.Linq;

namespace GroundChickenKing.Players
{
    public sealed class PlayerManager
    {
        private readonly object _gate = new();
        private readonly int _initialCoins;
        private readonly Dictionary<PlayerSeat, PlayerState> _states;
        private readonly List<BalanceTransaction> _transactions = new();

        public PlayerManager(int initialCoins)
        {
            if (initialCoins < 0)
                throw new ArgumentOutOfRangeException(nameof(initialCoins));

            _initialCoins = initialCoins;
            _states = CreateSeatDefinitions().ToDictionary(definition => definition.Seat, definition => new PlayerState(definition));
        }

        public event Action<PlayerSnapshot> PlayerJoined;
        public event Action<PlayerSnapshot> PlayerLeft;
        public event Action<PlayerSnapshot> SnapshotChanged;
        public event Action<BalanceTransaction> BalanceChanged;

        public int JoinedCount
        {
            get
            {
                lock (_gate)
                    return _states.Values.Count(state => state.IsJoined);
            }
        }

        public bool TryJoin(PlayerSeat seat, out PlayerSnapshot snapshot)
        {
            BalanceTransaction transaction;
            lock (_gate)
            {
                if (!_states.TryGetValue(seat, out var state) || !state.TryJoin(_initialCoins))
                {
                    snapshot = _states.TryGetValue(seat, out state) ? state.ToSnapshot() : default;
                    return false;
                }

                snapshot = state.ToSnapshot();
                transaction = new BalanceTransaction(seat, 0, _initialCoins, _initialCoins, 0, BalanceChangeReason.InitialAllocation);
                _transactions.Add(transaction);
            }

            PlayerJoined?.Invoke(snapshot);
            SnapshotChanged?.Invoke(snapshot);
            BalanceChanged?.Invoke(transaction);
            return true;
        }

        public bool TryDebit(PlayerSeat seat, int amount, int round, BalanceChangeReason reason, out BalanceTransaction transaction)
        {
            PlayerSnapshot snapshot;
            lock (_gate)
            {
                if (!_states.TryGetValue(seat, out var state) || !state.TryDebit(amount, out var before, out var after))
                {
                    transaction = default;
                    return false;
                }

                transaction = new BalanceTransaction(seat, before, -amount, after, round, reason);
                _transactions.Add(transaction);
                snapshot = state.ToSnapshot();
            }

            BalanceChanged?.Invoke(transaction);
            SnapshotChanged?.Invoke(snapshot);
            return true;
        }

        public bool TryCredit(PlayerSeat seat, int amount, int round, BalanceChangeReason reason, out BalanceTransaction transaction)
        {
            PlayerSnapshot snapshot;
            lock (_gate)
            {
                if (!_states.TryGetValue(seat, out var state) || !state.TryCredit(amount, out var before, out var after))
                { transaction = default; return false; }
                transaction = new BalanceTransaction(seat, before, amount, after, round, reason);
                _transactions.Add(transaction); snapshot = state.ToSnapshot();
            }
            BalanceChanged?.Invoke(transaction); SnapshotChanged?.Invoke(snapshot); return true;
        }

        public void ResetJoinedCoins(int coins)
        {
            if (coins < 0) throw new ArgumentOutOfRangeException(nameof(coins));
            var changes = new List<(PlayerSnapshot Snapshot, BalanceTransaction Transaction)>();
            lock (_gate)
            {
                foreach (var pair in _states)
                {
                    if (!pair.Value.TryResetCoins(coins, out var before, out var after)) continue;
                    var transaction = new BalanceTransaction(pair.Key, before, after - before, after, 0, BalanceChangeReason.SessionRestart);
                    _transactions.Add(transaction); changes.Add((pair.Value.ToSnapshot(), transaction));
                }
            }
            foreach (var change in changes) { BalanceChanged?.Invoke(change.Transaction); SnapshotChanged?.Invoke(change.Snapshot); }
        }

        public bool TryLeave(PlayerSeat seat, out PlayerSnapshot snapshot)
        {
            lock (_gate)
            {
                if (!_states.TryGetValue(seat, out var state) || !state.TryLeave())
                {
                    snapshot = _states.TryGetValue(seat, out state) ? state.ToSnapshot() : default;
                    return false;
                }

                snapshot = state.ToSnapshot();
            }

            PlayerLeft?.Invoke(snapshot);
            SnapshotChanged?.Invoke(snapshot);
            return true;
        }

        public void ClearAll()
        {
            PlayerSnapshot[] changed;
            lock (_gate)
            {
                changed = _states.Values
                    .Where(state => state.TryLeave())
                    .Select(state => state.ToSnapshot())
                    .ToArray();
            }

            foreach (var snapshot in changed)
            {
                PlayerLeft?.Invoke(snapshot);
                SnapshotChanged?.Invoke(snapshot);
            }
        }

        public PlayerSnapshot GetSnapshot(PlayerSeat seat)
        {
            lock (_gate)
            {
                if (!_states.TryGetValue(seat, out var state))
                    throw new ArgumentOutOfRangeException(nameof(seat));
                return state.ToSnapshot();
            }
        }

        public IReadOnlyList<PlayerSnapshot> GetAllSnapshots()
        {
            lock (_gate)
                return _states.OrderBy(pair => pair.Key).Select(pair => pair.Value.ToSnapshot()).ToArray();
        }

        public IReadOnlyList<BalanceTransaction> GetTransactions()
        {
            lock (_gate)
                return _transactions.ToArray();
        }

        private static IEnumerable<PlayerSeatDefinition> CreateSeatDefinitions()
        {
            yield return new PlayerSeatDefinition(PlayerSeat.Player1, "player-1-bottom-left", "玩家 1", PlayerFacing.Bottom, PlayerTheme.RedDiamond, "◆");
            yield return new PlayerSeatDefinition(PlayerSeat.Player2, "player-2-bottom-right", "玩家 2", PlayerFacing.Bottom, PlayerTheme.BlueCircle, "●");
            yield return new PlayerSeatDefinition(PlayerSeat.Player3, "player-3-top-left", "玩家 3", PlayerFacing.Top, PlayerTheme.YellowTriangle, "▲");
            yield return new PlayerSeatDefinition(PlayerSeat.Player4, "player-4-top-right", "玩家 4", PlayerFacing.Top, PlayerTheme.GreenSquare, "■");
        }
    }
}
