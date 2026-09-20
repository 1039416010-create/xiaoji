using System.Linq;
using GroundChickenKing.Chickens;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace GroundChickenKing.Tests.EditMode
{
    public sealed class Chicken2DArtIntegrationTests
    {
        private const string PartsDirectory = "Assets/_Game/Art/Characters/Production/2D/Flash/Parts";
        private const string AnimationDirectory = "Assets/_Game/Art/Characters/Production/2D/Flash/Animations";
        private const string PrefabPath = "Assets/_Game/Prefabs/Chickens/2D/PF_Chicken_flash_2D_WIP.prefab";
        private const string DefinitionPath = "Assets/_Game/Config/Chickens/CFG_Chicken2D_flash_WIP.asset";

        private static readonly string[] PartNames =
        {
            "torso", "head_neck", "wing_near", "wing_far", "tail", "upper_leg_near",
            "lower_leg_foot_near", "leg_far", "eye_white", "iris_pupil", "eyelids",
            "brow", "beaks", "crest", "headband", "ribbons",
        };

        private static readonly string[] StateNames =
        {
            "Idle", "Warmup", "Run", "Sprint", "Stop", "Fall", "Recover", "Turn",
            "Interfere", "Celebrate", "Lose",
        };

        [Test]
        public void FlashParts_AreCleanSingleSprites()
        {
            foreach (var partName in PartNames)
            {
                var path = $"{PartsDirectory}/CH_flash_{partName}_v1.png";
                Assert.That(AssetDatabase.LoadAssetAtPath<Sprite>(path), Is.Not.Null, partName);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Assert.That(importer, Is.Not.Null, partName);
                Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite), partName);
                Assert.That(importer.alphaIsTransparency, Is.True, partName);
                Assert.That(importer.mipmapEnabled, Is.False, partName);
            }
        }

        [Test]
        public void FlashPrefab_UsesLayeredSpritesAndNoRootMotion()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null);
            var animator = prefab.GetComponent<Animator>();
            Assert.That(animator, Is.Not.Null);
            Assert.That(animator.applyRootMotion, Is.False);
            Assert.That(animator.runtimeAnimatorController, Is.Not.Null);
            Assert.That(prefab.GetComponentsInChildren<SpriteRenderer>(true).Length, Is.EqualTo(12));
            Assert.That(prefab.transform.Find("ArtRoot/Head"), Is.Not.Null);
            Assert.That(prefab.transform.Find("ArtRoot/WingNear"), Is.Not.Null);
            Assert.That(prefab.transform.Find("ArtRoot/LowerLegNear"), Is.Not.Null);
            Assert.That(prefab.transform.Find("ArtRoot/LowerLegFar"), Is.Not.Null);
            Assert.That(prefab.transform.Find("ArtRoot/Ribbons"), Is.Not.Null);
        }

        [Test]
        public void FlashAnimator_ContainsAllStatesAndNoForwardTranslation()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{AnimationDirectory}/AC_flash_2D_WIP.controller");
            Assert.That(controller, Is.Not.Null);
            var stateNames = controller.layers[0].stateMachine.states.Select(item => item.state.name).ToArray();
            Assert.That(stateNames, Is.EquivalentTo(StateNames));

            foreach (var stateName in StateNames)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimationDirectory}/AN_flash_{stateName}_2D_WIP.anim");
                Assert.That(clip, Is.Not.Null, stateName);
                var bindings = AnimationUtility.GetCurveBindings(clip);
                Assert.That(bindings, Is.Not.Empty, stateName);
                foreach (var binding in bindings.Where(binding => binding.propertyName == "m_LocalPosition.x"))
                {
                    var curve = AnimationUtility.GetEditorCurve(clip, binding);
                    Assert.That(curve.keys.All(key => Mathf.Abs(key.value) < 0.0001f), Is.True, stateName);
                }
            }
        }

        [Test]
        public void FlashDefinition_ReferencesPrefabOverrideAndPortrait()
        {
            var definition = AssetDatabase.LoadAssetAtPath<Chicken2DVisualDefinition>(DefinitionPath);
            Assert.That(definition, Is.Not.Null);
            Assert.DoesNotThrow(definition.ValidateOrThrow);
            Assert.That(definition.ChickenId, Is.EqualTo("chicken-flash"));
            Assert.That(definition.Prefab, Is.Not.Null);
            Assert.That(definition.AnimatorOverride, Is.Not.Null);
            Assert.That(definition.Portrait, Is.Not.Null);
        }
    }
}
