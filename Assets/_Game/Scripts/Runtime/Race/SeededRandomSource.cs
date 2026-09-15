using System;

namespace GroundChickenKing.Race
{
    public sealed class SeededRandomSource : IRandomSource
    {
        private readonly Random _random;
        public SeededRandomSource(RaceSeed seed) => _random = new Random(seed.Value);
        public int NextInt(int minimumInclusive, int maximumExclusive) => _random.Next(minimumInclusive, maximumExclusive);
        public float NextFloat(float minimumInclusive, float maximumInclusive) => minimumInclusive + (float)_random.NextDouble() * (maximumInclusive - minimumInclusive);
    }
}
