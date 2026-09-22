using System.Linq;
using GroundChickenKing.Chickens;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GroundChickenKing.Tests.EditMode
{
    public sealed class Chicken2DBodyTypeFrameAnimationTests
    {
        [TestCase("Chubby", "chubby", 12)]
        [TestCase("Slacker", "slacker", 13)]
        public void ApprovedPoseSheet_ProducesFixedTransparentFullCharacterFrames(
            string displayKey,
            string key,
            int expectedCount)
        {
            var directory = $"Assets/_Game/Art/Characters/Production/2D/{displayKey}/FrameAnimation/Frames";
            var guids = AssetDatabase.FindAssets("t:Sprite", new[] { directory });
            Assert.That(guids.Length, Is.EqualTo(expectedCount));
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                Assert.That(texture.width, Is.EqualTo(384), path);
                Assert.That(texture.height, Is.EqualTo(512), path);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Assert.That(importer, Is.Not.Null, path);
                Assert.That(importer.alphaIsTransparency, Is.True, path);
                Assert.That(path, Does.Contain($"CH_{key}_"));
            }
        }

        [TestCase("Chubby", "chubby")]
        [TestCase("Slacker", "slacker")]
        public void RunClip_AlternatesTheTwoAuthoredRunPosesWithoutRootMovement(string displayKey, string key)
        {
            var directory = $"Assets/_Game/Art/Characters/Production/2D/{displayKey}/FrameAnimation/Animations";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{directory}/AN_{key}_Run_2D_Frames.anim");
            Assert.That(clip, Is.Not.Null);
            var binding = AnimationUtility.GetObjectReferenceCurveBindings(clip).Single();
            var names = AnimationUtility.GetObjectReferenceCurve(clip, binding)
                .Select(frame => frame.value.name)
                .ToArray();
            Assert.That(names, Is.EqualTo(new[]
            {
                $"CH_{key}_run_contact_v1",
                $"CH_{key}_run_passing_v1",
                $"CH_{key}_run_contact_v1",
                $"CH_{key}_run_passing_v1",
            }));
            Assert.That(AnimationUtility.GetCurveBindings(clip), Is.Empty);
        }

        [TestCase("chubby")]
        [TestCase("slacker")]
        public void FramePrefabAndDefinition_UseOneUnscaledSpriteRenderer(string key)
        {
            var prefabPath = $"Assets/_Game/Prefabs/Chickens/2D/PF_Chicken_{key}_2D_Frames.prefab";
            var definitionPath = $"Assets/_Game/Config/Chickens/CFG_Chicken2D_{key}_Frames.asset";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var definition = AssetDatabase.LoadAssetAtPath<Chicken2DVisualDefinition>(definitionPath);
            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.transform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(prefab.GetComponentsInChildren<SpriteRenderer>(true).Length, Is.EqualTo(1));
            Assert.That(prefab.GetComponent<Animator>().applyRootMotion, Is.False);
            Assert.That(definition, Is.Not.Null);
            Assert.DoesNotThrow(definition.ValidateOrThrow);
            Assert.That(definition.Prefab, Is.EqualTo(prefab));
        }
    }
}
