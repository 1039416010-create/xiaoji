namespace GroundChickenKing.Race
{
    public interface IRandomSource
    {
        int NextInt(int minimumInclusive, int maximumExclusive);
        float NextFloat(float minimumInclusive, float maximumInclusive);
    }
}
