using System.Collections.Generic;
using GroundChickenKing.Core;
using UnityEngine;

namespace GroundChickenKing.Chickens
{
    [CreateAssetMenu(fileName = "CFG_ChickenRosterCatalog", menuName = "Ground Chicken King/Config/Chicken Roster Catalog")]
    public sealed class ChickenRosterCatalogConfig : ScriptableObject, IValidatedConfig
    {
        [SerializeField] private ChickenDefinition[] _definitions;
        public IReadOnlyList<ChickenDefinition> Definitions => _definitions;
        public IReadOnlyList<string> CollectValidationErrors()
        {
            var errors = new List<string>();
            if (_definitions == null || _definitions.Length < 9) { errors.Add("At least nine chicken definitions are required to replace four losers."); return errors; }
            var ids = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (var definition in _definitions)
            {
                if (definition == null) errors.Add("Catalog contains a missing definition.");
                else if (!ids.Add(definition.StableId)) errors.Add($"Duplicate chicken ID: {definition.StableId}.");
            }
            return errors;
        }
#if UNITY_EDITOR
        public void ConfigureEditor(ChickenDefinition[] definitions) => _definitions = definitions;
#endif
    }
}
