using System.Linq;
using GroundChickenKing.Betting;
using GroundChickenKing.Flow;
using GroundChickenKing.Players;
using GroundChickenKing.Race;
using NUnit.Framework;

namespace GroundChickenKing.Tests.EditMode
{
    public sealed class SettlementAndRosterTests
    {
        private static readonly string[] Initial = { "c0", "c1", "c2", "c3", "c4" };
        private static readonly string[] Catalog = { "c0", "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "c9", "c10", "c11", "c12" };

        [Test]
        public void Settlement_WinnerGetsTwoTimesStake_LoserKeepsPostDebitBalance()
        {
            var players = JoinedPlayers(50, 2); var betting = LockBets(players, 1, (PlayerSeat.Player1, "c0", 10), (PlayerSeat.Player2, "c1", 10));
            var service = new SettlementService(players);
            Assert.That(service.TrySettle(1, Result("race-1", "c0"), betting.GetSnapshots(), out var record), Is.True);
            Assert.That(players.GetSnapshot(PlayerSeat.Player1).Coins, Is.EqualTo(60));
            Assert.That(players.GetSnapshot(PlayerSeat.Player2).Coins, Is.EqualTo(40));
            Assert.That(record.Details.Single(x => x.Seat == PlayerSeat.Player1).ReturnAmount, Is.EqualTo(20));
        }

        [Test]
        public void Settlement_MultiplePlayersSameChampion_AllReceiveIndependentPayouts()
        {
            var players = JoinedPlayers(50, 3); var betting = LockBets(players, 1,
                (PlayerSeat.Player1, "c2", 5), (PlayerSeat.Player2, "c2", 10), (PlayerSeat.Player3, "c1", 1));
            new SettlementService(players).TrySettle(1, Result("race", "c2"), betting.GetSnapshots(), out var record);
            Assert.That(players.GetSnapshot(PlayerSeat.Player1).Coins, Is.EqualTo(55));
            Assert.That(players.GetSnapshot(PlayerSeat.Player2).Coins, Is.EqualTo(60));
            Assert.That(players.GetSnapshot(PlayerSeat.Player3).Coins, Is.EqualTo(49));
            Assert.That(record.Details.Count(x => x.DidWin), Is.EqualTo(2));
        }

        [Test]
        public void Settlement_SameRaceSubmittedTwice_DoesNotPayTwice()
        {
            var players = JoinedPlayers(50, 1); var betting = LockBets(players, 1, (PlayerSeat.Player1, "c0", 10)); var service = new SettlementService(players);
            Assert.That(service.TrySettle(1, Result("same", "c0"), betting.GetSnapshots(), out var first), Is.True);
            Assert.That(service.TrySettle(1, Result("same", "c0"), betting.GetSnapshots(), out var second), Is.False);
            Assert.That(second, Is.SameAs(first)); Assert.That(players.GetSnapshot(PlayerSeat.Player1).Coins, Is.EqualTo(60));
            Assert.That(players.GetTransactions().Count(x => x.Reason == BalanceChangeReason.BetPayout), Is.EqualTo(1));
        }

        [Test]
        public void Settlement_NoPlayerPickedChampion_NoPayoutOccurs()
        {
            var players = JoinedPlayers(50, 2); var betting = LockBets(players, 1, (PlayerSeat.Player1, "c0", 5), (PlayerSeat.Player2, "c1", 10));
            new SettlementService(players).TrySettle(1, Result("none", "c4"), betting.GetSnapshots(), out var record);
            Assert.That(record.Details.All(x => !x.DidWin && x.ReturnAmount == 0), Is.True);
            Assert.That(players.GetSnapshot(PlayerSeat.Player1).Coins, Is.EqualTo(45)); Assert.That(players.GetSnapshot(PlayerSeat.Player2).Coins, Is.EqualTo(40));
        }

        [Test]
        public void RosterRefresh_RetainsChampionAndReplacesAllFourLosers()
        {
            var service = new RosterService(Catalog, Initial); var result = service.Refresh("c2", new RaceSeed(88));
            Assert.That(result.CurrentRoster.Count, Is.EqualTo(5)); Assert.That(result.CurrentRoster.Distinct().Count(), Is.EqualTo(5));
            Assert.That(result.CurrentRoster[2], Is.EqualTo("c2"));
            Assert.That(result.CurrentRoster.Where(x => x != "c2").Any(Initial.Contains), Is.False);
        }

        [Test]
        public void RosterCatalog_FewerThanNineUniqueChickens_IsRejectedBeforePlay()
        {
            Assert.Throws<System.ArgumentException>(() => new RosterService(Catalog.Take(8).ToArray(), Initial));
        }

        [Test]
        public void SessionEndEvaluator_SingleBankruptPlayerDoesNotStopSolventPlayers()
        {
            var players = JoinedPlayers(1, 2); LockBets(players, 1, (PlayerSeat.Player1, "c0", 1));
            Assert.That(SessionEndEvaluator.Evaluate(players.GetAllSnapshots(), 1, 20), Is.EqualTo(GameOverReason.None));
        }

        [Test]
        public void SessionEndEvaluator_AllBankruptAndMaximumRoundsReachBothReasons()
        {
            var bankrupt = JoinedPlayers(1, 1); LockBets(bankrupt, 1, (PlayerSeat.Player1, "c0", 1));
            Assert.That(SessionEndEvaluator.Evaluate(bankrupt.GetAllSnapshots(), 1, 20), Is.EqualTo(GameOverReason.Bankrupt));
            var solvent = JoinedPlayers(50, 1);
            Assert.That(SessionEndEvaluator.Evaluate(solvent.GetAllSnapshots(), 20, 20), Is.EqualTo(GameOverReason.MaxRounds));
        }

        [Test]
        public void Loop_OneHundredRounds_NoNegativeBalanceDuplicatePayoutOrInvalidRoster()
        {
            var players = JoinedPlayers(50, 1); var settlement = new SettlementService(players); var roster = new RosterService(Catalog, Initial);
            for (var round = 1; round <= 100; round++)
            {
                var target = roster.CurrentRoster[0]; var betting = LockBets(players, round, roster.CurrentRoster, (PlayerSeat.Player1, target, 1));
                var champion = round % 2 == 0 ? target : roster.CurrentRoster[1];
                Assert.That(settlement.TrySettle(round, Result($"race-{round}", champion), betting.GetSnapshots(), out _), Is.True);
                Assert.That(players.GetSnapshot(PlayerSeat.Player1).Coins, Is.GreaterThanOrEqualTo(0));
                var refresh = roster.Refresh(champion, new RaceSeed(round));
                Assert.That(refresh.CurrentRoster.Count, Is.EqualTo(5)); Assert.That(refresh.CurrentRoster.Distinct().Count(), Is.EqualTo(5));
            }
            Assert.That(players.GetSnapshot(PlayerSeat.Player1).Coins, Is.EqualTo(50));
            Assert.That(players.GetTransactions().Count(x => x.Reason == BalanceChangeReason.BetPayout), Is.EqualTo(50));
        }

        private static PlayerManager JoinedPlayers(int coins, int count)
        {
            var players = new PlayerManager(coins); for (var i = 1; i <= count; i++) players.TryJoin((PlayerSeat)i, out _); return players;
        }
        private static BettingManager LockBets(PlayerManager players, int round, params (PlayerSeat Seat, string Chicken, int Stake)[] bets)
        {
            return LockBets(players, round, Initial, bets);
        }
        private static BettingManager LockBets(PlayerManager players, int round, System.Collections.Generic.IReadOnlyList<string> roster, params (PlayerSeat Seat, string Chicken, int Stake)[] bets)
        {
            var manager = new BettingManager(players, roster, new[] { 1, 5, 10 }, round);
            foreach (var bet in bets) { manager.TrySelectChicken(bet.Seat, bet.Chicken); manager.TrySelectStake(bet.Seat, bet.Stake); Assert.That(manager.TryLockBet(bet.Seat).IsSuccess, Is.True); }
            return manager;
        }
        private static RaceResult Result(string planId, string champion) => new(champion, 10f, RaceCompletionReason.PlannedFinish, planId);
    }
}
