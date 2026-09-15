using NUnit.Framework;
using GroundChickenKing.Players;
using GroundChickenKing.UI;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace GroundChickenKing.Tests.EditMode
{
    public sealed class MainSceneBaselineTests
    {
        [Test]
        public void MainScene_ConfiguredForTouchOnlyUI_HasRequiredComponents()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/SCN_Main.unity", OpenSceneMode.Single);

            var inputModule = Object.FindFirstObjectByType<InputSystemUIInputModule>();
            Assert.That(inputModule, Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<StandaloneInputModule>(), Is.Null);
            Assert.That(inputModule.point, Is.Not.Null);
            Assert.That(inputModule.leftClick, Is.Not.Null);
            Assert.That(inputModule.move, Is.Null);
            Assert.That(inputModule.submit, Is.Null);
            Assert.That(inputModule.cancel, Is.Null);

            var exitButton = GameObject.Find("UI_Button_ExitGame")?.GetComponent<Button>();
            Assert.That(exitButton, Is.Not.Null);
            Assert.That(exitButton.onClick.GetPersistentEventCount(), Is.EqualTo(1));
        }

        [Test]
        public void MainScene_PlayerSeatPanels_HaveFourLargeButtonsAndCorrectOrientation()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/SCN_Main.unity", OpenSceneMode.Single);

            var seatViews = Object.FindObjectsByType<PlayerSeatView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.That(seatViews, Has.Length.EqualTo(4));
            foreach (var seatView in seatViews)
            {
                var buttonSize = seatView.JoinButton.GetComponent<RectTransform>().sizeDelta;
                Assert.That(buttonSize.x, Is.GreaterThanOrEqualTo(280f));
                Assert.That(buttonSize.y, Is.GreaterThanOrEqualTo(100f));

                var expectedRotation = seatView.Seat is PlayerSeat.Player3 or PlayerSeat.Player4 ? 180f : 0f;
                Assert.That(Mathf.DeltaAngle(seatView.transform.localEulerAngles.z, expectedRotation), Is.EqualTo(0f).Within(0.1f));
            }
        }
    }
}
