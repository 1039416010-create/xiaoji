using UnityEngine;

namespace GroundChickenKing.Chickens
{
    public static class ChickenAnimatorHashes
    {
        public static readonly int Idle = Animator.StringToHash("Base Layer.Idle");
        public static readonly int Warmup = Animator.StringToHash("Base Layer.Warmup");
        public static readonly int Run = Animator.StringToHash("Base Layer.Run");
        public static readonly int Sprint = Animator.StringToHash("Base Layer.Sprint");
        public static readonly int Stop = Animator.StringToHash("Base Layer.Stop");
        public static readonly int Fall = Animator.StringToHash("Base Layer.Fall");
        public static readonly int Recover = Animator.StringToHash("Base Layer.Recover");
        public static readonly int Turn = Animator.StringToHash("Base Layer.Turn");
        public static readonly int Interfere = Animator.StringToHash("Base Layer.Interfere");
        public static readonly int Celebrate = Animator.StringToHash("Base Layer.Celebrate");
        public static readonly int Lose = Animator.StringToHash("Base Layer.Lose");
    }
}
