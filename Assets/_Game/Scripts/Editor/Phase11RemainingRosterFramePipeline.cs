using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GroundChickenKing.Chickens;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace GroundChickenKing.Editor
{
    public static class Phase11RemainingRosterFramePipeline
    {
        private sealed class CharacterSpec
        {
            public CharacterSpec(string key, float runSecondsPerFrame, float sprintSecondsPerFrame)
            {
                Key = key;
                RunSecondsPerFrame = runSecondsPerFrame;
                SprintSecondsPerFrame = sprintSecondsPerFrame;
            }

            public string Key { get; }
            public float RunSecondsPerFrame { get; }
            public float SprintSecondsPerFrame { get; }
            public string DisplayKey => char.ToUpperInvariant(Key[0]) + Key.Substring(1);
            public string Root => $"Assets/_Game/Art/Characters/Production/2D/{DisplayKey}";
            public string FramesDirectory => $"{Root}/FrameAnimation/Frames";
            public string AnimationDirectory => $"{Root}/FrameAnimation/Animations";
            public string ControllerPath => $"{AnimationDirectory}/AC_{Key}_2D_Frames.controller";
            public string OverridePath => $"{AnimationDirectory}/AOC_{Key}_2D_Frames.overrideController";
            public string MaterialPath => $"Assets/_Game/Art/Materials/MAT_Chicken2D_{Key}_Unlit.mat";
            public string PrefabPath => $"Assets/_Game/Prefabs/Chickens/2D/PF_Chicken_{Key}_2D_Frames.prefab";
            public string DefinitionPath => $"Assets/_Game/Config/Chickens/CFG_Chicken2D_{Key}_Frames.asset";
        }

        private static readonly CharacterSpec[] Characters =
        {
            new CharacterSpec("bro", 0.2125f, 0.30f),
            new CharacterSpec("captain", 0.20f, 0.30f),
            new CharacterSpec("dumpling", 0.25f, 0.36f),
            new CharacterSpec("lucky", 0.2125f, 0.30f),
            new CharacterSpec("ninja", 0.1625f, 0.225f),
            new CharacterSpec("rocket", 0.175f, 0.24f),
            new CharacterSpec("scholar", 0.225f, 0.34f),
            new CharacterSpec("sleepy", 0.275f, 0.325f),
            new CharacterSpec("thunder", 0.2125f, 0.275f),
            new CharacterSpec("tiny", 0.15f, 0.225f),
        };

        private static readonly string[] RequiredFrames =
        {
            "idle", "warmup", "run_contact", "run_passing", "run_airborne", "sprint",
            "slowdown", "fall", "recover", "turn", "interfere", "celebrate", "lose",
        };

        [MenuItem("Ground Chicken King/Phase 11/Build Remaining Roster Frame Animation")]
        public static void BuildRemainingRosterFrameAnimation()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (var spec in Characters)
                BuildCharacter(spec);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("PHASE11_REMAINING_ROSTER_FRAMES_BUILD_PASS characters=10 states=110");
        }

        private static void BuildCharacter(CharacterSpec spec)
        {
            EnsureAssetDirectory(spec.AnimationDirectory);
            foreach (var path in new[]
                     {
                         spec.ControllerPath, spec.OverridePath, spec.MaterialPath,
                         spec.PrefabPath, spec.DefinitionPath,
                     })
                if (AssetDatabase.LoadMainAssetAtPath(path) != null)
                    AssetDatabase.DeleteAsset(path);

            foreach (var name in RequiredFrames)
                ConfigureFrame(FramePath(spec, name));

            var sprites = RequiredFrames.ToDictionary(name => name, name => LoadFrame(spec, name));
            var clips = new Dictionary<string, AnimationClip>
            {
                { "Idle", CreateClip(spec, "Idle", true, 0.24f, sprites["idle"]) },
                { "Warmup", CreateClip(spec, "Warmup", true, 0.16f, sprites["warmup"], sprites["idle"], sprites["warmup"]) },
                { "Run", CreateClip(spec, "Run", true, spec.RunSecondsPerFrame, sprites["run_contact"], sprites["run_passing"], sprites["run_contact"], sprites["run_passing"]) },
                { "Sprint", CreateClip(spec, "Sprint", true, spec.SprintSecondsPerFrame, sprites["run_airborne"], sprites["sprint"]) },
                { "Stop", CreateClip(spec, "Stop", false, 0.17f, sprites["sprint"], sprites["slowdown"], sprites["idle"]) },
                { "Fall", CreateClip(spec, "Fall", false, 0.19f, sprites["slowdown"], sprites["fall"]) },
                { "Recover", CreateClip(spec, "Recover", false, 0.19f, sprites["fall"], sprites["recover"], sprites["idle"]) },
                { "Turn", CreateClip(spec, "Turn", false, 0.38f, sprites["turn"]) },
                { "Interfere", CreateClip(spec, "Interfere", false, 0.38f, sprites["interfere"]) },
                { "Celebrate", CreateClip(spec, "Celebrate", true, 0.23f, sprites["celebrate"], sprites["idle"], sprites["celebrate"]) },
                { "Lose", CreateClip(spec, "Lose", true, 0.30f, sprites["lose"], sprites["slowdown"], sprites["lose"]) },
            };

            var controller = AnimatorController.CreateAnimatorControllerAtPath(spec.ControllerPath);
            var stateMachine = controller.layers[0].stateMachine;
            stateMachine.states = Array.Empty<ChildAnimatorState>();
            foreach (var pair in clips)
            {
                var state = stateMachine.AddState(pair.Key);
                state.motion = pair.Value;
                state.writeDefaultValues = true;
                if (pair.Key == "Idle")
                    stateMachine.defaultState = state;
            }

            var sourceMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/_Game/Art/Materials/MAT_Chicken2D_slacker_Unlit.mat");
            if (sourceMaterial == null)
                throw new InvalidOperationException("The approved 2D unlit source material is missing.");
            var material = new Material(sourceMaterial.shader) { name = $"MAT_Chicken2D_{spec.Key}_Unlit" };
            AssetDatabase.CreateAsset(material, spec.MaterialPath);

            var root = new GameObject($"PF_Chicken_{spec.Key}_2D_Frames", typeof(Animator));
            var animator = root.GetComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            var visual = new GameObject("Visual", typeof(SpriteRenderer));
            visual.transform.SetParent(root.transform, false);
            var renderer = visual.GetComponent<SpriteRenderer>();
            renderer.sprite = sprites["idle"];
            renderer.sharedMaterial = material;
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, spec.PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            if (prefab == null)
                throw new InvalidOperationException($"The {spec.Key} frame-animation prefab could not be saved.");

            var animatorOverride = new AnimatorOverrideController(controller)
            {
                name = $"AOC_{spec.Key}_2D_Frames",
            };
            AssetDatabase.CreateAsset(animatorOverride, spec.OverridePath);
            var definition = ScriptableObject.CreateInstance<Chicken2DVisualDefinition>();
            definition.name = $"CFG_Chicken2D_{spec.Key}_Frames";
            AssetDatabase.CreateAsset(definition, spec.DefinitionPath);
            var serialized = new SerializedObject(definition);
            serialized.FindProperty("_chickenId").stringValue = $"chicken-{spec.Key}";
            serialized.FindProperty("_prefab").objectReferenceValue = prefab;
            serialized.FindProperty("_animatorOverride").objectReferenceValue = animatorOverride;
            serialized.FindProperty("_portrait").objectReferenceValue = sprites["idle"];
            serialized.FindProperty("_stageScale").floatValue = 1f;
            serialized.FindProperty("_footBaseline").floatValue = 0f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }

        private static AnimationClip CreateClip(
            CharacterSpec spec,
            string stateName,
            bool loop,
            float secondsPerFrame,
            params Sprite[] sprites)
        {
            var clip = new AnimationClip
            {
                name = $"AN_{spec.Key}_{stateName}_2D_Frames",
                frameRate = 30f,
            };
            var binding = EditorCurveBinding.PPtrCurve("Visual", typeof(SpriteRenderer), "m_Sprite");
            var keys = new ObjectReferenceKeyframe[sprites.Length];
            for (var index = 0; index < sprites.Length; index++)
                keys[index] = new ObjectReferenceKeyframe { time = index * secondsPerFrame, value = sprites[index] };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            settings.stopTime = sprites.Length * secondsPerFrame;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, $"{spec.AnimationDirectory}/{clip.name}.anim");
            return clip;
        }

        private static void ConfigureFrame(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                throw new InvalidOperationException($"Missing full-character animation frame: {assetPath}");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.spritePixelsPerUnit = 384f;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(0.5f, 0.5f);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static Sprite LoadFrame(CharacterSpec spec, string name)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(FramePath(spec, name));
            if (sprite == null)
                throw new InvalidOperationException($"Missing {spec.Key} animation frame: {name}");
            return sprite;
        }

        private static string FramePath(CharacterSpec spec, string name)
        {
            return $"{spec.FramesDirectory}/CH_{spec.Key}_{name}_v1.png";
        }

        private static void EnsureAssetDirectory(string assetDirectory)
        {
            if (AssetDatabase.IsValidFolder(assetDirectory))
                return;
            var parent = Path.GetDirectoryName(assetDirectory)?.Replace('\\', '/');
            EnsureAssetDirectory(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(assetDirectory));
        }
    }
}
