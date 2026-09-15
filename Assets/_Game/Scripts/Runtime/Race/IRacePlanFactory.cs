using System.Collections.Generic;

namespace GroundChickenKing.Race
{
    public interface IRacePlanFactory
    {
        RacePlan Create(int round, RaceSeed seed, IReadOnlyList<string> chickenIds, RaceRules rules);
    }
}
