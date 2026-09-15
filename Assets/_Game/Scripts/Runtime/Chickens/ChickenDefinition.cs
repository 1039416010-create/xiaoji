using System.Collections.Generic;
using GroundChickenKing.Core;
using UnityEngine;

namespace GroundChickenKing.Chickens
{
    [CreateAssetMenu(fileName = "CFG_Chicken_New", menuName = "Ground Chicken King/Config/Chicken")]
    public sealed class ChickenDefinition : ScriptableObject, IValidatedConfig
    {
        [SerializeField] private string _stableId;
        [SerializeField] private string _displayName;
        [SerializeField] private string _personalityTag;
        [SerializeField] private GameObject _prefab;
        [SerializeField] private Color _bodyColor = Color.white;
        [SerializeField] private Vector2 _bodyScale = Vector2.one;
        [SerializeField] private bool _supportsSprint = true;
        [SerializeField] private bool _supportsFall = true;
        [SerializeField] private bool _supportsTurn = true;
        [SerializeField] private bool _supportsInterfere = true;

        public string StableId => _stableId;
        public string DisplayName => _displayName;
        public string PersonalityTag => _personalityTag;
        public GameObject Prefab => _prefab;
        public Color BodyColor => _bodyColor;
        public Vector2 BodyScale => _bodyScale;

        public IReadOnlyList<string> CollectValidationErrors()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(_stableId)) errors.Add("StableId is required.");
            if (string.IsNullOrWhiteSpace(_displayName)) errors.Add("DisplayName is required.");
            if (string.IsNullOrWhiteSpace(_personalityTag)) errors.Add("PersonalityTag is required.");
            if (_prefab == null) errors.Add("Prefab is required.");
            if (_bodyScale.x <= 0f || _bodyScale.y <= 0f) errors.Add("BodyScale must be positive.");
            if (!_supportsSprint || !_supportsFall || !_supportsTurn || !_supportsInterfere) errors.Add("Phase 05 definitions must support all required fallback behaviors.");
            return errors;
        }

#if UNITY_EDITOR
        public void ConfigureEditor(string stableId, string displayName, string personalityTag, GameObject prefab, Color bodyColor, Vector2 bodyScale)
        {
            _stableId = stableId; _displayName = displayName; _personalityTag = personalityTag; _prefab = prefab;
            _bodyColor = bodyColor; _bodyScale = bodyScale;
        }
#endif
    }
}
