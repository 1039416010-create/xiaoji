using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GroundChickenKing.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class TouchButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private float _cooldownSeconds = 0.2f;
        private Button _button;
        private Vector3 _restScale;
        private Coroutine _cooldown;

        private void Awake()
        {
            _button = GetComponent<Button>(); _restScale = transform.localScale;
            _button.navigation = new Navigation { mode = Navigation.Mode.None };
        }

        public void OnPointerDown(PointerEventData eventData) { if (_button.interactable) transform.localScale = _restScale * 0.94f; }
        public void OnPointerUp(PointerEventData eventData) => transform.localScale = _restScale;
        public void OnPointerExit(PointerEventData eventData) => transform.localScale = _restScale;

        public void OnPointerClick(PointerEventData eventData)
        {
            transform.localScale = _restScale;
            if (_cooldown == null && isActiveAndEnabled) _cooldown = StartCoroutine(Cooldown());
        }

        private IEnumerator Cooldown()
        {
            var wasInteractable = _button.interactable; _button.interactable = false;
            yield return new WaitForSecondsRealtime(_cooldownSeconds);
            if (wasInteractable) _button.interactable = true;
            _cooldown = null;
        }
    }
}
