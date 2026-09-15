using System.Linq;
using GroundChickenKing.Betting;
using GroundChickenKing.Players;
using GroundChickenKing.Race;
using NUnit.Framework;
using System.Diagnostics;
using System;

namespace GroundChickenKing.Tests.EditMode
{
    public sealed class FullSessionSoakTests
    {
        private static readonly string[] Catalog = Enumerable.Range(0, 13).Select(i => $"soak-{i}").ToArray();

        [Test]
        [Timeout(30000)]
        public void FullSession_FiveHundredRounds_NoStallNegativeBalanceDuplicateSettlementOrInvalidRoster()
        {
            GC.Collect(); var memoryBefore = GC.GetTotalMemory(true); var stopwatch = Stopwatch.StartNew();
            var players = new PlayerManager(1000); for (var i = 1; i <= 4; i++) Assert.That(players.TryJoin((PlayerSeat)i, out _), Is.True);
            var roster = new RosterService(Catalog, Catalog.Take(5).ToArray()); var settlement = new SettlementService(players);
            var rules = new RaceRules(5, 0f, 1f, 12f, 30f, .5f, .002f, 3, 40f);
            var generator = new RacePlanGenerator();
            for (var round = 1; round <= 500; round++)
            {
                var current = roster.CurrentRoster; var betting = new BettingManager(players, current, new[] { 1, 5, 10 }, round);
                for (var seat = 1; seat <= 4; seat++) { var player = (PlayerSeat)seat; Assert.That(betting.TrySelectChicken(player, current[seat - 1]).IsSuccess); Assert.That(betting.TrySelectStake(player, 1).IsSuccess); Assert.That(betting.TryLockBet(player).IsSuccess); }
                var plan = generator.Create(round, new RaceSeed(round * 7919), current, rules); Assert.That(plan.Chickens.Count(chicken => chicken.IsFinisher), Is.EqualTo(1));
                var result = new RaceResult(plan.ChampionId, plan.TotalDuration, RaceCompletionReason.PlannedFinish, plan.PlanId);
                Assert.That(settlement.TrySettle(round, result, betting.GetSnapshots(), out _), Is.True); Assert.That(settlement.TrySettle(round, result, betting.GetSnapshots(), out _), Is.False);
                Assert.That(players.GetAllSnapshots().Where(player => player.IsJoined).All(player => player.Coins >= 0), Is.True);
                var refreshed = roster.Refresh(plan.ChampionId, new RaceSeed(round * 104729)); Assert.That(refreshed.CurrentRoster.Count, Is.EqualTo(5)); Assert.That(refreshed.CurrentRoster.Distinct().Count(), Is.EqualTo(5));
            }
            stopwatch.Stop(); GC.Collect(); var memoryAfter = GC.GetTotalMemory(true); var memoryDelta = memoryAfter - memoryBefore;
            TestContext.Out.WriteLine($"500-round soak elapsedMs={stopwatch.ElapsedMilliseconds} managedMemoryDeltaBytes={memoryDelta}");
            Assert.That(memoryDelta, Is.LessThan(32L * 1024 * 1024));
        }
    }
}
