using System;
using UnityEngine;
using UnityEngine.UI;

namespace GroundChickenKing.UI
{
    public sealed class SettlementCelebrationView : MonoBehaviour
    {
        [SerializeField] private Text _championText;
        [SerializeField] private Text _loserCaption;
        [SerializeField] private RawImage[] _curryMeals;
        [SerializeField] private RectTransform[] _confetti;
        private Vector2[] _mealPositions = Array.Empty<Vector2>();
        private Vector2[] _confettiStarts = Array.Empty<Vector2>();
        private float _startedAt;
        private bool _isPresenting;

        public bool IsPresenting => _isPresenting;

        public void Configure(Text championText, Text loserCaption, RawImage[] curryMeals, RectTransform[] confetti)
        {
            _championText = championText;
            _loserCaption = loserCaption;
            _curryMeals = curryMeals ?? Array.Empty<RawImage>();
            _confetti = confetti ?? Array.Empty<RectTransform>();
            CaptureBasePositions();
        }

        private void Awake() => CaptureBasePositions();

        private void CaptureBasePositions()
        {
            _curryMeals ??= Array.Empty<RawImage>();
            _confetti ??= Array.Empty<RectTransform>();
            _mealPositions = new Vector2[_curryMeals.Length];
            for (var i = 0; i < _curryMeals.Length; i++)
                _mealPositions[i] = MealPosition(i);
            _confettiStarts = new Vector2[_confetti.Length];
            for (var i = 0; i < _confetti.Length; i++)
                _confettiStarts[i] = _confetti[i].anchoredPosition;
        }

        public void Present(string championDisplayName, string loserDisplayNames)
        {
            _championText.text = $"冠军：{championDisplayName}";
            _loserCaption.text = $"其余选手变成了今日特餐：{loserDisplayNames}";
            _startedAt = Time.unscaledTime;
            _isPresenting = true;
            for (var i = 0; i < _curryMeals.Length; i++)
                _curryMeals[i].gameObject.SetActive(true);
        }

        public void Clear()
        {
            _isPresenting = false;
            for (var i = 0; i < _curryMeals.Length; i++)
            {
                _curryMeals[i].rectTransform.anchoredPosition = _mealPositions.Length > i ? _mealPositions[i] : Vector2.zero;
                _curryMeals[i].rectTransform.localScale = Vector3.one;
            }
        }

        private void Update()
        {
            if (!_isPresenting || !gameObject.activeInHierarchy)
                return;
            var elapsed = Time.unscaledTime - _startedAt;
            for (var i = 0; i < _curryMeals.Length; i++)
            {
                var reveal = Mathf.Clamp01((elapsed - .35f - i * .18f) / .42f);
                var bounce = 1f + Mathf.Sin(reveal * Mathf.PI) * .22f;
                _curryMeals[i].rectTransform.localScale = Vector3.one * reveal * bounce;
                var basePosition = MealPosition(i);
                _curryMeals[i].rectTransform.anchoredPosition = basePosition + Vector2.up * (Mathf.Sin(elapsed * 2.4f + i) * 5f);
            }
            for (var i = 0; i < _confetti.Length; i++)
            {
                var phase = Mathf.Repeat(elapsed * (.16f + i % 4 * .018f) + i * .083f, 1f);
                var start = i < _confettiStarts.Length ? _confettiStarts[i] : _confetti[i].anchoredPosition;
                _confetti[i].anchoredPosition = new Vector2(start.x + Mathf.Sin(elapsed * 1.8f + i) * 38f, Mathf.Lerp(470f, -470f, phase));
                _confetti[i].localRotation = Quaternion.Euler(0f, 0f, elapsed * (70f + i * 11f));
            }
        }

        private static Vector2 MealPosition(int index) => new(-510f + index * 340f, -290f);
    }
}
