using System.Collections;
using System.Linq;
using GroundChickenKing.Betting;
using GroundChickenKing.Chickens;
using GroundChickenKing.Flow;
using GroundChickenKing.Players;
using GroundChickenKing.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace GroundChickenKing.Tests.PlayMode
{
    public sealed class BettingFlowPlayModeTests
    {
        [UnityTest]
        public IEnumerator BettingScreen_PlayerOneSelection_DoesNotChangePlayerTwo()
        {
            SceneManager.LoadScene("SCN_Main", LoadSceneMode.Single);
            yield return null;
            var presenter = NavigateToBettingWithTwoPlayers();
            yield return null;

            Click("UI_Button_BetChicken_Player1_0");
            Click("UI_Button_BetStake_Player1_10");
            Assert.That(presenter.TryGetBetSnapshot(PlayerSeat.Player1, out var playerOne), Is.True);
            Assert.That(presenter.TryGetBetSnapshot(PlayerSeat.Player2, out var playerTwo), Is.True);
            Assert.That(playerOne.ChickenId, Is.EqualTo("chicken-flash"));
            Assert.That(playerOne.Stake, Is.EqualTo(10));
            Assert.That(playerTwo.HasChicken, Is.False);
            Assert.That(playerTwo.HasStake, Is.False);
        }

        [UnityTest]
        public IEnumerator BettingScreen_DoubleLock_DeductsPlayerOnlyOnce()
        {
            SceneManager.LoadScene("SCN_Main", LoadSceneMode.Single);
            yield return null;
            var presenter = NavigateToBettingWithTwoPlayers();
            yield return null;
            Click("UI_Button_BetChicken_Player1_0");
            Click("UI_Button_BetStake_Player1_10");
            Click("UI_Button_LockBet_Player1");
            Click("UI_Button_LockBet_Player1");
            Assert.That(presenter.GetPlayerBalance(PlayerSeat.Player1), Is.EqualTo(40));
            Assert.That(presenter.TryGetBetSnapshot(PlayerSeat.Player1, out var snapshot), Is.True);
            Assert.That(snapshot.IsLocked, Is.True);
        }

        [UnityTest]
        public IEnumerator BettingScreen_AllPlayersLock_GeneratesOneLockedPlanAndReachesCountdown()
        {
            SceneManager.LoadScene("SCN_Main", LoadSceneMode.Single);
            yield return null;
            var presenter = NavigateToBettingWithTwoPlayers();
            yield return null;
            LockBet("Player1", 0, 10);
            LockBet("Player2", 1, 5);
            Assert.That(presenter.CurrentState, Is.EqualTo(GameFlowState.RaceCountdown));
            Assert.That(presenter.GetPlayerBalance(PlayerSeat.Player1), Is.EqualTo(40));
            Assert.That(presenter.GetPlayerBalance(PlayerSeat.Player2), Is.EqualTo(45));
            Assert.That(GameObject.Find("UI_Panel_RaceCountdown"), Is.Not.Null);
            Assert.That(presenter.CurrentRacePlan, Is.Not.Null);
            Assert.That(presenter.CurrentRacePlan.Chickens.Count, Is.EqualTo(5));
            Assert.That(presenter.CurrentRacePlan.Chickens.Count(chicken => chicken.IsFinisher), Is.EqualTo(1));
            Assert.That(presenter.LastRaceResult, Is.Null);
            yield return new WaitForSecondsRealtime(4.1f);
            Assert.That(presenter.CurrentState, Is.EqualTo(GameFlowState.Racing));
            var chickens = Object.FindObjectsByType<ChickenController>(FindObjectsSortMode.None);
            Assert.That(chickens.Length, Is.EqualTo(5));
            Assert.That(chickens.All(chicken => chicken.IsPlaying), Is.True);
        }

        private static GameSessionPresenter NavigateToBettingWithTwoPlayers()
        {
            var presenter = Object.FindFirstObjectByType<GameSessionPresenter>();
            Click("UI_Button_StartGame");
            Click("UI_Button_Join_Player1");
            Click("UI_Button_Join_Player2");
            Click("UI_Button_ContinueToWarmup");
            Click("UI_Button_EnterBetting");
            Assert.That(presenter.CurrentState, Is.EqualTo(GameFlowState.Betting));
            return presenter;
        }

        private static void LockBet(string playerName, int chickenIndex, int stake)
        {
            Click($"UI_Button_BetChicken_{playerName}_{chickenIndex}");
            Click($"UI_Button_BetStake_{playerName}_{stake}");
            Click($"UI_Button_LockBet_{playerName}");
        }

        private static void Click(string name)
        {
            var button = GameObject.Find(name)?.GetComponent<Button>();
            Assert.That(button, Is.Not.Null, $"Button not found: {name}");
            button.onClick.Invoke();
        }
    }
}
