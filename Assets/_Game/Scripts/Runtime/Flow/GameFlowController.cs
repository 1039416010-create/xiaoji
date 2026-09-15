using System;

namespace GroundChickenKing.Flow
{
    public sealed class GameFlowController
    {
        private readonly Func<int> _getJoinedPlayerCount;
        private readonly float _stateTimeoutSeconds;
        private float _elapsedInState;

        public GameFlowController(Func<int> getJoinedPlayerCount, float stateTimeoutSeconds)
        {
            _getJoinedPlayerCount = getJoinedPlayerCount ?? throw new ArgumentNullException(nameof(getJoinedPlayerCount));
            if (stateTimeoutSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(stateTimeoutSeconds));

            _stateTimeoutSeconds = stateTimeoutSeconds;
            CurrentState = GameFlowState.Boot;
        }

        public event Action<GameFlowTransition> StateChanged;
        public event Action<GameFlowState> StateRecoveredFromTimeout;

        public GameFlowState CurrentState { get; private set; }

        public bool Initialize()
        {
            return TryTransition(GameFlowState.Boot, GameFlowState.MainMenu, "Application initialized");
        }

        public bool EnterPlayerJoin()
        {
            return TryTransition(GameFlowState.MainMenu, GameFlowState.PlayerJoin, "Start game button pressed");
        }

        public bool ContinueToWarmup()
        {
            if (_getJoinedPlayerCount() < 1)
                return false;

            return TryTransition(GameFlowState.PlayerJoin, GameFlowState.Warmup, "At least one player confirmed");
        }

        public bool ReturnToPlayerJoin()
        {
            return TryTransition(GameFlowState.Warmup, GameFlowState.PlayerJoin, "Return to player join button pressed");
        }

        public bool EnterBetting()
        {
            if (_getJoinedPlayerCount() < 1)
                return false;

            return TryTransition(GameFlowState.Warmup, GameFlowState.Betting, "Enter betting button pressed");
        }

        public bool MarkAllBetsLocked()
        {
            return TryTransition(GameFlowState.Betting, GameFlowState.RaceCountdown, "All eligible players locked their bets");
        }

        public bool MarkCountdownCompleted()
        {
            return TryTransition(GameFlowState.RaceCountdown, GameFlowState.Racing, "Locked race countdown completed");
        }

        public bool MarkRaceCompleted()
        {
            return TryTransition(GameFlowState.Racing, GameFlowState.Settlement, "Validated race plan completed");
        }

        public bool MarkSettlementComplete()
        {
            return TryTransition(GameFlowState.Settlement, GameFlowState.RosterRefresh, "Settlement presentation completed");
        }

        public bool BeginNextRound()
        {
            return TryTransition(GameFlowState.RosterRefresh, GameFlowState.Warmup, "Champion retained and roster refreshed");
        }

        public bool EndGameFromSettlement()
        {
            return TryTransition(GameFlowState.Settlement, GameFlowState.GameOver, "Configured game-over condition reached");
        }

        public bool RestartFromGameOver()
        {
            if (_getJoinedPlayerCount() < 1) return false;
            return TryTransition(GameFlowState.GameOver, GameFlowState.Warmup, "Touch restart requested");
        }

        public bool ReturnToMainMenu()
        {
            if (CurrentState != GameFlowState.PlayerJoin &&
                CurrentState != GameFlowState.Warmup &&
                CurrentState != GameFlowState.Betting &&
                CurrentState != GameFlowState.RaceCountdown &&
                CurrentState != GameFlowState.Racing &&
                CurrentState != GameFlowState.Settlement &&
                CurrentState != GameFlowState.RosterRefresh &&
                CurrentState != GameFlowState.GameOver)
                return false;

            return TransitionTo(GameFlowState.MainMenu, "Return to main menu button pressed");
        }

        public void Tick(float unscaledDeltaTime)
        {
            if (unscaledDeltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(unscaledDeltaTime));

            if (CurrentState != GameFlowState.PlayerJoin && CurrentState != GameFlowState.Warmup)
                return;

            _elapsedInState += unscaledDeltaTime;
            if (_elapsedInState < _stateTimeoutSeconds)
                return;

            RecoverFromTimeout();
        }

        private void RecoverFromTimeout()
        {
            var recoveredState = CurrentState;
            if (CurrentState == GameFlowState.Warmup)
                TransitionTo(GameFlowState.PlayerJoin, "Warmup timeout recovered safely");
            else
                _elapsedInState = 0f;

            StateRecoveredFromTimeout?.Invoke(recoveredState);
        }

        private bool TryTransition(GameFlowState expected, GameFlowState target, string reason)
        {
            if (CurrentState != expected)
                return false;

            return TransitionTo(target, reason);
        }

        private bool TransitionTo(GameFlowState target, string reason)
        {
            var previous = CurrentState;
            CurrentState = target;
            _elapsedInState = 0f;
            StateChanged?.Invoke(new GameFlowTransition(previous, target, reason));
            return true;
        }
    }
}
