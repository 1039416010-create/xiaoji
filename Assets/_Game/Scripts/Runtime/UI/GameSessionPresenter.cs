using System.Linq;
using GroundChickenKing.Betting;
using GroundChickenKing.Audio;
using GroundChickenKing.Chickens;
using GroundChickenKing.Core;
using GroundChickenKing.Diagnostics;
using GroundChickenKing.Flow;
using GroundChickenKing.Players;
using GroundChickenKing.Race;
using UnityEngine;
using UnityEngine.UI;

namespace GroundChickenKing.UI
{
    public sealed class GameSessionPresenter : MonoBehaviour
    {
        [SerializeField] private GameRulesConfig _rules;
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _playerJoinPanel;
        [SerializeField] private GameObject _warmupPanel;
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _joinBackButton;
        [SerializeField] private Button _warmupBackButton;
        [SerializeField] private Button _warmupMainMenuButton;
        [SerializeField] private Text _joinSummaryText;
        [SerializeField] private Text _warmupSummaryText;
        [SerializeField] private PlayerSeatView[] _seatViews;
        [SerializeField] private GameObject _bettingPanel;
        [SerializeField] private GameObject _raceCountdownPanel;
        [SerializeField] private Button _enterBettingButton;
        [SerializeField] private Button _bettingMainMenuButton;
        [SerializeField] private Button _raceCountdownMainMenuButton;
        [SerializeField] private Text _bettingStatusText;
        [SerializeField] private Text _raceCountdownSummaryText;
        [SerializeField] private PlayerBettingView[] _bettingViews;
        [SerializeField] private RaceConfig _raceConfig;
        [SerializeField] private ChickenRacePresenter _racePresentation;
        [SerializeField] private ChickenRosterCatalogConfig _rosterCatalog;
        [SerializeField] private GameObject _settlementPanel;
        [SerializeField] private Text _settlementSummaryText;
        [SerializeField] private Text _curryResultText;
        [SerializeField] private Button _nextRoundButton;
        [SerializeField] private Button _settlementMainMenuButton;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private Text _gameOverSummaryText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _gameOverMainMenuButton;
        [SerializeField] private ConfirmationDialog _confirmationDialog;
        [SerializeField] private ExhibitAudioController _audio;

        private PlayerManager _players;
        private GameFlowController _flow;
        private BettingManager _betting;
        private RaceDirector _raceDirector;
        private RaceCountdownClock _countdownClock;
        private RacePlaybackClock _playbackClock;
        private RaceResult? _lastRaceResult;
        private string _lastCountdownToken;
        private float _nextRaceUiRefresh;
        private SettlementService _settlementService;
        private SettlementRecord _lastSettlement;
        private RosterService _rosterService;
        private int _round = 1;
        private GameOverReason _gameOverReason;
        private int _runtimeMaximumRounds;
        private static readonly string[] InitialChickenIds =
        {
            "chicken-flash", "chicken-chubby", "chicken-tiny", "chicken-bro", "chicken-slacker",
        };

        public GameFlowState CurrentState => _flow?.CurrentState ?? GameFlowState.Boot;
        public int JoinedPlayerCount => _players?.JoinedCount ?? 0;
        public Button ContinueButton => _continueButton;
        public Button EnterBettingButton => _enterBettingButton;
        public RacePlan CurrentRacePlan => _raceDirector?.CurrentPlan;
        public RaceResult? LastRaceResult => _lastRaceResult;
        public SettlementRecord LastSettlement => _lastSettlement;
        public int CurrentRound => _round;
        public GameOverReason CurrentGameOverReason => _gameOverReason;
        public System.Collections.Generic.IReadOnlyList<string> CurrentRoster => _rosterService?.CurrentRoster;

        public void ConfigureOperations(ConfirmationDialog confirmationDialog, ExhibitAudioController audio)
        {
            _confirmationDialog = confirmationDialog; _audio = audio;
        }

        public void ApplyMaximumRounds(int maximumRounds) => _runtimeMaximumRounds = Mathf.Clamp(maximumRounds, 1, 50);

        public void Configure(
            GameRulesConfig rules,
            GameObject mainMenuPanel,
            GameObject playerJoinPanel,
            GameObject warmupPanel,
            Button startGameButton,
            Button continueButton,
            Button joinBackButton,
            Button warmupBackButton,
            Button warmupMainMenuButton,
            Text joinSummaryText,
            Text warmupSummaryText,
            PlayerSeatView[] seatViews)
        {
            _rules = rules;
            _mainMenuPanel = mainMenuPanel;
            _playerJoinPanel = playerJoinPanel;
            _warmupPanel = warmupPanel;
            _startGameButton = startGameButton;
            _continueButton = continueButton;
            _joinBackButton = joinBackButton;
            _warmupBackButton = warmupBackButton;
            _warmupMainMenuButton = warmupMainMenuButton;
            _joinSummaryText = joinSummaryText;
            _warmupSummaryText = warmupSummaryText;
            _seatViews = seatViews;
        }

        public void ConfigureBetting(
            GameObject bettingPanel,
            GameObject raceCountdownPanel,
            Button enterBettingButton,
            Button bettingMainMenuButton,
            Button raceCountdownMainMenuButton,
            Text bettingStatusText,
            Text raceCountdownSummaryText,
            PlayerBettingView[] bettingViews)
        {
            _bettingPanel = bettingPanel;
            _raceCountdownPanel = raceCountdownPanel;
            _enterBettingButton = enterBettingButton;
            _bettingMainMenuButton = bettingMainMenuButton;
            _raceCountdownMainMenuButton = raceCountdownMainMenuButton;
            _bettingStatusText = bettingStatusText;
            _raceCountdownSummaryText = raceCountdownSummaryText;
            _bettingViews = bettingViews;
        }

        public void ConfigureRace(RaceConfig raceConfig)
        {
            _raceConfig = raceConfig;
        }

        public void ConfigurePresentation(ChickenRacePresenter racePresentation)
        {
            _racePresentation = racePresentation;
        }

        public void ConfigureLoop(
            ChickenRosterCatalogConfig rosterCatalog,
            GameObject settlementPanel, Text settlementSummaryText, Text curryResultText, Button nextRoundButton, Button settlementMainMenuButton,
            GameObject gameOverPanel, Text gameOverSummaryText, Button restartButton, Button gameOverMainMenuButton)
        {
            _rosterCatalog = rosterCatalog; _settlementPanel = settlementPanel; _settlementSummaryText = settlementSummaryText; _curryResultText = curryResultText;
            _nextRoundButton = nextRoundButton; _settlementMainMenuButton = settlementMainMenuButton; _gameOverPanel = gameOverPanel;
            _gameOverSummaryText = gameOverSummaryText; _restartButton = restartButton; _gameOverMainMenuButton = gameOverMainMenuButton;
        }

        private void Awake()
        {
            _runtimeMaximumRounds = _rules.MaximumRounds;
            _players = new PlayerManager(_rules.InitialCoins);
            if (_raceConfig == null)
                _raceConfig = ScriptableObject.CreateInstance<RaceConfig>();
            _flow = new GameFlowController(() => _players.JoinedCount, _rules.StateTimeoutSeconds);
            if (_rosterCatalog == null) throw new System.InvalidOperationException("Chicken roster catalog is required.");
            var catalogIds = _rosterCatalog.Definitions.Select(definition => definition.StableId).ToArray();
            _rosterService = new RosterService(catalogIds, InitialChickenIds);
            _settlementService = new SettlementService(_players);
            _raceDirector = new RaceDirector(_rosterService.CurrentRoster, RaceRules.FromConfig(_raceConfig));
            _countdownClock = new RaceCountdownClock();
            _playbackClock = new RacePlaybackClock(_raceConfig.HardTimeoutSeconds);
            _playbackClock.RaceCompleted += HandleRaceCompleted;
            _flow.StateChanged += HandleStateChanged;
            _flow.StateRecoveredFromTimeout += HandleTimeoutRecovery;
            _players.SnapshotChanged += HandlePlayerSnapshotChanged;

            _startGameButton.onClick.AddListener(HandleStartGame);
            _continueButton.onClick.AddListener(HandleContinue);
            _joinBackButton.onClick.AddListener(RequestReturnToMainMenu);
            _warmupBackButton.onClick.AddListener(HandleReturnToPlayerJoin);
            _warmupMainMenuButton.onClick.AddListener(RequestReturnToMainMenu);
            _enterBettingButton.onClick.AddListener(HandleEnterBetting);
            _bettingMainMenuButton.onClick.AddListener(RequestReturnToMainMenu);
            _raceCountdownMainMenuButton.onClick.AddListener(RequestReturnToMainMenu);
            _nextRoundButton.onClick.AddListener(HandleContinueAfterSettlement);
            _settlementMainMenuButton.onClick.AddListener(RequestReturnToMainMenu);
            _restartButton.onClick.AddListener(RequestRestart);
            _gameOverMainMenuButton.onClick.AddListener(RequestReturnToMainMenu);

            foreach (var seatView in _seatViews)
                seatView.Bind(HandleJoinRequested);
            foreach (var bettingView in _bettingViews)
                bettingView.Bind(HandleChickenSelected, HandleStakeSelected, HandleLockRequested);

            _flow.Initialize();
            RefreshAll();
        }

        private void Update()
        {
            _flow?.Tick(Time.unscaledDeltaTime);
            if (_flow?.CurrentState == GameFlowState.RaceCountdown && _countdownClock.IsRunning)
            {
                var countdownToken = _countdownClock.DisplayText;
                if (_lastCountdownToken != countdownToken)
                {
                    _lastCountdownToken = countdownToken;
                    _raceCountdownSummaryText.text = UiTextCatalog.Countdown(_raceDirector.CurrentPlan.Seed.Value, countdownToken);
                    if (countdownToken == "GO") _audio?.PlayGo(); else _audio?.PlayCountdown();
                }
                if (_countdownClock.Tick(Time.unscaledDeltaTime))
                {
                    _flow.MarkCountdownCompleted();
                    _racePresentation?.Play();
                    _playbackClock.Start(_raceDirector.CurrentPlan);
                    _nextRaceUiRefresh = 0f;
                }
            }
            else if (_flow?.CurrentState == GameFlowState.Racing && Time.unscaledTime >= _nextRaceUiRefresh)
            {
                _nextRaceUiRefresh = Time.unscaledTime + 0.1f;
                _raceCountdownSummaryText.text = UiTextCatalog.Racing(_playbackClock.Elapsed, _raceDirector.CurrentPlan.PlanId);
                _playbackClock.Tick(Time.unscaledDeltaTime);
            }
            else if (_flow?.CurrentState == GameFlowState.Racing)
            {
                _playbackClock.Tick(Time.unscaledDeltaTime);
            }
        }

        private void HandleStartGame()
        {
            _round = 1; _gameOverReason = GameOverReason.None; _lastSettlement = null; _lastRaceResult = null;
            _rosterService.Reset(InitialChickenIds); _racePresentation?.ApplyRoster(_rosterService.CurrentRoster);
            _settlementService = new SettlementService(_players);
            _raceDirector = new RaceDirector(_rosterService.CurrentRoster, RaceRules.FromConfig(_raceConfig));
            _flow.EnterPlayerJoin();
        }

        private void HandleContinue()
        {
            _flow.ContinueToWarmup();
        }

        private void HandleReturnToMainMenu()
        {
            if (_flow.ReturnToMainMenu())
            {
                _racePresentation?.CancelAll();
                _players.ClearAll();
            }
        }

        private void RequestReturnToMainMenu()
        {
            if (_confirmationDialog == null) { HandleReturnToMainMenu(); return; }
            _confirmationDialog.Show(UiTextCatalog.ConfirmReturnHome, HandleReturnToMainMenu);
        }

        private void RequestRestart()
        {
            if (_confirmationDialog == null) { HandleRestart(); return; }
            _confirmationDialog.Show(UiTextCatalog.ConfirmRestart, HandleRestart);
        }

        public void ReturnHomeForIdle()
        {
            if (_flow == null || _flow.CurrentState is GameFlowState.MainMenu or GameFlowState.RaceCountdown or GameFlowState.Racing) return;
            _confirmationDialog?.Cancel(); HandleReturnToMainMenu();
        }

        private void HandleReturnToPlayerJoin()
        {
            _flow.ReturnToPlayerJoin();
        }

        private void HandleEnterBetting()
        {
            if (_flow.CurrentState != GameFlowState.Warmup)
                return;

            var roster = _rosterService.CurrentRoster;
            var displayNames = roster.Select(id => _rosterCatalog.Definitions.First(definition => definition.StableId == id).DisplayName).ToArray();
            foreach (var bettingView in _bettingViews) bettingView.ApplyChickenRoster(roster, displayNames);
            _audio?.BindVisibleButtons();
            _betting = new BettingManager(_players, roster, _rules.AllowedBets, _round);
            _betting.BetChanged += HandleBetChanged;
            _betting.AllBetsLocked += HandleAllBetsLocked;
            _flow.EnterBetting();
            RefreshAll();
        }

        private void HandleChickenSelected(PlayerSeat seat, string chickenId)
        {
            if (_flow.CurrentState == GameFlowState.Betting)
                _betting.TrySelectChicken(seat, chickenId);
        }

        private void HandleStakeSelected(PlayerSeat seat, int stake)
        {
            if (_flow.CurrentState == GameFlowState.Betting)
                _betting.TrySelectStake(seat, stake);
        }

        private void HandleLockRequested(PlayerSeat seat)
        {
            if (_flow.CurrentState == GameFlowState.Betting)
                _betting.TryLockBet(seat);
        }

        private void HandleBetChanged(BetSnapshot snapshot)
        {
            GameLog.Info("Betting", $"seat={snapshot.Seat} target={snapshot.ChickenId ?? "none"} stake={snapshot.Stake} locked={snapshot.IsLocked}");
            RefreshAll();
        }

        private void HandleAllBetsLocked(System.Collections.Generic.IReadOnlyList<BetSnapshot> snapshots)
        {
            _raceDirector.NotifyAllBetsLocked();
            var seed = new RaceSeed(System.BitConverter.ToInt32(System.Guid.NewGuid().ToByteArray(), 0));
            if (!_raceDirector.TryGeneratePlan(_round, seed, out var plan, out var error))
            {
                GameLog.Error("Race", error, round: _round);
                return;
            }

            GameLog.Info("Race", $"roster={string.Join(",", _rosterService.CurrentRoster)} seed={plan.Seed.Value} config={plan.ConfigHash} champion={plan.ChampionId} plan={plan.PlanId} validation=passed", round: plan.Round);
            _racePresentation?.BindPlan(plan);
            _flow.MarkAllBetsLocked();
            _lastCountdownToken = null;
            _countdownClock.Start(plan);
        }

        private void HandleRaceCompleted(RaceResult result)
        {
            _audio?.PlayFinish();
            _lastRaceResult = result;
            _racePresentation?.ForceSafeCompletion();
            GameLog.Info("Race", $"plan={result.PlanId} champion={result.ChampionId} finish={result.FinishTime:0.000} reason={result.Reason}", round: _round);
            _flow.MarkRaceCompleted();
            if (!_settlementService.TrySettle(_round, result, _betting.GetSnapshots(), out _lastSettlement))
                GameLog.Warning("Settlement", $"Duplicate settlement rejected plan={result.PlanId}", round: _round);
            else
                foreach (var detail in _lastSettlement.Details)
                    GameLog.Info("Settlement", $"seat={detail.Seat} target={detail.ChickenId} stake={detail.Stake} win={detail.DidWin} return={detail.ReturnAmount} balance={detail.BalanceAfter}", round: _round);
            _audio?.PlaySettlement();
            RefreshAll();
        }

        private void HandleContinueAfterSettlement()
        {
            if (_flow.CurrentState != GameFlowState.Settlement || _lastSettlement == null) return;
            _gameOverReason = SessionEndEvaluator.Evaluate(_players.GetAllSnapshots(), _round, _runtimeMaximumRounds);
            if (_gameOverReason != GameOverReason.None) { _flow.EndGameFromSettlement(); return; }
            _flow.MarkSettlementComplete();
            var refresh = _rosterService.Refresh(_lastSettlement.ChampionId, new RaceSeed(unchecked(_raceDirector.CurrentPlan.Seed.Value + _round * 104729)));
            _racePresentation?.ApplyRoster(refresh.CurrentRoster);
            GameLog.Info("Roster", $"champion={refresh.ChampionId} previous={string.Join(",", refresh.PreviousRoster)} current={string.Join(",", refresh.CurrentRoster)}", round: _round);
            _round++; _lastRaceResult = null; _lastSettlement = null; _betting = null;
            _raceDirector = new RaceDirector(_rosterService.CurrentRoster, RaceRules.FromConfig(_raceConfig));
            _flow.BeginNextRound();
        }

        private void HandleRestart()
        {
            if (_flow.CurrentState != GameFlowState.GameOver) return;
            _players.ResetJoinedCoins(_rules.InitialCoins); _round = 1; _gameOverReason = GameOverReason.None;
            _rosterService.Reset(InitialChickenIds); _racePresentation?.ApplyRoster(_rosterService.CurrentRoster);
            _settlementService = new SettlementService(_players); _raceDirector = new RaceDirector(_rosterService.CurrentRoster, RaceRules.FromConfig(_raceConfig));
            _lastRaceResult = null; _lastSettlement = null; _betting = null; _flow.RestartFromGameOver();
        }

        private void HandleJoinRequested(PlayerSeat seat)
        {
            if (_flow.CurrentState != GameFlowState.PlayerJoin)
                return;

            _players.TryJoin(seat, out _);
        }

        private void HandleStateChanged(GameFlowTransition transition)
        {
            GameLog.Info("Flow", $"{transition.From} -> {transition.To}: {transition.Reason}");
            RefreshAll();
        }

        private void HandleTimeoutRecovery(GameFlowState recoveredState)
        {
            GameLog.Warning("Flow", $"Recovered safely from {recoveredState} timeout.");
            RefreshAll();
        }

        private void HandlePlayerSnapshotChanged(PlayerSnapshot snapshot)
        {
            GameLog.Info("Players", $"{snapshot.Definition.StableId} joined={snapshot.IsJoined} coins={snapshot.Coins}");
            RefreshAll();
        }

        private void RefreshAll()
        {
            if (_flow == null || _players == null)
                return;

            _mainMenuPanel.SetActive(_flow.CurrentState == GameFlowState.MainMenu);
            _playerJoinPanel.SetActive(_flow.CurrentState == GameFlowState.PlayerJoin);
            _warmupPanel.SetActive(_flow.CurrentState == GameFlowState.Warmup);
            _bettingPanel.SetActive(_flow.CurrentState == GameFlowState.Betting);
            _raceCountdownPanel.SetActive(_flow.CurrentState == GameFlowState.RaceCountdown || _flow.CurrentState == GameFlowState.Racing);
            _settlementPanel.SetActive(_flow.CurrentState == GameFlowState.Settlement);
            _gameOverPanel.SetActive(_flow.CurrentState == GameFlowState.GameOver);

            var snapshots = _players.GetAllSnapshots();
            foreach (var seatView in _seatViews)
                seatView.Render(snapshots.First(snapshot => snapshot.Definition.Seat == seatView.Seat));

            _continueButton.interactable = _players.JoinedCount >= _rules.MinimumPlayers;
            _joinSummaryText.text = UiTextCatalog.JoinSummary(_players.JoinedCount);
            _warmupSummaryText.text = UiTextCatalog.Warmup(_players.JoinedCount);

            if (_betting != null)
            {
                var lockedCount = _betting.GetSnapshots().Count(snapshot => snapshot.IsLocked);
                _bettingStatusText.text = UiTextCatalog.BettingStatus(lockedCount, _betting.EligiblePlayerCount);
                foreach (var bettingView in _bettingViews)
                {
                    if (_betting.TryGetSnapshot(bettingView.Seat, out var bet))
                    {
                        bettingView.gameObject.SetActive(true);
                        bettingView.Render(bet, _players.GetSnapshot(bettingView.Seat).Coins);
                    }
                    else
                    {
                        bettingView.gameObject.SetActive(false);
                    }
                }

                if (_flow.CurrentState == GameFlowState.RaceCountdown && _raceDirector.CurrentPlan != null)
                    _raceCountdownSummaryText.text = UiTextCatalog.Countdown(_raceDirector.CurrentPlan.Seed.Value, "3");
            }

            if (_flow.CurrentState == GameFlowState.Settlement && _lastSettlement != null)
            {
                _settlementSummaryText.text = BuildSettlementSummary();
                var losers = _rosterService.CurrentRoster.Where(id => id != _lastSettlement.ChampionId);
                _curryResultText.text = UiTextCatalog.Curry(_lastSettlement.ChampionId, string.Join("、", losers));
            }
            if (_flow.CurrentState == GameFlowState.GameOver)
                _gameOverSummaryText.text = BuildGameOverSummary();
        }

        private string BuildSettlementSummary()
        {
            var lines = new System.Collections.Generic.List<string> { UiTextCatalog.RoundChampion(_round, _lastSettlement.ChampionId) };
            foreach (var detail in _lastSettlement.Details)
                lines.Add(UiTextCatalog.Settlement(detail));
            return string.Join("\n", lines);
        }

        private string BuildGameOverSummary()
        {
            var players = _players.GetAllSnapshots().Where(player => player.IsJoined).OrderByDescending(player => player.Coins).ToArray();
            var lines = new System.Collections.Generic.List<string> { UiTextCatalog.GameOverMessage(_gameOverReason, _runtimeMaximumRounds) };
            for (var i = 0; i < players.Length; i++) lines.Add(UiTextCatalog.Ranking(i + 1, players[i]));
            return string.Join("\n", lines);
        }

        public bool TryGetBetSnapshot(PlayerSeat seat, out BetSnapshot snapshot)
        {
            if (_betting == null)
            {
                snapshot = default;
                return false;
            }
            return _betting.TryGetSnapshot(seat, out snapshot);
        }

        public int GetPlayerBalance(PlayerSeat seat)
        {
            return _players.GetSnapshot(seat).Coins;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus) { GameLog.Info("Recovery", $"Application focus restored in {CurrentState}.", round: _round); RefreshAll(); }
        }
    }
}
