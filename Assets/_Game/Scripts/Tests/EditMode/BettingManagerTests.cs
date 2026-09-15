using GroundChickenKing.Betting;
using GroundChickenKing.Players;
using NUnit.Framework;

namespace GroundChickenKing.Tests.EditMode
{
    public sealed class BettingManagerTests
    {
        private static readonly string[] Chickens =
        {
            "chicken-flash", "chicken-chubby", "chicken-tiny", "chicken-bro", "chicken-slacker",
        };
        private static readonly int[] Stakes = { 1, 5, 10 };

        [Test]
        public void TryLockBet_ValidSelection_DebitsStakeAndWritesLedger()
        {
            var players = CreatePlayers(50, 1);
            var betting = new BettingManager(players, Chickens, Stakes, 1);
            betting.TrySelectChicken(PlayerSeat.Player1, Chickens[0]);
            betting.TrySelectStake(PlayerSeat.Player1, 10);

            BalanceTransaction transaction = default;
            players.BalanceChanged += value => transaction = value;
            var result = betting.TryLockBet(PlayerSeat.Player1);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(players.GetSnapshot(PlayerSeat.Player1).Coins, Is.EqualTo(40));
            Assert.That(transaction.Before, Is.EqualTo(50));
            Assert.That(transaction.Delta, Is.EqualTo(-10));
            Assert.That(transaction.After, Is.EqualTo(40));
            Assert.That(transaction.Reason, Is.EqualTo(BalanceChangeReason.BetLocked));
        }

        [TestCase(0)]
        [TestCase(2)]
        [TestCase(25)]
        public void TrySelectStake_InvalidAmount_IsRejected(int stake)
        {
            var players = CreatePlayers(50, 1);
            var betting = new BettingManager(players, Chickens, Stakes, 1);
            var result = betting.TrySelectStake(PlayerSeat.Player1, stake);
            Assert.That(result.Error, Is.EqualTo(BetCommandError.InvalidStake));
        }

        [Test]
        public void TrySelectChicken_UnknownChicken_IsRejected()
        {
            var betting = new BettingManager(CreatePlayers(50, 1), Chickens, Stakes, 1);
            var result = betting.TrySelectChicken(PlayerSeat.Player1, "not-in-roster");
            Assert.That(result.Error, Is.EqualTo(BetCommandError.InvalidChicken));
        }

        [Test]
        public void TrySelectStake_AboveBalance_IsRejected()
        {
            var betting = new BettingManager(CreatePlayers(4, 1), Chickens, Stakes, 1);
            Assert.That(betting.TrySelectStake(PlayerSeat.Player1, 1).IsSuccess, Is.True);
            Assert.That(betting.TrySelectStake(PlayerSeat.Player1, 5).Error, Is.EqualTo(BetCommandError.InsufficientBalance));
            Assert.That(betting.TrySelectStake(PlayerSeat.Player1, 10).Error, Is.EqualTo(BetCommandError.InsufficientBalance));
        }

        [Test]
        public void TryLockBet_MissingSelections_ReturnsExplicitErrors()
        {
            var betting = new BettingManager(CreatePlayers(50, 1), Chickens, Stakes, 1);
            Assert.That(betting.TryLockBet(PlayerSeat.Player1).Error, Is.EqualTo(BetCommandError.MissingChicken));
            betting.TrySelectChicken(PlayerSeat.Player1, Chickens[0]);
            Assert.That(betting.TryLockBet(PlayerSeat.Player1).Error, Is.EqualTo(BetCommandError.MissingStake));
        }

        [Test]
        public void TryLockBet_DoubleConfirmation_DebitsOnlyOnce()
        {
            var players = CreatePlayers(50, 2);
            var betting = new BettingManager(players, Chickens, Stakes, 1);
            betting.TrySelectChicken(PlayerSeat.Player1, Chickens[0]);
            betting.TrySelectStake(PlayerSeat.Player1, 10);
            Assert.That(betting.TryLockBet(PlayerSeat.Player1).IsSuccess, Is.True);
            Assert.That(betting.TryLockBet(PlayerSeat.Player1).Error, Is.EqualTo(BetCommandError.AlreadyLocked));
            Assert.That(players.GetSnapshot(PlayerSeat.Player1).Coins, Is.EqualTo(40));
        }

        [Test]
        public void LockedBet_SelectionChangesAreRejected()
        {
            var betting = new BettingManager(CreatePlayers(50, 2), Chickens, Stakes, 1);
            betting.TrySelectChicken(PlayerSeat.Player1, Chickens[0]);
            betting.TrySelectStake(PlayerSeat.Player1, 5);
            betting.TryLockBet(PlayerSeat.Player1);
            Assert.That(betting.TrySelectChicken(PlayerSeat.Player1, Chickens[1]).Error, Is.EqualTo(BetCommandError.AlreadyLocked));
            Assert.That(betting.TrySelectStake(PlayerSeat.Player1, 10).Error, Is.EqualTo(BetCommandError.AlreadyLocked));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void AllBetsLocked_OneToFourPlayers_PublishesExactlyOnce(int playerCount)
        {
            var players = CreatePlayers(50, playerCount);
            var betting = new BettingManager(players, Chickens, Stakes, 1);
            var publishCount = 0;
            betting.AllBetsLocked += _ => publishCount++;

            for (var index = 1; index <= playerCount; index++)
            {
                var seat = (PlayerSeat)index;
                betting.TrySelectChicken(seat, Chickens[index % Chickens.Length]);
                betting.TrySelectStake(seat, 1);
                betting.TryLockBet(seat);
            }

            Assert.That(publishCount, Is.EqualTo(1));
            Assert.That(betting.IsOpen, Is.False);
        }

        [Test]
        public void BankruptPlayer_IsExcludedAndDoesNotBlockAllBetsLocked()
        {
            var players = CreatePlayers(1, 2);
            players.TryDebit(PlayerSeat.Player1, 1, 0, BalanceChangeReason.BetLocked, out _);
            var betting = new BettingManager(players, Chickens, Stakes, 1);
            Assert.That(betting.EligiblePlayerCount, Is.EqualTo(1));
            Assert.That(betting.TryGetSnapshot(PlayerSeat.Player1, out _), Is.False);

            var publishCount = 0;
            betting.AllBetsLocked += _ => publishCount++;
            betting.TrySelectChicken(PlayerSeat.Player2, Chickens[0]);
            betting.TrySelectStake(PlayerSeat.Player2, 1);
            betting.TryLockBet(PlayerSeat.Player2);
            Assert.That(publishCount, Is.EqualTo(1));
        }

        [Test]
        public void ClearUncommittedRound_PreservesPlayerBalance()
        {
            var players = CreatePlayers(50, 1);
            var betting = new BettingManager(players, Chickens, Stakes, 1);
            betting.TrySelectChicken(PlayerSeat.Player1, Chickens[0]);
            betting.TrySelectStake(PlayerSeat.Player1, 10);
            Assert.That(betting.ClearUncommittedRound().IsSuccess, Is.True);
            Assert.That(players.GetSnapshot(PlayerSeat.Player1).Coins, Is.EqualTo(50));
        }

        private static PlayerManager CreatePlayers(int initialCoins, int count)
        {
            var players = new PlayerManager(initialCoins);
            for (var index = 1; index <= count; index++)
                players.TryJoin((PlayerSeat)index, out _);
            return players;
        }
    }
}
