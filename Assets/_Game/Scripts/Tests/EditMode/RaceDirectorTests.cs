using System.Collections.Generic;
using System.Linq;
using GroundChickenKing.Race;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;

namespace GroundChickenKing.Tests.EditMode
{
    public sealed class RaceDirectorTests
    {
        private static readonly string[] Roster = { "flash", "chubby", "tiny", "bro", "slacker" };
        private static RaceRules Rules => new(5, 0f, 1f, 12f, 30f, 0.5f, 0.002f, 3, 40f);

        [Test]
        public void TryGeneratePlan_BeforeBetsLocked_IsRejectedAndKeepsPlanNull()
        {
            var director = new RaceDirector(Roster, Rules);
            Assert.That(director.TryGeneratePlan(1, new RaceSeed(42), out var plan, out var error), Is.False);
            Assert.That(plan, Is.Null); Assert.That(director.CurrentPlan, Is.Null); Assert.That(error, Does.Contain("locked"));
        }

        [Test]
        public void TryGeneratePlan_CalledTwice_GeneratesExactlyOnce()
        {
            var director = new RaceDirector(Roster, Rules); var count = 0;
            director.PlanGenerated += _ => count++;
            director.NotifyAllBetsLocked();
            Assert.That(director.TryGeneratePlan(1, new RaceSeed(42), out var first, out _), Is.True);
            Assert.That(director.TryGeneratePlan(1, new RaceSeed(43), out _, out _), Is.False);
            Assert.That(count, Is.EqualTo(1)); Assert.That(director.CurrentPlan, Is.SameAs(first));
        }

        [Test]
        public void Generator_TenThousandSeeds_AllPlansAreValidWithOneChampion()
        {
            var generator = new RacePlanGenerator(); var validator = new RacePlanValidator();
            for (var seed = 0; seed < 10000; seed++)
            {
                var plan = generator.Create(1, new RaceSeed(seed), Roster, Rules);
                var validation = validator.Validate(plan, Rules);
                Assert.That(validation.IsValid, Is.True, $"seed={seed}: {string.Join(";", validation.Errors)}");
                Assert.That(plan.Chickens.Count(chicken => chicken.IsFinisher), Is.EqualTo(1), $"seed={seed}");
                Assert.That(plan.Chickens.Single(chicken => chicken.IsFinisher).ChickenId, Is.EqualTo(plan.ChampionId));
            }
        }

        [Test]
        public void Generator_SameSeedRosterAndConfig_ReproducesCorePlan()
        {
            var generator = new RacePlanGenerator();
            var first = generator.Create(3, new RaceSeed(-1942), Roster, Rules);
            var second = generator.Create(3, new RaceSeed(-1942), Roster, Rules);
            Assert.That(second.PlanId, Is.EqualTo(first.PlanId)); Assert.That(second.ChampionId, Is.EqualTo(first.ChampionId));
            Assert.That(second.TotalDuration, Is.EqualTo(first.TotalDuration));
            for (var i = 0; i < 5; i++)
            {
                Assert.That(second.Chickens[i].ExpectedEndProgress, Is.EqualTo(first.Chickens[i].ExpectedEndProgress));
                Assert.That(second.Chickens[i].Segments.Select(x => (x.StartTime, x.EndTime, x.StartProgress, x.EndProgress)),
                    Is.EqualTo(first.Chickens[i].Segments.Select(x => (x.StartTime, x.EndTime, x.StartProgress, x.EndProgress))));
            }
        }

        [Test]
        public void Director_DifferentExternalBetSnapshots_SameInputsRemainIdentical()
        {
            var betsA = new[] { "flash:10", "bro:1" };
            var betsB = new[] { "slacker:1", "tiny:10" };
            Assert.That(betsA, Is.Not.EqualTo(betsB));
            var first = Generate(9981); var second = Generate(9981);
            Assert.That(second.PlanId, Is.EqualTo(first.PlanId));
            Assert.That(second.ChampionId, Is.EqualTo(first.ChampionId));
        }

        [Test]
        public void Director_InvalidGenerator_UsesValidatedFallbackAfterRetryLimit()
        {
            var invalid = new InvalidFactory();
            var director = new RaceDirector(Roster, Rules, invalid);
            director.NotifyAllBetsLocked();
            LogAssert.Expect(LogType.Error, new Regex("Generator exhausted 3 attempts"));
            Assert.That(director.TryGeneratePlan(2, new RaceSeed(7), out var plan, out _), Is.True);
            Assert.That(invalid.CallCount, Is.EqualTo(Rules.RetryLimit));
            Assert.That(new RacePlanValidator().Validate(plan, Rules).IsValid, Is.True);
        }

        [Test]
        public void Playback_PresentationStalled_CompletesAtHardTimeoutWithPlannedChampion()
        {
            var plan = Generate(555); var clock = new RacePlaybackClock(Rules.HardTimeout); RaceResult? result = null;
            clock.RaceCompleted += value => result = value; clock.Start(plan);
            Assert.That(clock.Tick(Rules.HardTimeout - 0.1f, false), Is.False);
            Assert.That(clock.Tick(0.1f, false), Is.True);
            Assert.That(result.Value.ChampionId, Is.EqualTo(plan.ChampionId));
            Assert.That(result.Value.Reason, Is.EqualTo(RaceCompletionReason.HardTimeout));
        }

        [Test]
        public void Playback_DifferentFrameSteps_CannotChangeLockedChampion()
        {
            var plan = Generate(77123);
            RaceResult? fineResult = null; RaceResult? coarseResult = null;
            var fine = new RacePlaybackClock(Rules.HardTimeout); fine.RaceCompleted += value => fineResult = value; fine.Start(plan);
            var coarse = new RacePlaybackClock(Rules.HardTimeout); coarse.RaceCompleted += value => coarseResult = value; coarse.Start(plan);
            while (!fine.IsComplete) fine.Tick(1f / 120f);
            coarse.Tick(plan.TotalDuration + 1f);
            Assert.That(fineResult.Value.ChampionId, Is.EqualTo(plan.ChampionId));
            Assert.That(coarseResult.Value.ChampionId, Is.EqualTo(plan.ChampionId));
            Assert.That(coarseResult.Value.PlanId, Is.EqualTo(fineResult.Value.PlanId));
        }

        [Test]
        public void Countdown_RequiresLockedPlanAndCompletesAfterGo()
        {
            var clock = new RaceCountdownClock(); Assert.That(clock.DisplayText, Is.Empty);
            clock.Start(Generate(1)); Assert.That(clock.DisplayText, Is.EqualTo("3"));
            clock.Tick(1f); Assert.That(clock.DisplayText, Is.EqualTo("2"));
            clock.Tick(1f); Assert.That(clock.DisplayText, Is.EqualTo("1"));
            clock.Tick(1f); Assert.That(clock.DisplayText, Is.EqualTo("GO!"));
            Assert.That(clock.Tick(1f), Is.True); Assert.That(clock.IsComplete, Is.True);
        }

        private static RacePlan Generate(int seed)
        {
            var director = new RaceDirector(Roster, Rules); director.NotifyAllBetsLocked();
            Assert.That(director.TryGeneratePlan(1, new RaceSeed(seed), out var plan, out _), Is.True); return plan;
        }

        private sealed class InvalidFactory : IRacePlanFactory
        {
            public int CallCount { get; private set; }
            public RacePlan Create(int round, RaceSeed seed, IReadOnlyList<string> chickenIds, RaceRules rules)
            {
                CallCount++;
                var chickens = chickenIds.Select((id, lane) => new ChickenRacePlan(id, lane, false, -1f, 0.5f,
                    new[] { new RaceSegment(0f, rules.MinimumDuration, 0f, 0.5f, RaceInterpolation.Linear, true) }, new RaceEvent[0])).ToArray();
                return new RacePlan("invalid", rules.ConfigHash, round, seed, chickenIds[0], rules.MinimumDuration, chickens, "invalid");
            }
        }
    }
}
