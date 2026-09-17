using System.Linq;
using GroundChickenKing.Chickens;
using GroundChickenKing.Persistence;
using GroundChickenKing.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GroundChickenKing.Tests.EditMode
{
    public sealed class ImmersivePresentationTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/SCN_Main.unity";

        [Test]
        public void Chicken3DStyles_ThirteenRosterMembersHaveUniqueAccessoryAndProportions()
        {
            var styles = Chicken3DStyleCatalog.All;
            Assert.That(styles.Count, Is.EqualTo(13));
            Assert.That(styles.Select(item => item.Id).Distinct().Count(), Is.EqualTo(13));
            Assert.That(styles.Select(item => item.Accessory).Distinct().Count(), Is.EqualTo(13));
            Assert.That(styles.Select(item => item.Proportions).Distinct().Count(), Is.EqualTo(13));
            Assert.That(styles.All(item => item.Gait > 0f), Is.True);
        }

        [Test]
        public void ExhibitSettings_CopyPreservesFullscreenChoice()
        {
            var settings = new ExhibitSettings { Fullscreen = false };
            Assert.That(settings.Copy().Fullscreen, Is.False);
            settings.Fullscreen = true;
            Assert.That(settings.Copy().Fullscreen, Is.True);
        }

        [Test]
        public void MainScene_ContainsFour3DOutputsAndVisibleFullscreenButton()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            foreach (var name in new[] { "UI_3D_Warmup", "UI_3D_Betting", "UI_3D_Race", "UI_3D_Settlement" })
            {
                var output = Find(scene, name)?.GetComponent<RawImage>();
                Assert.That(output, Is.Not.Null, name);
                Assert.That(output.raycastTarget, Is.False, name);
            }
            var fullscreen = Find(scene, "UI_Button_Fullscreen")?.GetComponent<Button>();
            Assert.That(fullscreen, Is.Not.Null);
            Assert.That(fullscreen.gameObject.activeInHierarchy, Is.False, "Settings starts closed but the button must remain serialized in it.");
            Assert.That(Find(scene, "ImmersiveChickenStage")?.GetComponent<Chicken3DStage>(), Is.Not.Null);
        }

        [Test]
        public void ChickenRacePresenter_Awake_RebindsFiveControllersToImmersiveStage()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var presenter = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<ChickenRacePresenter>(true))
                .Single();
            var stage = Find(scene, "ImmersiveChickenStage").GetComponent<Chicken3DStage>();

            presenter.RebindImmersiveStage();

            Assert.That(stage.BoundControllerCount, Is.EqualTo(5));
        }

        [Test]
        public void SettlementScene_UsesPodiumBackgroundMealsAndCelebrationView()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var background = Find(scene, "UI_Image_SettlementStage")?.GetComponent<Image>();
            Assert.That(background, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(background.sprite), Is.EqualTo("Assets/_Game/Art/Environment/Generated/BG_SettlementPodium_v2.png"));
            Assert.That(Find(scene, "UI_Panel_SettlementCelebration")?.GetComponent<SettlementCelebrationView>(), Is.Not.Null);
            for (var i = 1; i <= 4; i++)
                Assert.That(Find(scene, $"UI_Image_CurryMeal_{i}")?.GetComponent<RawImage>(), Is.Not.Null);
        }

        private static GameObject Find(UnityEngine.SceneManagement.Scene scene, string name)
        {
            return scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).FirstOrDefault(item => item.name == name)?.gameObject;
        }
    }
}
