using System.Collections.Generic;
using System.Linq;
using GroundChickenKing.Chickens;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace GroundChickenKing.Tests.EditMode
{
    public sealed class Chicken2DRemainingRosterFrameAnimationTests
    {
        private static readonly string[] Keys =
        {
            "bro", "captain", "dumpling", "lucky", "ninja",
            "rocket", "scholar", "sleepy", "thunder", "tiny",
        };

        private static IEnumerable<TestCaseData> CharacterCases()
        {
            foreach (var key in Keys)
                yield return new TestCaseData(key).SetName($"{key}_uses_approved_full_character_frames");
        }

        [TestCaseSource(nameof(CharacterCases))]
        public void ApprovedPoseSheet_ProducesThirteenFixedTransparentFrames(string key)
        {
            var displayKey = char.ToUpperInvariant(key[0]) + key.Substring(1);
            var directory = $"Assets/_Game/Art/Characters/Production/2D/{displayKey}/FrameAnimation/Frames";
            var guids = AssetDatabase.FindAssets("t:Sprite", new[] { directory });
            Assert.That(guids.Length, Is.EqualTo(13));
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                Assert.That(texture.width, Is.EqualTo(384), path);
                Assert.That(texture.height, Is.EqualTo(512), path);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Assert.That(importer, Is.Not.Null, path);
                Assert.That(importer.alphaIsTransparency, Is.True, path);
            }
        }

        [TestCaseSource(nameof(CharacterCases))]
        public void RunClip_AlternatesOnlyTheTwoAuthoredRunPoses(string key)
        {
            var displayKey = char.ToUpperInvariant(key[0]) + key.Substring(1);
            var directory = $"Assets/_Game/Art/Characters/Production/2D/{displayKey}/FrameAnimation/Animations";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{directory}/AN_{key}_Run_2D_Frames.anim");
            Assert.That(clip, Is.Not.Null);
            var binding = AnimationUtility.GetObjectReferenceCurveBindings(clip).Single();
            var names = AnimationUtility.GetObjectReferenceCurve(clip, binding)
                .Select(frame => frame.value.name)
                .ToArray();
            Assert.That(names, Is.EqualTo(new[]
            {
                $"CH_{key}_run_contact_v1", $"CH_{key}_run_passing_v1",
                $"CH_{key}_run_contact_v1", $"CH_{key}_run_passing_v1",
            }));
            Assert.That(AnimationUtility.GetCurveBindings(clip), Is.Empty);
        }

        [TestCaseSource(nameof(CharacterCases))]
        public void ControllerPrefabAndDefinition_AreCompleteAndDoNotDeformCharacter(string key)
        {
            var displayKey = char.ToUpperInvariant(key[0]) + key.Substring(1);
            var animationDirectory = $"Assets/_Game/Art/Characters/Production/2D/{displayKey}/FrameAnimation/Animations";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                $"{animationDirectory}/AC_{key}_2D_Frames.controller");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                $"Assets/_Game/Prefabs/Chickens/2D/PF_Chicken_{key}_2D_Frames.prefab");
            var definition = AssetDatabase.LoadAssetAtPath<Chicken2DVisualDefinition>(
                $"Assets/_Game/Config/Chickens/CFG_Chicken2D_{key}_Frames.asset");
            var material = AssetDatabase.LoadAssetAtPath<Material>(
                $"Assets/_Game/Art/Materials/MAT_Chicken2D_{key}_Unlit.mat");

            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.layers[0].stateMachine.states.Length, Is.EqualTo(11));
            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.transform.localScale, Is.EqualTo(Vector3.one));
            var renderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
            Assert.That(renderers.Length, Is.EqualTo(1));
            Assert.That(renderers[0].sharedMaterial, Is.EqualTo(material));
            Assert.That(prefab.GetComponent<Animator>().applyRootMotion, Is.False);
            Assert.That(definition, Is.Not.Null);
            Assert.DoesNotThrow(definition.ValidateOrThrow);
            Assert.That(definition.ChickenId, Is.EqualTo($"chicken-{key}"));
            Assert.That(definition.Prefab, Is.EqualTo(prefab));
            Assert.That(definition.StageScale, Is.EqualTo(1f));
        }

        [Test]
        public void CompletedRoster_HasThirteenUniqueFrameDefinitions()
        {
            var allKeys = Keys.Concat(new[] { "flash", "chubby", "slacker" }).ToArray();
            var definitions = allKeys.Select(key => AssetDatabase.LoadAssetAtPath<Chicken2DVisualDefinition>(
                    $"Assets/_Game/Config/Chickens/CFG_Chicken2D_{key}_Frames.asset"))
                .ToArray();
            Assert.That(definitions, Has.None.Null);
            Assert.That(definitions.Select(definition => definition.ChickenId).Distinct().Count(), Is.EqualTo(13));
        }
    }
}
