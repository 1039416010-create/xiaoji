using System.Collections;
using System.Linq;
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
    public sealed class SettlementLoopPlayModeTests
    {
        [UnityTest]
        [Timeout(45000)]
        public IEnumerator CompletedRace_SettlesOnce_ThenRetainsChampionAndStartsNextRound()
        {
            SceneManager.LoadScene("SCN_Main", LoadSceneMode.Single);
            yield return null;
            var presenter = Object.FindFirstObjectByType<GameSessionPresenter>();
            Assert.That(presenter, Is.Not.Null);

            Click("UI_Button_StartGame");
            Click("UI_Button_Join_Player1");
            Click("UI_Button_Join_Player2");
            Click("UI_Button_ContinueToWarmup");
            Click("UI_Button_EnterBetting");
            var previousRoster = presenter.CurrentRoster.ToArray();
            LockBet("Player1", 0, 10);
            LockBet("Player2", 1, 5);

            var duration = presenter.CurrentRacePlan.TotalDuration;
            yield return new WaitForSecondsRealtime(4.2f + duration + 0.5f);

            Assert.That(presenter.CurrentState, Is.EqualTo(GameFlowState.Settlement));
            Assert.That(presenter.LastSettlement, Is.Not.Null);
            Assert.That(presenter.LastSettlement.Details.Count, Is.EqualTo(2));
            Assert.That(presenter.LastSettlement.Details.All(detail => detail.BalanceAfter >= 0), Is.True);
            Assert.That(GameObject.Find("UI_Panel_Settlement"), Is.Not.Null);
            var champion = presenter.LastSettlement.ChampionId;
            var championLane = System.Array.IndexOf(previousRoster, champion);
            Assert.That(championLane, Is.GreaterThanOrEqualTo(0));

            Click("UI_Button_NextRound");
            Assert.That(presenter.CurrentState, Is.EqualTo(GameFlowState.Warmup));
            Assert.That(presenter.CurrentRound, Is.EqualTo(2));
            Assert.That(presenter.LastSettlement, Is.Null);
            Assert.That(presenter.CurrentRacePlan, Is.Null);
            Assert.That(presenter.CurrentRoster.Count, Is.EqualTo(5));
            Assert.That(presenter.CurrentRoster.Distinct().Count(), Is.EqualTo(5));
            Assert.That(presenter.CurrentRoster[championLane], Is.EqualTo(champion));
            Assert.That(presenter.CurrentRoster.Where(id => id != champion).Any(previousRoster.Contains), Is.False);
            Click("UI_Button_EnterBetting"); Click("UI_Button_BetChicken_Player1_0");
            Assert.That(presenter.TryGetBetSnapshot(PlayerSeat.Player1, out var nextBet), Is.True);
            Assert.That(nextBet.ChickenId, Is.EqualTo(presenter.CurrentRoster[0]));
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
            Assert.That(button.interactable, Is.True, $"Button not interactable: {name}");
            button.onClick.Invoke();
        }
    }
}
