using System.Reflection;
using GroundChickenKing.Core;
using NUnit.Framework;
using UnityEngine;

namespace GroundChickenKing.Tests.EditMode
{
    public sealed class ConfigDefaultsTests
    {
        [Test]
        public void GameRulesConfig_DefaultInstance_HasRequiredEconomyAndPlayerValues()
        {
            var config = ScriptableObject.CreateInstance<GameRulesConfig>();
            Assert.That(config.InitialCoins, Is.EqualTo(50));
            Assert.That(config.AllowedBets, Is.EqualTo(new[] { 1, 5, 10 }));
            Assert.That(config.MinimumPlayers, Is.EqualTo(1));
            Assert.That(config.MaximumPlayers, Is.EqualTo(4));
            Assert.That(config.CollectValidationErrors(), Is.Empty);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void RaceConfig_DefaultInstance_HasFiveLanesAndSafeTimeout()
        {
            var config = ScriptableObject.CreateInstance<RaceConfig>();
            Assert.That(config.LaneCount, Is.EqualTo(5));
            Assert.That(config.HardTimeoutSeconds, Is.GreaterThanOrEqualTo(config.MaximumRaceDurationSeconds));
            Assert.That(config.CollectValidationErrors(), Is.Empty);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void GameRulesConfig_InvalidInitialCoins_ReturnsValidationError()
        {
            var config = ScriptableObject.CreateInstance<GameRulesConfig>();
            typeof(GameRulesConfig).GetField("_initialCoins", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(config, 49);
            Assert.That(config.CollectValidationErrors(), Is.Not.Empty);
            Object.DestroyImmediate(config);
        }
    }
}
