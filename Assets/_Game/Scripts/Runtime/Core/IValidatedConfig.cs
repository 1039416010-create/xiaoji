using System.Collections.Generic;

namespace GroundChickenKing.Core
{
    public interface IValidatedConfig
    {
        IReadOnlyList<string> CollectValidationErrors();
    }
}
