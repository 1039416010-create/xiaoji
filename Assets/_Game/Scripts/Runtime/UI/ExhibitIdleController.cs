using GroundChickenKing.Flow;
using UnityEngine;
using UnityEngine.UI;

namespace GroundChickenKing.UI
{
    public sealed class ExhibitIdleController : MonoBehaviour
    {
        [SerializeField] private GameSessionPresenter _session;
        [SerializeField] private float _idleSeconds = 120f;
        private float _lastInteraction;

        public void Configure(GameSessionPresenter session, float idleSeconds) { _session = session; _idleSeconds = Mathf.Max(30f, idleSeconds); }
        private void Start()
        {
            _lastInteraction = Time.unscaledTime;
            foreach (var button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None)) button.onClick.AddListener(NotifyInteraction);
        }
        private void Update()
        {
            if (_session == null || Time.unscaledTime - _lastInteraction < _idleSeconds) return;
            if (_session.CurrentState is GameFlowState.RaceCountdown or GameFlowState.Racing) return;
            _lastInteraction = Time.unscaledTime; _session.ReturnHomeForIdle();
        }
        public void NotifyInteraction() => _lastInteraction = Time.unscaledTime;
    }
}
