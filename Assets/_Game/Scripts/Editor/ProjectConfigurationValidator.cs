using System.Collections.Generic;
using GroundChickenKing.Core;
using UnityEditor;
using UnityEngine;

namespace GroundChickenKing.Editor
{
    public static class ProjectConfigurationValidator
    {
        [MenuItem("Ground Chicken King/Validate Project Configuration")]
        public static void ValidateFromMenu()
        {
            var errors = ValidateAllConfigs();
            if (errors.Count == 0)
            {
                Debug.Log("[Configuration] All Ground Chicken King configuration assets are valid.");
                return;
            }

            foreach (var error in errors)
                Debug.LogError(error);
        }

        public static IReadOnlyList<string> ValidateAllConfigs()
        {
            var errors = new List<string>();
            var guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/_Game/Config" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) is not IValidatedConfig config)
                    continue;

                foreach (var validationError in config.CollectValidationErrors())
                    errors.Add($"[Configuration] {path}: {validationError}");
            }

            return errors;
        }
    }
}
