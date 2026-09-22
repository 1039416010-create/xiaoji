using System.Linq;
using GroundChickenKing.Chickens;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GroundChickenKing.Tests.EditMode
{
    public sealed class Chicken2DExactArtTests
    {
        private const string MasterPath = "Assets/_Game/Art/Characters/Production/2D/Flash/Master/CH_flash_side_master_v1.png";
        private const string PrefabPath = "Assets/_Game/Prefabs/Chickens/2D/PF_Chicken_flash_2D_Exact.prefab";
        private const string RunClipPath = "Assets/_Game/Art/Characters/Production/2D/Flash/Exact/Animations/AN_flash_Run_2D_Exact.anim";
        private const string DefinitionPath = "Assets/_Game/Config/Chickens/CFG_Chicken2D_flash_Exact.asset";

        [Test]
        public void ExactPrefab_UsesOnlyApprovedMasterSprite()
        {
            var master = AssetDatabase.LoadAssetAtPath<Sprite>(MasterPath);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(master, Is.Not.Null);
            Assert.That(prefab, Is.Not.Null);
            var renderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
            Assert.That(renderers.Length, Is.EqualTo(1));
            Assert.That(renderers[0].sprite, Is.SameAs(master));
            Assert.That(renderers[0].transform.localPosition, Is.EqualTo(Vector3.zero));
            Assert.That(renderers[0].transform.localScale, Is.EqualTo(Vector3.one));
        }

        [Test]
        public void ExactRun_DoesNotChangeScaleOrForwardPosition()
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(RunClipPath);
            Assert.That(clip, Is.Not.Null);
            var bindings = AnimationUtility.GetCurveBindings(clip);
            Assert.That(bindings.Any(binding => binding.propertyName == "m_LocalPosition.y"), Is.True);
            foreach (var binding in bindings.Where(binding => binding.propertyName == "m_LocalPosition.x"))
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                Assert.That(curve.keys.All(key => Mathf.Abs(key.value) < 0.0001f), Is.True);
            }
            Assert.That(bindings.Any(binding => binding.propertyName.StartsWith("m_LocalScale")), Is.False);
        }

        [Test]
        public void ExactDefinition_ReferencesExactPrefabAndApprovedPortrait()
        {
            var definition = AssetDatabase.LoadAssetAtPath<Chicken2DVisualDefinition>(DefinitionPath);
            var master = AssetDatabase.LoadAssetAtPath<Sprite>(MasterPath);
            Assert.That(definition, Is.Not.Null);
            Assert.DoesNotThrow(definition.ValidateOrThrow);
            Assert.That(definition.Prefab, Is.EqualTo(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath)));
            Assert.That(definition.Portrait, Is.SameAs(master));
        }
    }
}
