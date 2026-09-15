using GroundChickenKing.Core;
using UnityEngine;

namespace GroundChickenKing.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class AspectRatioController : MonoBehaviour
    {
        private Camera _camera;
        private int _lastWidth;
        private int _lastHeight;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            Apply();
        }

        private void LateUpdate()
        {
            if (_lastWidth != Screen.width || _lastHeight != Screen.height)
                Apply();
        }

        private void Apply()
        {
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
            _camera.rect = DisplayAspectUtility.CalculateViewport(_lastWidth, _lastHeight);
        }
    }
}
