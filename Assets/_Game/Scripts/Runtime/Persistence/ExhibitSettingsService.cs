using UnityEngine;

namespace GroundChickenKing.Persistence
{
    public sealed class ExhibitSettingsService
    {
        private const string SettingsKey = "GroundChickenKing.ExhibitSettings.v1";

        public ExhibitSettings Load()
        {
            var json = PlayerPrefs.GetString(SettingsKey, string.Empty);
            var settings = string.IsNullOrWhiteSpace(json) ? new ExhibitSettings() : JsonUtility.FromJson<ExhibitSettings>(json) ?? new ExhibitSettings();
            settings.Sanitize();
            return settings;
        }

        public void Save(ExhibitSettings settings)
        {
            settings.Sanitize();
            PlayerPrefs.SetString(SettingsKey, JsonUtility.ToJson(settings));
            PlayerPrefs.Save();
        }
    }
}
