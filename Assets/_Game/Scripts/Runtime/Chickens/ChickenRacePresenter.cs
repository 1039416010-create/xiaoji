using System;
using System.Linq;
using GroundChickenKing.Race;
using UnityEngine;

namespace GroundChickenKing.Chickens
{
    [DefaultExecutionOrder(100)]
    public sealed class ChickenRacePresenter : MonoBehaviour
    {
        [SerializeField] private ChickenController[] _controllers;
        [SerializeField] private Chicken3DStage _immersiveStage;
        private RacePlan _plan;
        public int ControllerCount => _controllers?.Length ?? 0;
        public void Configure(ChickenController[] controllers)
        {
            _controllers = controllers;
            RebindImmersiveStage();
        }

        private void Awake() => RebindImmersiveStage();

        public void ConfigureImmersiveStage(Chicken3DStage immersiveStage)
        {
            _immersiveStage = immersiveStage;
            RebindImmersiveStage();
        }

        public void RebindImmersiveStage()
        {
            if (_immersiveStage != null && _controllers != null)
                _immersiveStage.BindControllers(_controllers);
        }
        public void BindPlan(RacePlan plan)
        {
            _plan = plan ?? throw new ArgumentNullException(nameof(plan));
            if (_controllers == null || _controllers.Length != 5) throw new InvalidOperationException("Exactly five stable chicken controllers are required.");
            foreach (var controller in _controllers)
                if (!_plan.Chickens.Any(item => item.ChickenId == controller.ChickenId && item.LaneIndex == controller.LaneIndex))
                    throw new InvalidOperationException("Presentation roster does not match race plan.");
            ResetAll();
        }
        public void Play()
        {
            if (_plan == null) throw new InvalidOperationException("Bind a locked plan before playback.");
            foreach (var controller in _controllers) controller.Play(_plan.Chickens.Single(item => item.ChickenId == controller.ChickenId));
        }
        public void ResetAll() { foreach (var controller in _controllers) controller.ResetToStart(); }
        public void CancelAll() { foreach (var controller in _controllers) controller.Cancel(); }
        public void ForceSafeCompletion() { foreach (var controller in _controllers) controller.ForceSafeCompletion(); }
        public void ApplyRoster(System.Collections.Generic.IReadOnlyList<string> roster)
        {
            if (roster == null || roster.Count != 5) throw new ArgumentException("Exactly five roster IDs are required.", nameof(roster));
            for (var lane = 0; lane < _controllers.Length; lane++) { _controllers[lane].ResetToStart(); _controllers[lane].AssignIdentity(roster[lane]); }
            _immersiveStage?.SetRoster(roster);
            _plan = null;
        }

        public void ShowSettlement(string championId) => _immersiveStage?.ShowSettlement(championId);
        public void ClearSettlement() => _immersiveStage?.ClearSettlement();

        private void LateUpdate() => RefreshPairedInterference();

        public void RefreshPairedInterference()
        {
            if (_controllers == null) return;
            for (var i = 0; i < _controllers.Length; i++) _controllers[i].SetPairedInterferenceReaction(false);
            for (var i = 0; i < _controllers.Length; i++)
            {
                var source = _controllers[i];
                if (source.ActiveEventType != RaceEventType.Interfere || !source.ActiveEventTargetLane.HasValue) continue;
                for (var target = 0; target < _controllers.Length; target++)
                    if (_controllers[target].LaneIndex == source.ActiveEventTargetLane.Value)
                        _controllers[target].SetPairedInterferenceReaction(true);
            }
        }
    }
}
