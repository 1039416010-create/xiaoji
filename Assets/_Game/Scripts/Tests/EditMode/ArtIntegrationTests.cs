using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GroundChickenKing.Tests.EditMode
{
    public sealed class ArtIntegrationTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/SCN_Main.unity";
        private const string BackgroundPath = "Assets/_Game/Art/Environment/Generated/BG_TabletopFarmFestival.png";
        private const string ChickenAtlasPath = "Assets/_Game/Art/Characters/Generated/SPR_ChickenRoster_Atlas.png";
        private const string ButtonPath = "Assets/_Game/Art/UI/Generated/SPR_UI_WoodButton.png";
        private const string PanelPath = "Assets/_Game/Art/UI/Generated/SPR_UI_ParchmentPanel.png";
        private const string IconAtlasPath = "Assets/_Game/Art/UI/Generated/SPR_UI_IconAtlas.png";

        [Test]
        public void GeneratedArtAssets_AreImportedWithExpectedDimensions()
        {
            var background = AssetDatabase.LoadAssetAtPath<Texture2D>(BackgroundPath);
            var chickens = AssetDatabase.LoadAssetAtPath<Texture2D>(ChickenAtlasPath);
            var icons = AssetDatabase.LoadAssetAtPath<Texture2D>(IconAtlasPath);
            Assert.That(background, Is.Not.Null);
            Assert.That(chickens, Is.Not.Null);
            Assert.That(icons, Is.Not.Null);
            Assert.That((float)background.width / background.height, Is.EqualTo(2f).Within(0.01f));
            Assert.That(chickens.width, Is.GreaterThan(chickens.height * 2));
            Assert.That(AssetDatabase.LoadAssetAtPath<Sprite>(ButtonPath), Is.Not.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<Sprite>(PanelPath), Is.Not.Null);
        }

        [Test]
        public void MainScene_UsesGeneratedBackgroundAndButtonSkin()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var background = Find(scene, "UI_Image_GeneratedBackground")?.GetComponent<Image>();
            Assert.That(background, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(background.sprite), Is.EqualTo(BackgroundPath));
            var buttons = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Button>(true)).ToArray();
            Assert.That(buttons, Is.Not.Empty);
            Assert.That(buttons.All(button => AssetDatabase.GetAssetPath(button.image.sprite) == ButtonPath), Is.True);
            Assert.That(buttons.All(button => button.image.type == Image.Type.Sliced), Is.True);
        }

        [Test]
        public void ChickenPrefabs_UseFiveDistinctAtlasColumns()
        {
            for (var i = 0; i < 5; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Game/Prefabs/Chickens/PF_Chicken_{i + 1}.prefab");
                var artwork = prefab.transform.Find("Visual/Artwork")?.GetComponent<RawImage>();
                Assert.That(artwork, Is.Not.Null, $"Chicken {i + 1} is missing generated artwork.");
                Assert.That(AssetDatabase.GetAssetPath(artwork.texture), Is.EqualTo(ChickenAtlasPath));
                Assert.That(artwork.uvRect.x, Is.EqualTo(i * 0.2f).Within(0.001f));
                Assert.That(artwork.uvRect.width, Is.EqualTo(0.2f).Within(0.001f));
            }
        }

        [Test]
        public void ChickenAnimationClips_TargetArtworkWithoutRootMotion()
        {
            var stateNames = new[] { "Idle", "Warmup", "Run", "Sprint", "Stop", "Fall", "Recover", "Turn", "Interfere", "Celebrate", "Lose" };
            foreach (var stateName in stateNames)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"Assets/_Game/Animations/Chickens/AN_Chicken_{stateName}.anim");
                Assert.That(clip, Is.Not.Null);
                Assert.That(AnimationUtility.GetCurveBindings(clip), Is.Not.Empty);
                Assert.That(AnimationUtility.GetCurveBindings(clip).All(binding => binding.path == "Artwork"), Is.True);
            }

            for (var i = 0; i < 5; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Game/Prefabs/Chickens/PF_Chicken_{i + 1}.prefab");
                Assert.That(prefab.transform.Find("Visual").GetComponent<Animator>().applyRootMotion, Is.False);
            }
        }

        private static GameObject Find(UnityEngine.SceneManagement.Scene scene, string name)
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(transform => transform.name == name)?.gameObject;
        }
    }
}
