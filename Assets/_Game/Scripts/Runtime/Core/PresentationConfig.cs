using System.Collections.Generic;
using UnityEngine;

namespace GroundChickenKing.Core
{
    [CreateAssetMenu(fileName = "CFG_Presentation_Default", menuName = "Ground Chicken King/Config/Presentation")]
    public sealed class PresentationConfig : ScriptableObject, IValidatedConfig
    {
        [SerializeField] private float _warmupSeconds = 8f;
        [SerializeField] private float _countdownStepSeconds = 1f;
        [SerializeField] private float _settlementSeconds = 6f;
        [SerializeField] private float _rosterRefreshSeconds = 3f;
        [SerializeField] private float _uiTransitionSeconds = 0.25f;

        public float WarmupSeconds => _warmupSeconds;
        public float CountdownStepSeconds => _countdownStepSeconds;
        public float SettlementSeconds => _settlementSeconds;
        public float RosterRefreshSeconds => _rosterRefreshSeconds;
        public float UiTransitionSeconds => _uiTransitionSeconds;

        public IReadOnlyList<string> CollectValidationErrors()
        {
            var errors = new List<string>();
            if (_warmupSeconds <= 0f) errors.Add("WarmupSeconds must be positive.");
            if (_countdownStepSeconds <= 0f) errors.Add("CountdownStepSeconds must be positive.");
            if (_settlementSeconds <= 0f) errors.Add("SettlementSeconds must be positive.");
            if (_rosterRefreshSeconds < 0f) errors.Add("RosterRefreshSeconds cannot be negative.");
            if (_uiTransitionSeconds < 0f) errors.Add("UiTransitionSeconds cannot be negative.");
            return errors;
        }
    }
}
