using System.Collections.Generic;
using UnityEngine;

namespace GroundChickenKing.Core
{
    [CreateAssetMenu(fileName = "CFG_Touch_Default", menuName = "Ground Chicken King/Config/Touch")]
    public sealed class TouchConfig : ScriptableObject, IValidatedConfig
    {
        [SerializeField] private float _minimumButtonSizeMillimeters = 25f;
        [SerializeField] private float _edgeSafeAreaMillimeters = 15f;
        [SerializeField] private float _repeatClickCooldownSeconds = 0.2f;
        [SerializeField] private int _maximumConcurrentTouches = 10;

        public float MinimumButtonSizeMillimeters => _minimumButtonSizeMillimeters;
        public float EdgeSafeAreaMillimeters => _edgeSafeAreaMillimeters;
        public float RepeatClickCooldownSeconds => _repeatClickCooldownSeconds;
        public int MaximumConcurrentTouches => _maximumConcurrentTouches;

        public IReadOnlyList<string> CollectValidationErrors()
        {
            var errors = new List<string>();
            if (_minimumButtonSizeMillimeters < 20f) errors.Add("MinimumButtonSizeMillimeters must be at least 20 mm.");
            if (_edgeSafeAreaMillimeters < 0f) errors.Add("EdgeSafeAreaMillimeters cannot be negative.");
            if (_repeatClickCooldownSeconds < 0f) errors.Add("RepeatClickCooldownSeconds cannot be negative.");
            if (_maximumConcurrentTouches < 4) errors.Add("MaximumConcurrentTouches must support at least four players.");
            return errors;
        }
    }
}
