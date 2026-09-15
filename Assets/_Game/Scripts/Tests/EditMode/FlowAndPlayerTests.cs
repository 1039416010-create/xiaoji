using System.Collections.Concurrent;
using System.Threading.Tasks;
using GroundChickenKing.Flow;
using GroundChickenKing.Players;
using NUnit.Framework;

namespace GroundChickenKing.Tests.EditMode
{
    public sealed class FlowAndPlayerTests
    {
        [Test]
        public void GameFlow_InitializeAndStartGame_ReachesPlayerJoin()
        {
            var flow = new GameFlowController(() => 0, 30f);
            Assert.That(flow.Initialize(), Is.True);
            Assert.That(flow.EnterPlayerJoin(), Is.True);
            Assert.That(flow.CurrentState, Is.EqualTo(GameFlowState.PlayerJoin));
        }

        [Test]
        public void ContinueToWarmup_WithoutPlayers_IsRejected()
        {
            var flow = new GameFlowController(() => 0, 30f);
            flow.Initialize();
            flow.EnterPlayerJoin();
            Assert.That(flow.ContinueToWarmup(), Is.False);
            Assert.That(flow.CurrentState, Is.EqualTo(GameFlowState.PlayerJoin));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void ContinueToWarmup_WithOneToFourPlayers_Succeeds(int playerCount)
        {
            var players = new PlayerManager(50);
            for (var index = 1; index <= playerCount; index++)
                Assert.That(players.TryJoin((PlayerSeat)index, out _), Is.True);

            var flow = new GameFlowController(() => players.JoinedCount, 30f);
            flow.Initialize();
            flow.EnterPlayerJoin();
            Assert.That(flow.ContinueToWarmup(), Is.True);
            Assert.That(flow.CurrentState, Is.EqualTo(GameFlowState.Warmup));
        }

        [Test]
        public void TryJoin_SameSeatTwice_JoinsOnlyOnceAndPreservesInitialCoins()
        {
            var players = new PlayerManager(50);
            Assert.That(players.TryJoin(PlayerSeat.Player1, out var first), Is.True);
            Assert.That(players.TryJoin(PlayerSeat.Player1, out var second), Is.False);
            Assert.That(players.JoinedCount, Is.EqualTo(1));
            Assert.That(first.Coins, Is.EqualTo(50));
            Assert.That(second.Coins, Is.EqualTo(50));
        }

        [Test]
        public void TryJoin_FourConcurrentSeatRequests_KeepsIndependentPlayers()
        {
            var players = new PlayerManager(50);
            var results = new ConcurrentBag<bool>();
            Parallel.For(1, 5, index => results.Add(players.TryJoin((PlayerSeat)index, out _)));
            Assert.That(results, Has.All.True);
            Assert.That(players.JoinedCount, Is.EqualTo(4));
        }

        [Test]
        public void ReturnToMainMenu_AfterWarmup_AllowsSessionCleanup()
        {
            var players = new PlayerManager(50);
            players.TryJoin(PlayerSeat.Player1, out _);
            var flow = new GameFlowController(() => players.JoinedCount, 30f);
            flow.Initialize();
            flow.EnterPlayerJoin();
            flow.ContinueToWarmup();

            Assert.That(flow.ReturnToMainMenu(), Is.True);
            players.ClearAll();
            Assert.That(flow.CurrentState, Is.EqualTo(GameFlowState.MainMenu));
            Assert.That(players.JoinedCount, Is.Zero);
        }

        [Test]
        public void Tick_PlayerJoinTimeout_StaysOperableAndDoesNotStartWarmup()
        {
            var recoveryCount = 0;
            var flow = new GameFlowController(() => 1, 1f);
            flow.StateRecoveredFromTimeout += _ => recoveryCount++;
            flow.Initialize();
            flow.EnterPlayerJoin();
            flow.Tick(1f);
            Assert.That(flow.CurrentState, Is.EqualTo(GameFlowState.PlayerJoin));
            Assert.That(recoveryCount, Is.EqualTo(1));
        }

        [Test]
        public void Tick_WarmupTimeout_ReturnsSafelyToPlayerJoin()
        {
            var flow = new GameFlowController(() => 1, 1f);
            flow.Initialize();
            flow.EnterPlayerJoin();
            flow.ContinueToWarmup();
            flow.Tick(1f);
            Assert.That(flow.CurrentState, Is.EqualTo(GameFlowState.PlayerJoin));
        }
    }
}
