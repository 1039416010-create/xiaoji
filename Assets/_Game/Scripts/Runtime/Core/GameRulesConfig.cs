using System.Collections.Generic;
using UnityEngine;

namespace GroundChickenKing.Core
{
    [CreateAssetMenu(fileName = "CFG_GameRules_Default", menuName = "Ground Chicken King/Config/Game Rules")]
    public sealed class GameRulesConfig : ScriptableObject, IValidatedConfig
    {
        [SerializeField] private int _initialCoins = 50;
        [SerializeField] private int[] _allowedBets = { 1, 5, 10 };
        [SerializeField] private int _minimumPlayers = 1;
        [SerializeField] private int _maximumPlayers = 4;
        [SerializeField] private int _maximumRounds = 20;
        [SerializeField] private float _stateTimeoutSeconds = 30f;

        public int InitialCoins => _initialCoins;
        public IReadOnlyList<int> AllowedBets => _allowedBets;
        public int MinimumPlayers => _minimumPlayers;
        public int MaximumPlayers => _maximumPlayers;
        public int MaximumRounds => _maximumRounds;
        public float StateTimeoutSeconds => _stateTimeoutSeconds;

        public IReadOnlyList<string> CollectValidationErrors()
        {
            var errors = new List<string>();
            if (_initialCoins != 50) errors.Add("InitialCoins must be 50.");
            if (_allowedBets == null || _allowedBets.Length != 3 || _allowedBets[0] != 1 || _allowedBets[1] != 5 || _allowedBets[2] != 10)
                errors.Add("AllowedBets must contain exactly 1, 5, 10 in ascending order.");
            if (_minimumPlayers != 1 || _maximumPlayers != 4) errors.Add("Player range must be 1 through 4.");
            if (_maximumRounds <= 0) errors.Add("MaximumRounds must be positive.");
            if (_stateTimeoutSeconds <= 0f) errors.Add("StateTimeoutSeconds must be positive.");
            return errors;
        }
    }
}
