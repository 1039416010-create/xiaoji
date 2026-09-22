using System.Linq;
using GroundChickenKing.Chickens;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GroundChickenKing.Tests.EditMode
{
    public sealed class Chicken2DFrameAnimationTests
    {
        private const string FramesDirectory = "Assets/_Game/Art/Characters/Production/2D/Flash/FrameAnimation/Frames";
        private const string AnimationDirectory = "Assets/_Game/Art/Characters/Production/2D/Flash/FrameAnimation/Animations";
        private const string PrefabPath = "Assets/_Game/Prefabs/Chickens/2D/PF_Chicken_flash_2D_Frames.prefab";
        private const string DefinitionPath = "Assets/_Game/Config/Chickens/CFG_Chicken2D_flash_Frames.asset";

        [Test]
        public void ApprovedMotionSheet_ProducesTwelveFixedSizeFrames()
        {
            var guids = AssetDatabase.FindAssets("t:Sprite", new[] { FramesDirectory });
            Assert.That(guids.Length, Is.EqualTo(12));
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                Assert.That(texture.width, Is.EqualTo(362), path);
                Assert.That(texture.height, Is.EqualTo(362), path);
            }
        }

        [Test]
        public void RunClip_UsesAuthoredContactPassingAndAirborneFrames()
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimationDirectory}/AN_flash_Run_2D_Frames.anim");
            Assert.That(clip, Is.Not.Null);
            var binding = AnimationUtility.GetObjectReferenceCurveBindings(clip).Single();
            var names = AnimationUtility.GetObjectReferenceCurve(clip, binding)
                .Select(key => key.value.name).ToArray();
            Assert.That(names, Is.EqualTo(new[]
            {
                "CH_flash_run_contact_v1", "CH_flash_run_passing_v1",
                "CH_flash_run_airborne_v1", "CH_flash_run_passing_v1",
            }));
            Assert.That(AnimationUtility.GetCurveBindings(clip).Any(binding => binding.propertyName == "m_LocalPosition.x"), Is.False);
        }

        [Test]
        public void FramePrefabAndDefinition_UseSingleSpriteRenderer()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var definition = AssetDatabase.LoadAssetAtPath<Chicken2DVisualDefinition>(DefinitionPath);
            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponentsInChildren<SpriteRenderer>(true).Length, Is.EqualTo(1));
            Assert.That(prefab.GetComponent<Animator>().applyRootMotion, Is.False);
            Assert.That(definition, Is.Not.Null);
            Assert.DoesNotThrow(definition.ValidateOrThrow);
            Assert.That(definition.Prefab, Is.EqualTo(prefab));
        }
    }
}
