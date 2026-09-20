using UnityEngine;

namespace GroundChickenKing.Chickens
{
    /// <summary>
    /// Central transition rules shared by 2D character animator controllers.
    /// Locomotion states keep their cycle phase so feet do not pop when a
    /// planned sprint starts or ends. One-shot actions start from frame zero.
    /// </summary>
    public static class ChickenAnimationTransitionPolicy
    {
        public static float GetDuration(ChickenVisualState from, ChickenVisualState to)
        {
            if (from == to)
                return 0f;
            if (IsLocomotion(from) && IsLocomotion(to))
                return 0.1f;
            if (to == ChickenVisualState.Fall)
                return 0.035f;
            if (from == ChickenVisualState.Fall && to == ChickenVisualState.Recover)
                return 0.055f;
            if (to == ChickenVisualState.Turn || to == ChickenVisualState.Interfere)
                return 0.065f;
            if (to == ChickenVisualState.Celebrate || to == ChickenVisualState.Lose)
                return 0.12f;
            return 0.08f;
        }

        public static bool ShouldPreservePhase(ChickenVisualState from, ChickenVisualState to)
        {
            return from != to && IsLocomotion(from) && IsLocomotion(to);
        }

        private static bool IsLocomotion(ChickenVisualState state)
        {
            return state == ChickenVisualState.Run || state == ChickenVisualState.Sprint;
        }
    }
}
