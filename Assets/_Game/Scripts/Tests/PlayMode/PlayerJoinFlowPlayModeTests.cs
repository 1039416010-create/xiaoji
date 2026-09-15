using System.Collections;
using GroundChickenKing.Flow;
using GroundChickenKing.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace GroundChickenKing.Tests.PlayMode
{
    public sealed class PlayerJoinFlowPlayModeTests
    {
        [UnityTest]
        public IEnumerator MainScene_TwoSeatButtonsInSameFrame_ReachWarmupWithoutCrossingPlayers()
        {
            SceneManager.LoadScene("SCN_Main", LoadSceneMode.Single);
            yield return null;

            var presenter = Object.FindFirstObjectByType<GameSessionPresenter>();
            Assert.That(presenter, Is.Not.Null);
            Assert.That(presenter.CurrentState, Is.EqualTo(GameFlowState.MainMenu));

            GameObject.Find("UI_Button_StartGame").GetComponent<Button>().onClick.Invoke();
            Assert.That(presenter.CurrentState, Is.EqualTo(GameFlowState.PlayerJoin));
            Assert.That(presenter.ContinueButton.interactable, Is.False);

            GameObject.Find("UI_Button_Join_Player1").GetComponent<Button>().onClick.Invoke();
            GameObject.Find("UI_Button_Join_Player2").GetComponent<Button>().onClick.Invoke();
            Assert.That(presenter.JoinedPlayerCount, Is.EqualTo(2));
            Assert.That(presenter.ContinueButton.interactable, Is.True);

            presenter.ContinueButton.onClick.Invoke();
            Assert.That(presenter.CurrentState, Is.EqualTo(GameFlowState.Warmup));
            Assert.That(GameObject.Find("UI_Panel_Warmup"), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator MainScene_ReturnToMenu_ClearsJoinedPlayersAndRestoresMainMenu()
        {
            SceneManager.LoadScene("SCN_Main", LoadSceneMode.Single);
            yield return null;

            var presenter = Object.FindFirstObjectByType<GameSessionPresenter>();
            GameObject.Find("UI_Button_StartGame").GetComponent<Button>().onClick.Invoke();
            GameObject.Find("UI_Button_Join_Player1").GetComponent<Button>().onClick.Invoke();
            GameObject.Find("UI_Button_JoinBack").GetComponent<Button>().onClick.Invoke();

            Assert.That(presenter.CurrentState, Is.EqualTo(GameFlowState.PlayerJoin));
            Assert.That(GameObject.Find("UI_Panel_Confirmation"), Is.Not.Null);
            GameObject.Find("UI_Button_ConfirmationConfirm").GetComponent<Button>().onClick.Invoke();

            Assert.That(presenter.CurrentState, Is.EqualTo(GameFlowState.MainMenu));
            Assert.That(presenter.JoinedPlayerCount, Is.Zero);
        }
    }
}
