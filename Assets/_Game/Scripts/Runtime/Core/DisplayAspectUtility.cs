using UnityEngine;

namespace GroundChickenKing.Core
{
    public static class DisplayAspectUtility
    {
        public const float TargetAspect = 2f;
        public static readonly Vector2Int ReferenceResolution = new(1920, 960);

        public static Rect CalculateViewport(int screenWidth, int screenHeight, float targetAspect = TargetAspect)
        {
            if (screenWidth <= 0 || screenHeight <= 0 || targetAspect <= 0f)
                return new Rect(0f, 0f, 1f, 1f);

            var screenAspect = (float)screenWidth / screenHeight;
            if (Mathf.Approximately(screenAspect, targetAspect))
                return new Rect(0f, 0f, 1f, 1f);

            if (screenAspect > targetAspect)
            {
                var width = targetAspect / screenAspect;
                return new Rect((1f - width) * 0.5f, 0f, width, 1f);
            }

            var height = screenAspect / targetAspect;
            return new Rect(0f, (1f - height) * 0.5f, 1f, height);
        }
    }
}
