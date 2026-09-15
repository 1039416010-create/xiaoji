using GroundChickenKing.Persistence;
using NUnit.Framework;

namespace GroundChickenKing.Tests.EditMode
{
    public sealed class ExhibitSettingsTests
    {
        [Test]
        public void Sanitize_OutOfRangeValues_ClampsToPublicExhibitLimits()
        {
            var settings = new ExhibitSettings { MasterVolume = 2f, VisualIntensity = 0.1f, MaximumRounds = 100 };
            settings.Sanitize();
            Assert.That(settings.MasterVolume, Is.EqualTo(1f));
            Assert.That(settings.VisualIntensity, Is.EqualTo(0.55f));
            Assert.That(settings.MaximumRounds, Is.EqualTo(50));
        }

        [Test]
        public void Copy_ChangedDraft_DoesNotMutateSavedSettings()
        {
            var saved = new ExhibitSettings { MasterVolume = 0.8f, VisualIntensity = 0.9f, MaximumRounds = 20 };
            var draft = saved.Copy(); draft.MaximumRounds = 5;
            Assert.That(saved.MaximumRounds, Is.EqualTo(20));
        }
    }
}
