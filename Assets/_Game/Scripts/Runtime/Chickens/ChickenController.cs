using System;
using GroundChickenKing.Race;
using UnityEngine;

namespace GroundChickenKing.Chickens
{
    public sealed class ChickenController : MonoBehaviour
    {
        [SerializeField] private string _chickenId;
        [SerializeField] private int _laneIndex;
        [SerializeField] private RectTransform _motionRoot;
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private Animator _animator;
        [SerializeField] private float _startX = -720f;
        [SerializeField] private float _finishX = 720f;
        [SerializeField] private float _laneY;

        private ChickenRacePlan _plan;
        private float _elapsed;
        private bool _isPlaying;
        private int _activeEventIndex = -1;
        private ChickenVisualState _state = ChickenVisualState.Idle;
        private Vector3 _baseVisualScale = Vector3.one;
        private RaceEventType? _activeEventType;
        private int? _activeEventTargetLane;
        private bool _pairedInterferenceActive;

        public string ChickenId => _chickenId;
        public int LaneIndex => _laneIndex;
        public float NormalizedProgress { get; private set; }
        public bool IsPlaying => _isPlaying;
        public ChickenVisualState VisualState => _state;
        public bool UsesRootMotion => _animator != null && _animator.applyRootMotion;
        public RaceEventType? ActiveEventType => _activeEventType;
        public int? ActiveEventTargetLane => _activeEventTargetLane;
        public float ActiveEventPhase { get; private set; }

        public void Configure(string chickenId, int laneIndex, RectTransform motionRoot, Transform visualRoot, Animator animator, float startX, float finishX, float laneY)
        {
            _chickenId = chickenId; _laneIndex = laneIndex; _motionRoot = motionRoot; _visualRoot = visualRoot; _animator = animator;
            _startX = startX; _finishX = finishX; _laneY = laneY;
            if (_visualRoot != null) _baseVisualScale = _visualRoot.localScale;
            if (_animator != null) _animator.applyRootMotion = false;
            ResetToStart();
        }

        private void Awake()
        {
            if (_visualRoot != null) _baseVisualScale = _visualRoot.localScale;
            if (_animator != null) _animator.applyRootMotion = false;
        }

        public void Play(ChickenRacePlan plan)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (plan.ChickenId != _chickenId || plan.LaneIndex != _laneIndex) throw new ArgumentException("Chicken plan identity or lane does not match controller.", nameof(plan));
            _plan = plan; _elapsed = 0f; _activeEventIndex = -1; _activeEventType = null; _activeEventTargetLane = null; ActiveEventPhase = 0f; _pairedInterferenceActive = false; _isPlaying = true;
            SetProgress(plan.Segments[0].StartProgress); SetState(ChickenVisualState.Run);
        }

        public void AssignIdentity(string chickenId)
        {
            if (_isPlaying) throw new InvalidOperationException("Cannot replace chicken identity during playback.");
            if (string.IsNullOrWhiteSpace(chickenId)) throw new ArgumentException("Chicken ID is required.", nameof(chickenId));
            _chickenId = chickenId;
            if (_visualRoot != null)
            {
                var label = _visualRoot.Find("Name")?.GetComponent<UnityEngine.UI.Text>();
                if (label != null) label.text = chickenId.Replace("chicken-", string.Empty);
            }
        }

        public void Advance(float unscaledDeltaTime)
        {
            if (unscaledDeltaTime < 0f) throw new ArgumentOutOfRangeException(nameof(unscaledDeltaTime));
            if (!_isPlaying || _plan == null) return;
            _elapsed += unscaledDeltaTime;
            SetProgress(SampleProgress(Mathf.Min(_elapsed, _plan.Segments[_plan.Segments.Count - 1].EndTime)));
            UpdateEventState();
            if (_elapsed < _plan.Segments[_plan.Segments.Count - 1].EndTime) return;
            _isPlaying = false; SetProgress(_plan.ExpectedEndProgress);
            SetState(_plan.IsFinisher ? ChickenVisualState.Celebrate : ChickenVisualState.Lose);
        }

        public void Cancel()
        {
            _isPlaying = false; _plan = null; _activeEventIndex = -1; _activeEventType = null; _activeEventTargetLane = null; ActiveEventPhase = 0f; _pairedInterferenceActive = false; SetState(ChickenVisualState.Idle); RestoreVisualPose();
        }

        public void ResetToStart()
        {
            _isPlaying = false; _plan = null; _elapsed = 0f; _activeEventIndex = -1; _activeEventType = null; _activeEventTargetLane = null; ActiveEventPhase = 0f; _pairedInterferenceActive = false; SetProgress(0f); SetState(ChickenVisualState.Idle); RestoreVisualPose();
        }

        public void ForceSafeCompletion()
        {
            if (_plan == null) { ResetToStart(); return; }
            _isPlaying = false; SetProgress(_plan.ExpectedEndProgress); SetState(_plan.IsFinisher ? ChickenVisualState.Celebrate : ChickenVisualState.Lose); RestoreVisualPose();
        }

        private void Update() => Advance(Time.unscaledDeltaTime);

        public void SetPairedInterferenceReaction(bool active)
        {
            if (!_isPlaying) return;
            if (_pairedInterferenceActive == active)
            {
                if (active && _visualRoot != null) _visualRoot.localRotation = Quaternion.Euler(0f, 0f, -12f);
                return;
            }
            _pairedInterferenceActive = active;
            RestoreVisualPose();
            if (active)
            {
                SetState(ChickenVisualState.Interfere);
                if (_visualRoot != null) _visualRoot.localRotation = Quaternion.Euler(0f, 0f, -12f);
            }
            else
            {
                SetState(_activeEventType.HasValue ? Map(_activeEventType.Value) : ChickenVisualState.Run);
            }
        }

        private float SampleProgress(float time)
        {
            for (var i = 0; i < _plan.Segments.Count; i++)
            {
                var segment = _plan.Segments[i];
                if (time > segment.EndTime && i < _plan.Segments.Count - 1) continue;
                var t = Mathf.InverseLerp(segment.StartTime, segment.EndTime, time);
                if (segment.Interpolation == RaceInterpolation.EaseInOut) t = t * t * (3f - 2f * t);
                return Mathf.Lerp(segment.StartProgress, segment.EndProgress, t);
            }
            return _plan.ExpectedEndProgress;
        }

        private void UpdateEventState()
        {
            var found = -1;
            for (var i = 0; i < _plan.Events.Count; i++)
                if (_elapsed >= _plan.Events[i].StartTime && _elapsed < _plan.Events[i].EndTime) { found = i; break; }
            ActiveEventPhase = found < 0 ? 0f : Mathf.InverseLerp(_plan.Events[found].StartTime, _plan.Events[found].EndTime, _elapsed);
            if (found == _activeEventIndex)
            {
                if (found >= 0 && _plan.Events[found].Type == RaceEventType.Trip)
                    SetState(ActiveEventPhase < 0.52f ? ChickenVisualState.Fall : ChickenVisualState.Recover);
                ApplyFallbackPose(found);
                return;
            }
            _activeEventIndex = found;
            _activeEventType = found < 0 ? null : _plan.Events[found].Type;
            _activeEventTargetLane = found < 0 ? null : _plan.Events[found].TargetLane;
            if (_pairedInterferenceActive) return;
            SetState(found < 0 ? ChickenVisualState.Run : _plan.Events[found].Type == RaceEventType.Trip && ActiveEventPhase >= 0.52f ? ChickenVisualState.Recover : Map(_plan.Events[found].Type));
            RestoreVisualPose(); ApplyFallbackPose(found);
        }

        private void ApplyFallbackPose(int eventIndex)
        {
            if (_visualRoot == null || eventIndex < 0) return;
            var raceEvent = _plan.Events[eventIndex];
            var phase = Mathf.InverseLerp(raceEvent.StartTime, raceEvent.EndTime, _elapsed);
            switch (raceEvent.Type)
            {
                case RaceEventType.Sprint: _visualRoot.localScale = _baseVisualScale * (1f + Mathf.Sin(phase * Mathf.PI) * 0.18f); break;
                case RaceEventType.Pause: case RaceEventType.Slowdown: _visualRoot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(phase * Mathf.PI * 2f) * 5f); break;
                case RaceEventType.Trip: _visualRoot.localRotation = Quaternion.Euler(0f, 0f, -75f * Mathf.Sin(phase * Mathf.PI)); break;
                case RaceEventType.TurnAround: _visualRoot.localScale = new Vector3((phase < 0.5f ? -1f : 1f) * Mathf.Abs(_baseVisualScale.x), _baseVisualScale.y, _baseVisualScale.z); break;
                case RaceEventType.Interfere: _visualRoot.localRotation = Quaternion.Euler(0f, 0f, 18f * Mathf.Sin(phase * Mathf.PI)); break;
            }
        }

        private void SetProgress(float progress)
        {
            NormalizedProgress = Mathf.Clamp01(progress);
            if (_motionRoot != null) _motionRoot.anchoredPosition = new Vector2(Mathf.Lerp(_startX, _finishX, NormalizedProgress), _laneY);
        }

        private void SetState(ChickenVisualState state)
        {
            _state = state;
            if (_animator == null || _animator.runtimeAnimatorController == null) return;
            var hash = AnimatorHash(state);
            if (_animator.HasState(0, hash)) _animator.CrossFade(hash, 0.08f, 0);
        }

        private void RestoreVisualPose(bool restoreScale = true)
        {
            if (_visualRoot == null) return;
            _visualRoot.localRotation = Quaternion.identity;
            if (restoreScale) _visualRoot.localScale = _baseVisualScale;
        }

        private static ChickenVisualState Map(RaceEventType type) => type switch
        {
            RaceEventType.Sprint => ChickenVisualState.Sprint,
            RaceEventType.Slowdown => ChickenVisualState.Stop,
            RaceEventType.Pause => ChickenVisualState.Stop,
            RaceEventType.Trip => ChickenVisualState.Fall,
            RaceEventType.TurnAround => ChickenVisualState.Turn,
            RaceEventType.Interfere => ChickenVisualState.Interfere,
            _ => ChickenVisualState.Run,
        };

        private static int AnimatorHash(ChickenVisualState state) => state switch
        {
            ChickenVisualState.Idle => ChickenAnimatorHashes.Idle, ChickenVisualState.Warmup => ChickenAnimatorHashes.Warmup,
            ChickenVisualState.Run => ChickenAnimatorHashes.Run, ChickenVisualState.Sprint => ChickenAnimatorHashes.Sprint,
            ChickenVisualState.Stop => ChickenAnimatorHashes.Stop, ChickenVisualState.Fall => ChickenAnimatorHashes.Fall,
            ChickenVisualState.Recover => ChickenAnimatorHashes.Recover, ChickenVisualState.Turn => ChickenAnimatorHashes.Turn,
            ChickenVisualState.Interfere => ChickenAnimatorHashes.Interfere, ChickenVisualState.Celebrate => ChickenAnimatorHashes.Celebrate,
            _ => ChickenAnimatorHashes.Lose,
        };
    }
}
