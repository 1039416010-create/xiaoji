using System.Collections.Generic;
using UnityEngine;

namespace GroundChickenKing.Core
{
    [CreateAssetMenu(fileName = "CFG_Race_Default", menuName = "Ground Chicken King/Config/Race")]
    public sealed class RaceConfig : ScriptableObject, IValidatedConfig
    {
        [SerializeField] private int _laneCount = 5;
        [SerializeField] private float _startProgress;
        [SerializeField] private float _finishProgress = 1f;
        [SerializeField] private float _minimumRaceDurationSeconds = 12f;
        [SerializeField] private float _maximumRaceDurationSeconds = 30f;
        [SerializeField] private float _minimumSpeed = 0.02f;
        [SerializeField] private float _maximumSpeed = 0.25f;
        [SerializeField] private float _minimumEventIntervalSeconds = 0.5f;
        [SerializeField] private float _finishTolerance = 0.002f;
        [SerializeField] private int _generationRetryLimit = 20;
        [SerializeField] private float _hardTimeoutSeconds = 40f;

        public int LaneCount => _laneCount;
        public float StartProgress => _startProgress;
        public float FinishProgress => _finishProgress;
        public float MinimumRaceDurationSeconds => _minimumRaceDurationSeconds;
        public float MaximumRaceDurationSeconds => _maximumRaceDurationSeconds;
        public float MinimumSpeed => _minimumSpeed;
        public float MaximumSpeed => _maximumSpeed;
        public float MinimumEventIntervalSeconds => _minimumEventIntervalSeconds;
        public float FinishTolerance => _finishTolerance;
        public int GenerationRetryLimit => _generationRetryLimit;
        public float HardTimeoutSeconds => _hardTimeoutSeconds;

        public IReadOnlyList<string> CollectValidationErrors()
        {
            var errors = new List<string>();
            if (_laneCount != 5) errors.Add("LaneCount must be 5.");
            if (_startProgress < 0f || _startProgress >= _finishProgress) errors.Add("StartProgress must be non-negative and before FinishProgress.");
            if (_finishProgress > 1f) errors.Add("FinishProgress cannot exceed 1.");
            if (_minimumRaceDurationSeconds <= 0f || _maximumRaceDurationSeconds < _minimumRaceDurationSeconds)
                errors.Add("Race duration range is invalid.");
            if (_minimumSpeed <= 0f || _maximumSpeed < _minimumSpeed) errors.Add("Speed range is invalid.");
            if (_minimumEventIntervalSeconds < 0f) errors.Add("MinimumEventIntervalSeconds cannot be negative.");
            if (_finishTolerance < 0f || _finishTolerance >= 0.1f) errors.Add("FinishTolerance must be in [0, 0.1). ");
            if (_generationRetryLimit <= 0) errors.Add("GenerationRetryLimit must be positive.");
            if (_hardTimeoutSeconds < _maximumRaceDurationSeconds) errors.Add("HardTimeoutSeconds must cover MaximumRaceDurationSeconds.");
            return errors;
        }
    }
}
