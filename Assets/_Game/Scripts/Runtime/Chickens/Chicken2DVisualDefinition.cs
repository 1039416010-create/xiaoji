using System;
using UnityEngine;

namespace GroundChickenKing.Chickens
{
    [CreateAssetMenu(menuName = "Ground Chicken King/Chicken 2D Visual Definition", fileName = "CFG_Chicken2D_New")]
    public sealed class Chicken2DVisualDefinition : ScriptableObject
    {
        [SerializeField] private string _chickenId;
        [SerializeField] private GameObject _prefab;
        [SerializeField] private AnimatorOverrideController _animatorOverride;
        [SerializeField] private Sprite _portrait;
        [SerializeField] private float _stageScale = 1f;
        [SerializeField] private float _footBaseline;

        public string ChickenId => _chickenId;
        public GameObject Prefab => _prefab;
        public AnimatorOverrideController AnimatorOverride => _animatorOverride;
        public Sprite Portrait => _portrait;
        public float StageScale => _stageScale;
        public float FootBaseline => _footBaseline;

        public void ValidateOrThrow()
        {
            if (string.IsNullOrWhiteSpace(_chickenId) || !_chickenId.StartsWith("chicken-", StringComparison.Ordinal))
                throw new InvalidOperationException($"{name}: chicken ID must start with 'chicken-'.");
            if (_stageScale <= 0f)
                throw new InvalidOperationException($"{name}: stage scale must be positive.");
        }

        private void OnValidate()
        {
            _stageScale = Mathf.Max(0.01f, _stageScale);
        }
    }
}
