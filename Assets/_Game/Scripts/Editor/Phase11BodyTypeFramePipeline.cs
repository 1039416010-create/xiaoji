using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GroundChickenKing.Chickens;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GroundChickenKing.Editor
{
    public static class Phase11BodyTypeFramePipeline
    {
        private sealed class CharacterSpec
        {
            public CharacterSpec(string key, string chickenId, bool hasAuthoredLoseFrame)
            {
                Key = key;
                ChickenId = chickenId;
                HasAuthoredLoseFrame = hasAuthoredLoseFrame;
            }

            public string Key { get; }
            public string ChickenId { get; }
            public bool HasAuthoredLoseFrame { get; }
            public string DisplayKey => char.ToUpperInvariant(Key[0]) + Key.Substring(1);
            public string Root => $"Assets/_Game/Art/Characters/Production/2D/{DisplayKey}";
            public string FramesDirectory => $"{Root}/FrameAnimation/Frames";
            public string AnimationDirectory => $"{Root}/FrameAnimation/Animations";
            public string PreviewDirectory => $"{Root}/FrameAnimation/RunSequence";
            public string ControllerPath => $"{AnimationDirectory}/AC_{Key}_2D_Frames.controller";
            public string OverridePath => $"{AnimationDirectory}/AOC_{Key}_2D_Frames.overrideController";
            public string PrefabPath => $"Assets/_Game/Prefabs/Chickens/2D/PF_Chicken_{Key}_2D_Frames.prefab";
            public string DefinitionPath => $"Assets/_Game/Config/Chickens/CFG_Chicken2D_{Key}_Frames.asset";
        }

        private static readonly CharacterSpec Chubby = new CharacterSpec("chubby", "chicken-chubby", false);
        private static readonly CharacterSpec Slacker = new CharacterSpec("slacker", "chicken-slacker", true);

        private static readonly string[] RequiredFrames =
        {
            "idle", "warmup", "run_contact", "run_passing", "run_airborne", "sprint",
            "slowdown", "fall", "recover", "turn", "interfere", "celebrate",
        };

        [MenuItem("Ground Chicken King/Phase 11/Build Chubby And Slacker Frame Animation")]
        public static void BuildChubbyAndSlackerFrameAnimation()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            BuildCharacter(Chubby);
            BuildCharacter(Slacker);
            Debug.Log("PHASE11_BODY_TYPES_FRAMES_BUILD_PASS characters=2 states=22");
        }

        [MenuItem("Ground Chicken King/Phase 11/Render Chubby And Slacker Frame Runs")]
        public static void RenderChubbyAndSlackerFrameRuns()
        {
            RenderRun(Chubby);
            RenderRun(Slacker);
            Debug.Log("PHASE11_BODY_TYPES_FRAMES_RUN_PASS characters=2 frames=48");
        }

        private static void BuildCharacter(CharacterSpec spec)
        {
            EnsureAssetDirectory(spec.AnimationDirectory);
            foreach (var path in new[] { spec.ControllerPath, spec.OverridePath, spec.PrefabPath, spec.DefinitionPath })
                if (AssetDatabase.LoadMainAssetAtPath(path) != null)
                    AssetDatabase.DeleteAsset(path);

            foreach (var name in RequiredFrames)
                ConfigureFrame(FramePath(spec, name));
            if (spec.HasAuthoredLoseFrame)
                ConfigureFrame(FramePath(spec, "lose"));

            var sprites = RequiredFrames.ToDictionary(name => name, name => LoadFrame(spec, name));
            var lose = spec.HasAuthoredLoseFrame ? LoadFrame(spec, "lose") : sprites["recover"];
            var clips = new Dictionary<string, AnimationClip>
            {
                { "Idle", CreateClip(spec, "Idle", true, 0.24f, sprites["idle"]) },
                { "Warmup", CreateClip(spec, "Warmup", true, 0.16f, sprites["warmup"], sprites["idle"], sprites["warmup"]) },
                { "Run", CreateClip(spec, "Run", true, 0.13f, sprites["run_contact"], sprites["run_passing"], sprites["run_contact"], sprites["run_passing"]) },
                { "Sprint", CreateClip(spec, "Sprint", true, 0.10f, sprites["run_airborne"], sprites["sprint"]) },
                { "Stop", CreateClip(spec, "Stop", false, 0.17f, sprites["sprint"], sprites["slowdown"], sprites["idle"]) },
                { "Fall", CreateClip(spec, "Fall", false, 0.19f, sprites["slowdown"], sprites["fall"]) },
                { "Recover", CreateClip(spec, "Recover", false, 0.19f, sprites["fall"], sprites["recover"], sprites["idle"]) },
                { "Turn", CreateClip(spec, "Turn", false, 0.38f, sprites["turn"]) },
                { "Interfere", CreateClip(spec, "Interfere", false, 0.38f, sprites["interfere"]) },
                { "Celebrate", CreateClip(spec, "Celebrate", true, 0.23f, sprites["celebrate"], sprites["idle"], sprites["celebrate"]) },
                { "Lose", CreateClip(spec, "Lose", true, 0.30f, lose, sprites["slowdown"], lose) },
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

            var root = new GameObject($"PF_Chicken_{spec.Key}_2D_Frames", typeof(Animator));
            var animator = root.GetComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            var visual = new GameObject("Visual", typeof(SpriteRenderer));
            visual.transform.SetParent(root.transform, false);
            visual.GetComponent<SpriteRenderer>().sprite = sprites["idle"];
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
            serialized.FindProperty("_chickenId").stringValue = spec.ChickenId;
            serialized.FindProperty("_prefab").objectReferenceValue = prefab;
            serialized.FindProperty("_animatorOverride").objectReferenceValue = animatorOverride;
            serialized.FindProperty("_portrait").objectReferenceValue = sprites["idle"];
            serialized.FindProperty("_stageScale").floatValue = 1f;
            serialized.FindProperty("_footBaseline").floatValue = 0f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
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
            var path = $"{spec.AnimationDirectory}/{clip.name}.anim";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null)
                AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static void RenderRun(CharacterSpec spec)
        {
            const int frameCount = 24;
            const int width = 640;
            const int height = 640;
            EnsureAssetDirectory(spec.PreviewDirectory);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(spec.PrefabPath);
            if (prefab == null)
                throw new InvalidOperationException($"Build the {spec.Key} frame animation before rendering it.");
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            var animator = instance.GetComponent<Animator>();
            animator.Rebind();
            animator.Update(0f);

            var cameraObject = new GameObject($"{spec.DisplayKey}FrameRunCamera", typeof(Camera));
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.aspect = 1f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.065f, 0.08f, 1f);
            camera.allowHDR = false;
            camera.allowMSAA = true;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographicSize = 0.72f;

            for (var frame = 0; frame < frameCount; frame++)
            {
                animator.Play("Base Layer.Run", 0, frame / (float)frameCount);
                animator.Update(0.001f);
                Render(camera, $"{spec.PreviewDirectory}/CH_{spec.Key}_frame_run_{frame:00}.png", width, height);
            }

            UnityEngine.Object.DestroyImmediate(instance);
            UnityEngine.Object.DestroyImmediate(cameraObject);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
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

        private static void Render(Camera camera, string assetPath, int width, int height)
        {
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 4,
            };
            var previous = RenderTexture.active;
            camera.targetTexture = renderTexture;
            camera.Render();
            camera.Render();
            RenderTexture.active = renderTexture;
            var output = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
            output.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            output.Apply(false, false);
            File.WriteAllBytes(ToAbsolutePath(assetPath), output.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(output);
            camera.targetTexture = null;
            RenderTexture.active = previous;
            renderTexture.Release();
            UnityEngine.Object.DestroyImmediate(renderTexture);
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

        private static string ToAbsolutePath(string assetPath)
        {
            return Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length));
        }
    }
}
