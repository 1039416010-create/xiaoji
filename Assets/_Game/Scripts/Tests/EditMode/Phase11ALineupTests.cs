using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using GroundChickenKing.UI;

namespace GroundChickenKing.Tests.EditMode
{
    public sealed class Phase11ALineupTests
    {
        private const string ScenePath = "Assets/_Game/Scenes/SCN_Phase11A_2DLineup.unity";
        private const string PrefabPath = "Assets/_Game/Prefabs/Race/PF_Race_Phase11A_2DLineup.prefab";
        private const string PreviewPath = "Assets/_Game/Art/Characters/Production/2D/Phase11A/Preview/PHASE11A_2D_Lineup_1920x960.png";

        [Test]
        public void LineupPrefab_HasFiveLanesWithAlignedCompleteCharacterSprites()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.transform.childCount, Is.EqualTo(5));
            var sortingOrders = new int[5];
            for (var laneIndex = 0; laneIndex < 5; laneIndex++)
            {
                var lane = prefab.transform.Find($"Lane_{laneIndex + 1}");
                Assert.That(lane, Is.Not.Null);
                var character = lane.Cast<Transform>().Single(child => child.name.StartsWith("Chicken_"));
                var renderers = character.GetComponentsInChildren<SpriteRenderer>(true);
                Assert.That(renderers.Length, Is.EqualTo(1), character.name);
                Assert.That(character.localScale, Is.EqualTo(Vector3.one), character.name);
                Assert.That(character.GetComponent<Animator>().applyRootMotion, Is.False, character.name);
                var renderer = renderers[0];
                Assert.That(renderer.sharedMaterial, Is.Not.Null, character.name);
                Assert.That(renderer.sharedMaterial.shader.name, Does.Contain("Sprite-Unlit-Default"), character.name);
                sortingOrders[laneIndex] = renderer.sortingOrder;
                var visibleBottom = character.localPosition.y + renderer.localBounds.min.y;
                var baseline = lane.Find("Baseline").localPosition.y;
                Assert.That(visibleBottom, Is.EqualTo(baseline).Within(0.001f), character.name);
            }
            Assert.That(sortingOrders.Distinct().Count(), Is.EqualTo(5));
        }

        [Test]
        public void LineupScene_UsesTwoToOneOrthographicComposition()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Assert.That(scene.IsValid(), Is.True);
            var camera = Object.FindFirstObjectByType<Camera>();
            Assert.That(camera, Is.Not.Null);
            Assert.That(camera.orthographic, Is.True);
            Assert.That(camera.orthographicSize, Is.EqualTo(4.8f).Within(0.001f));
            Assert.That(camera.GetComponent<AspectRatioController>(), Is.Not.Null);
            Assert.That(Object.FindObjectsByType<Animator>(FindObjectsSortMode.None).Length, Is.EqualTo(5));
        }

        [Test]
        public void LineupPreview_IsRenderedAtReferenceResolutionAndContainsVisibleArt()
        {
            var absolutePath = Path.GetFullPath(PreviewPath);
            Assert.That(File.Exists(absolutePath), Is.True);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
            Assert.That(texture.LoadImage(File.ReadAllBytes(absolutePath), false), Is.True);
            Assert.That(texture.width, Is.EqualTo(1920));
            Assert.That(texture.height, Is.EqualTo(960));
            var pixels = texture.GetPixels32();
            var sampledColors = pixels.Where((_, index) => index % 257 == 0).Distinct().Count();
            Object.DestroyImmediate(texture);
            Assert.That(sampledColors, Is.GreaterThan(32));
        }
    }
}
