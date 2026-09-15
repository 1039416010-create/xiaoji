using System;
using System.Collections.Generic;
using System.Linq;
using GroundChickenKing.Players;

namespace GroundChickenKing.Betting
{
    public sealed class BettingManager
    {
        private readonly object _gate = new();
        private readonly PlayerManager _players;
        private readonly HashSet<string> _chickenIds;
        private readonly HashSet<int> _allowedStakes;
        private readonly Dictionary<PlayerSeat, BetState> _states;
        private bool _isOpen = true;
        private bool _allBetsLockedPublished;

        public BettingManager(PlayerManager players, IEnumerable<string> chickenIds, IEnumerable<int> allowedStakes, int round)
        {
            _players = players ?? throw new ArgumentNullException(nameof(players));
            if (round <= 0) throw new ArgumentOutOfRangeException(nameof(round));
            Round = round;
            _chickenIds = new HashSet<string>(chickenIds ?? throw new ArgumentNullException(nameof(chickenIds)), StringComparer.Ordinal);
            _allowedStakes = new HashSet<int>(allowedStakes ?? throw new ArgumentNullException(nameof(allowedStakes)));
            if (_chickenIds.Count != 5) throw new ArgumentException("Exactly five unique chicken IDs are required.", nameof(chickenIds));
            if (!_allowedStakes.SetEquals(new[] { 1, 5, 10 })) throw new ArgumentException("Allowed stakes must be exactly 1, 5, 10.", nameof(allowedStakes));

            _states = _players.GetAllSnapshots()
                .Where(snapshot => snapshot.IsJoined && !snapshot.IsBankrupt)
                .ToDictionary(snapshot => snapshot.Definition.Seat, snapshot => new BetState(snapshot.Definition.Seat));
            if (_states.Count == 0) throw new InvalidOperationException("At least one eligible player is required to begin betting.");
        }

        public event Action<BetSnapshot> BetChanged;
        public event Action<IReadOnlyList<BetSnapshot>> AllBetsLocked;

        public int Round { get; }
        public bool IsOpen => _isOpen;
        public int EligiblePlayerCount => _states.Count;

        public BetCommandResult TrySelectChicken(PlayerSeat seat, string chickenId)
        {
            BetSnapshot snapshot;
            lock (_gate)
            {
                var validation = ValidateMutableSeat(seat, out var state);
                if (!validation.IsSuccess) return validation;
                if (!_chickenIds.Contains(chickenId)) return BetCommandResult.Failure(BetCommandError.InvalidChicken);
                state.SelectChicken(chickenId);
                snapshot = state.ToSnapshot();
            }

            BetChanged?.Invoke(snapshot);
            return BetCommandResult.Success();
        }

        public BetCommandResult TrySelectStake(PlayerSeat seat, int stake)
        {
            BetSnapshot snapshot;
            lock (_gate)
            {
                var validation = ValidateMutableSeat(seat, out var state);
                if (!validation.IsSuccess) return validation;
                if (!_allowedStakes.Contains(stake)) return BetCommandResult.Failure(BetCommandError.InvalidStake);
                if (_players.GetSnapshot(seat).Coins < stake) return BetCommandResult.Failure(BetCommandError.InsufficientBalance);
                state.SelectStake(stake);
                snapshot = state.ToSnapshot();
            }

            BetChanged?.Invoke(snapshot);
            return BetCommandResult.Success();
        }

        public BetCommandResult TryLockBet(PlayerSeat seat)
        {
            BetSnapshot snapshot;
            IReadOnlyList<BetSnapshot> completedBets = null;
            lock (_gate)
            {
                var validation = ValidateMutableSeat(seat, out var state);
                if (!validation.IsSuccess) return validation;
                if (string.IsNullOrWhiteSpace(state.ChickenId)) return BetCommandResult.Failure(BetCommandError.MissingChicken);
                if (state.Stake <= 0) return BetCommandResult.Failure(BetCommandError.MissingStake);
                if (!_players.TryDebit(seat, state.Stake, Round, BalanceChangeReason.BetLocked, out _))
                    return BetCommandResult.Failure(BetCommandError.InsufficientBalance);

                state.Lock();
                snapshot = state.ToSnapshot();
                if (!_allBetsLockedPublished && _states.Values.All(bet => bet.IsLocked))
                {
                    _allBetsLockedPublished = true;
                    _isOpen = false;
                    completedBets = GetSnapshotsUnsafe();
                }
            }

            BetChanged?.Invoke(snapshot);
            if (completedBets != null)
                AllBetsLocked?.Invoke(completedBets);
            return BetCommandResult.Success();
        }

        public BetCommandResult ClearUncommittedRound()
        {
            lock (_gate)
            {
                if (_states.Values.Any(state => state.IsLocked))
                    return BetCommandResult.Failure(BetCommandError.LockedBetsCannotBeCleared);
                _states.Clear();
                _isOpen = false;
                return BetCommandResult.Success();
            }
        }

        public bool TryGetSnapshot(PlayerSeat seat, out BetSnapshot snapshot)
        {
            lock (_gate)
            {
                if (!_states.TryGetValue(seat, out var state))
                {
                    snapshot = default;
                    return false;
                }
                snapshot = state.ToSnapshot();
                return true;
            }
        }

        public IReadOnlyList<BetSnapshot> GetSnapshots()
        {
            lock (_gate)
                return GetSnapshotsUnsafe();
        }

        private BetCommandResult ValidateMutableSeat(PlayerSeat seat, out BetState state)
        {
            state = null;
            if (!_isOpen) return BetCommandResult.Failure(BetCommandError.BettingClosed);
            if (!_states.TryGetValue(seat, out state)) return BetCommandResult.Failure(BetCommandError.PlayerNotEligible);
            if (state.IsLocked) return BetCommandResult.Failure(BetCommandError.AlreadyLocked);
            return BetCommandResult.Success();
        }

        private IReadOnlyList<BetSnapshot> GetSnapshotsUnsafe()
        {
            return _states.OrderBy(pair => pair.Key).Select(pair => pair.Value.ToSnapshot()).ToArray();
        }
    }
}
