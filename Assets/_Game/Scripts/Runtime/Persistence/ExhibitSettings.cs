using System;

namespace GroundChickenKing.Persistence
{
    [Serializable]
    public sealed class ExhibitSettings
    {
        public float MasterVolume = 0.8f;
        public float VisualIntensity = 1f;
        public int MaximumRounds = 20;
        public bool Fullscreen = true;

        public ExhibitSettings Copy() => new() { MasterVolume = MasterVolume, VisualIntensity = VisualIntensity, MaximumRounds = MaximumRounds, Fullscreen = Fullscreen };

        public void Sanitize()
        {
            MasterVolume = Math.Clamp(MasterVolume, 0f, 1f);
            VisualIntensity = Math.Clamp(VisualIntensity, 0.55f, 1f);
            MaximumRounds = Math.Clamp(MaximumRounds, 1, 50);
        }
    }
}
