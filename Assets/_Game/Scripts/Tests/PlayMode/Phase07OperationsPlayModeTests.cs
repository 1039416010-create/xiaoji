using System.Collections;
using GroundChickenKing.Flow;
using GroundChickenKing.UI;
using GroundChickenKing.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace GroundChickenKing.Tests.PlayMode
{
    public sealed class Phase07OperationsPlayModeTests
    {
        [UnityTest]
        public IEnumerator MainMenu_SettingsAndDiagnostics_OpenAndCloseOnlyThroughButtons()
        {
            SceneManager.LoadScene("SCN_Main", LoadSceneMode.Single); yield return null;
            Click("UI_Button_Settings"); Assert.That(GameObject.Find("UI_Panel_Settings"), Is.Not.Null);
            Click("UI_Button_VolumeDown"); Click("UI_Button_IntensityDown"); Click("UI_Button_RoundsDown");
            Click("UI_Button_SettingsSave"); Assert.That(GameObject.Find("UI_Panel_Settings"), Is.Null);
            Click("UI_Button_Diagnostics"); Assert.That(GameObject.Find("UI_Panel_Diagnostics"), Is.Not.Null);
            Assert.That(GameObject.Find("UI_Text_DiagnosticsSummary").GetComponent<Text>().text, Does.Contain("分辨率"));
            Click("UI_Button_DiagnosticsClose"); Assert.That(GameObject.Find("UI_Panel_Diagnostics"), Is.Null);
        }

        [UnityTest]
        public IEnumerator ReturnToMenu_ConfirmationCancelThenConfirm_PreservesThenClearsSession()
        {
            SceneManager.LoadScene("SCN_Main", LoadSceneMode.Single); yield return null;
            var presenter = Object.FindFirstObjectByType<GameSessionPresenter>();
            Click("UI_Button_StartGame"); Click("UI_Button_Join_Player1"); Click("UI_Button_JoinBack");
            Click("UI_Button_ConfirmationCancel"); Assert.That(presenter.CurrentState, Is.EqualTo(GameFlowState.PlayerJoin)); Assert.That(presenter.JoinedPlayerCount, Is.EqualTo(1));
            Click("UI_Button_JoinBack"); Click("UI_Button_ConfirmationConfirm");
            Assert.That(presenter.CurrentState, Is.EqualTo(GameFlowState.MainMenu)); Assert.That(presenter.JoinedPlayerCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator FourPlayerButtons_SameFrame_RemainIndependentWithTouchFeedback()
        {
            SceneManager.LoadScene("SCN_Main", LoadSceneMode.Single); yield return null;
            var presenter = Object.FindFirstObjectByType<GameSessionPresenter>(); Click("UI_Button_StartGame");
            for (var i = 1; i <= 4; i++) Click($"UI_Button_Join_Player{i}");
            Assert.That(presenter.JoinedPlayerCount, Is.EqualTo(4));
            for (var i = 1; i <= 4; i++) Assert.That(GameObject.Find($"UI_Button_Join_Player{i}").GetComponent<TouchButtonFeedback>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator Diagnostics_FiveCalibrationTargets_AllCompleteThroughVisibleButtons()
        {
            SceneManager.LoadScene("SCN_Main", LoadSceneMode.Single); yield return null; Click("UI_Button_Diagnostics"); Click("UI_Button_TouchCalibration");
            var controller = Object.FindFirstObjectByType<TouchCalibrationController>(); Assert.That(controller, Is.Not.Null);
            Click("UI_Button_CalibrationTopLeft"); Click("UI_Button_CalibrationTopRight"); Click("UI_Button_CalibrationCenter"); Click("UI_Button_CalibrationBottomLeft"); Click("UI_Button_CalibrationBottomRight");
            Assert.That(controller.PassedCount, Is.EqualTo(5)); Assert.That(GameObject.Find("UI_Text_TouchCalibrationStatus").GetComponent<Text>().text, Does.Contain("通过"));
            Click("UI_Button_TouchCalibrationClose"); Assert.That(GameObject.Find("UI_Panel_TouchCalibration"), Is.Null);
        }

        private static void Click(string name)
        {
            var button = GameObject.Find(name)?.GetComponent<Button>(); Assert.That(button, Is.Not.Null, name); button.onClick.Invoke();
        }
    }
}
