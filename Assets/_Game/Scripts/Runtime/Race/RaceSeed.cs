using System;

namespace GroundChickenKing.Race
{
    [Serializable]
    public readonly struct RaceSeed : IEquatable<RaceSeed>
    {
        public RaceSeed(int value) => Value = value;
        public int Value { get; }
        public bool Equals(RaceSeed other) => Value == other.Value;
        public override bool Equals(object obj) => obj is RaceSeed other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => Value.ToString();
    }
}
