using System;
using System.Collections.Generic;
using System.IO;
using GroundChickenKing.Chickens;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GroundChickenKing.Editor
{
    public static class Phase11ExactSpritePipeline
    {
        private const string MasterPath = "Assets/_Game/Art/Characters/Production/2D/Flash/Master/CH_flash_side_master_v1.png";
        private const string AnimationDirectory = "Assets/_Game/Art/Characters/Production/2D/Flash/Exact/Animations";
        private const string PreviewDirectory = "Assets/_Game/Art/Characters/Production/2D/Flash/Exact/RunSequence";
        private const string ControllerPath = AnimationDirectory + "/AC_flash_2D_Exact.controller";
        private const string OverridePath = AnimationDirectory + "/AOC_flash_2D_Exact.overrideController";
        private const string PrefabPath = "Assets/_Game/Prefabs/Chickens/2D/PF_Chicken_flash_2D_Exact.prefab";
        private const string DefinitionPath = "Assets/_Game/Config/Chickens/CFG_Chicken2D_flash_Exact.asset";

        [MenuItem("Ground Chicken King/Phase 11/Build Flash Exact Sprite")]
        public static void BuildFlashExactSprite()
        {
            EnsureAssetDirectory(AnimationDirectory);
            if (AssetDatabase.LoadMainAssetAtPath(ControllerPath) != null)
                AssetDatabase.DeleteAsset(ControllerPath);
            if (AssetDatabase.LoadMainAssetAtPath(OverridePath) != null)
                AssetDatabase.DeleteAsset(OverridePath);
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
                AssetDatabase.DeleteAsset(PrefabPath);
            if (AssetDatabase.LoadMainAssetAtPath(DefinitionPath) != null)
                AssetDatabase.DeleteAsset(DefinitionPath);

            var clips = new Dictionary<string, AnimationClip>
            {
                { "Idle", CreateClip("Idle", 2.4f, true, new[] { 0f, 0.012f, 0f }, new[] { 0f, 0.35f, 0f }) },
                { "Warmup", CreateClip("Warmup", 1.6f, true, new[] { 0f, -0.025f, 0.012f, -0.025f, 0f }, new[] { 0f, 1.2f, -0.8f, 1.2f, 0f }) },
                { "Run", CreateClip("Run", 0.65f, true, new[] { 0f, 0.045f, 0f, 0.045f, 0f }, new[] { 0.6f, -0.8f, 0.6f, -0.8f, 0.6f }) },
                { "Sprint", CreateClip("Sprint", 0.48f, true, new[] { 0f, 0.065f, 0f, 0.065f, 0f }, new[] { 2.2f, 0.2f, 2.2f, 0.2f, 2.2f }) },
                { "Stop", CreateClip("Stop", 0.7f, false, new[] { 0f, -0.035f, 0f }, new[] { 2f, -2f, 0f }) },
                { "Fall", CreateClip("Fall", 0.7f, false, new[] { 0f, 0.05f, -0.38f }, new[] { 0f, -24f, -78f }) },
                { "Recover", CreateClip("Recover", 0.75f, false, new[] { -0.38f, -0.12f, 0f }, new[] { -78f, -30f, 0f }) },
                { "Turn", CreateTurnClip() },
                { "Interfere", CreateClip("Interfere", 0.7f, false, new[] { 0f, 0.035f, 0f }, new[] { 0f, -4f, 0f }) },
                { "Celebrate", CreateClip("Celebrate", 1.15f, true, new[] { 0f, 0.12f, 0f, 0.075f, 0f }, new[] { 0f, -2f, 1.5f, -1f, 0f }) },
                { "Lose", CreateClip("Lose", 1.8f, true, new[] { 0f, -0.035f, -0.02f, -0.035f, 0f }, new[] { 0f, 2.5f, 3.5f, 2.5f, 0f }) },
            };

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
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

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(MasterPath);
            if (sprite == null)
                throw new InvalidOperationException("The approved Flash master must be imported as a Sprite.");
            var root = new GameObject("PF_Chicken_flash_2D_Exact", typeof(Animator));
            var animator = root.GetComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            var artRoot = new GameObject("ArtRoot");
            artRoot.transform.SetParent(root.transform, false);
            var visual = new GameObject("ApprovedMaster", typeof(SpriteRenderer));
            visual.transform.SetParent(artRoot.transform, false);
            visual.GetComponent<SpriteRenderer>().sprite = sprite;
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            if (prefab == null)
                throw new InvalidOperationException("The exact Flash prefab could not be saved.");
            UnityEngine.Object.DestroyImmediate(root);

            var animatorOverride = new AnimatorOverrideController(controller) { name = "AOC_flash_2D_Exact" };
            AssetDatabase.CreateAsset(animatorOverride, OverridePath);
            var definition = ScriptableObject.CreateInstance<Chicken2DVisualDefinition>();
            definition.name = "CFG_Chicken2D_flash_Exact";
            AssetDatabase.CreateAsset(definition, DefinitionPath);
            var serialized = new SerializedObject(definition);
            serialized.FindProperty("_chickenId").stringValue = "chicken-flash";
            serialized.FindProperty("_prefab").objectReferenceValue = prefab;
            serialized.FindProperty("_animatorOverride").objectReferenceValue = animatorOverride;
            serialized.FindProperty("_portrait").objectReferenceValue = sprite;
            serialized.FindProperty("_stageScale").floatValue = 1f;
            serialized.FindProperty("_footBaseline").floatValue = 0f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            Debug.Log("PHASE11_FLASH_EXACT_BUILD_PASS renderers=1 source=approved-master");
        }

        [MenuItem("Ground Chicken King/Phase 11/Render Flash Exact Run")]
        public static void RenderFlashExactRun()
        {
            const int frameCount = 20;
            const int width = 640;
            const int height = 780;
            EnsureAssetDirectory(PreviewDirectory);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
                throw new InvalidOperationException("Build the exact Flash prefab before rendering it.");
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            instance.transform.localScale = new Vector3(-1f, 1f, 1f);
            var animator = instance.GetComponent<Animator>();
            animator.Rebind();
            animator.Update(0f);

            var cameraObject = new GameObject("FlashExactCamera", typeof(Camera));
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.aspect = width / (float)height;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.065f, 0.08f, 1f);
            camera.allowHDR = false;
            camera.allowMSAA = true;

            var bounds = default(Bounds);
            for (var frame = 0; frame < frameCount; frame++)
            {
                Sample(animator, frame / (float)frameCount);
                var frameBounds = instance.GetComponentInChildren<SpriteRenderer>().bounds;
                if (frame == 0) bounds = frameBounds;
                else bounds.Encapsulate(frameBounds);
            }
            camera.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);
            camera.orthographicSize = Mathf.Max(bounds.extents.y * 1.16f, bounds.extents.x * 1.16f / camera.aspect);

            for (var frame = 0; frame < frameCount; frame++)
            {
                Sample(animator, frame / (float)frameCount);
                Render(camera, $"{PreviewDirectory}/CH_flash_exact_run_{frame:00}.png", width, height);
            }

            UnityEngine.Object.DestroyImmediate(instance);
            UnityEngine.Object.DestroyImmediate(cameraObject);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log($"PHASE11_FLASH_EXACT_RUN_PASS frames={frameCount}");
        }

        private static AnimationClip CreateClip(string stateName, float length, bool loop, float[] y, float[] rotation)
        {
            var clip = NewClip(stateName, length, loop);
            Curve(clip, "ArtRoot", "m_LocalPosition.y", y);
            Curve(clip, "ArtRoot", "localEulerAnglesRaw.z", rotation);
            return SaveClip(clip);
        }

        private static AnimationClip CreateTurnClip()
        {
            var clip = NewClip("Turn", 0.65f, false);
            Curve(clip, "ArtRoot", "m_LocalScale.x", new[] { 1f, 0.08f, -1f, 0.08f, 1f });
            Curve(clip, "ArtRoot", "m_LocalPosition.y", new[] { 0f, 0.04f, 0f });
            return SaveClip(clip);
        }

        private static AnimationClip NewClip(string stateName, float length, bool loop)
        {
            var clip = new AnimationClip { name = $"AN_flash_{stateName}_2D_Exact", frameRate = 30f };
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            settings.stopTime = length;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            return clip;
        }

        private static AnimationClip SaveClip(AnimationClip clip)
        {
            var path = $"{AnimationDirectory}/{clip.name}.anim";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null)
                AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static void Curve(AnimationClip clip, string path, string property, float[] values)
        {
            var length = AnimationUtility.GetAnimationClipSettings(clip).stopTime;
            var keys = new Keyframe[values.Length];
            for (var index = 0; index < values.Length; index++)
                keys[index] = new Keyframe(length * index / (values.Length - 1), values[index]);
            clip.SetCurve(path, typeof(Transform), property, new AnimationCurve(keys));
        }

        private static void Sample(Animator animator, float normalizedTime)
        {
            animator.Play("Base Layer.Run", 0, normalizedTime);
            animator.Update(0.001f);
        }

        private static void Render(Camera camera, string assetPath, int width, int height)
        {
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 4,
                filterMode = FilterMode.Bilinear,
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

        private static void EnsureAssetDirectory(string assetDirectory)
        {
            if (AssetDatabase.IsValidFolder(assetDirectory)) return;
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
