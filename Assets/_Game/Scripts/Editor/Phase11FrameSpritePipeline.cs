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
    public static class Phase11FrameSpritePipeline
    {
        private const string SourcePath = "Assets/_Game/Art/Characters/Production/2D/Flash/MotionReference/CH_flash_motion_keyposes_v1.png";
        private const string FramesDirectory = "Assets/_Game/Art/Characters/Production/2D/Flash/FrameAnimation/Frames";
        private const string AnimationDirectory = "Assets/_Game/Art/Characters/Production/2D/Flash/FrameAnimation/Animations";
        private const string PreviewDirectory = "Assets/_Game/Art/Characters/Production/2D/Flash/FrameAnimation/RunSequence";
        private const string ControllerPath = AnimationDirectory + "/AC_flash_2D_Frames.controller";
        private const string OverridePath = AnimationDirectory + "/AOC_flash_2D_Frames.overrideController";
        private const string PrefabPath = "Assets/_Game/Prefabs/Chickens/2D/PF_Chicken_flash_2D_Frames.prefab";
        private const string DefinitionPath = "Assets/_Game/Config/Chickens/CFG_Chicken2D_flash_Frames.asset";
        private const byte AlphaThreshold = 18;

        private static readonly string[] FrameNames =
        {
            "idle", "warmup", "run_contact", "run_passing",
            "run_airborne", "sprint", "slowdown", "fall",
            "recover", "turn", "interfere", "celebrate",
        };

        [MenuItem("Ground Chicken King/Phase 11/Build Flash Frame Animation")]
        public static void BuildFlashFrameAnimation()
        {
            ExtractFrames();
            EnsureAssetDirectory(AnimationDirectory);
            foreach (var path in new[] { ControllerPath, OverridePath, PrefabPath, DefinitionPath })
                if (AssetDatabase.LoadMainAssetAtPath(path) != null)
                    AssetDatabase.DeleteAsset(path);

            var sprites = FrameNames.ToDictionary(name => name, LoadFrame);
            var clips = new Dictionary<string, AnimationClip>
            {
                { "Idle", CreateClip("Idle", true, 0.24f, sprites["idle"]) },
                { "Warmup", CreateClip("Warmup", true, 0.14f, sprites["warmup"], sprites["idle"], sprites["warmup"]) },
                { "Run", CreateClip("Run", true, 0.12f, sprites["run_contact"], sprites["run_passing"], sprites["run_airborne"], sprites["run_passing"]) },
                { "Sprint", CreateClip("Sprint", true, 0.09f, sprites["run_airborne"], sprites["sprint"]) },
                { "Stop", CreateClip("Stop", false, 0.16f, sprites["sprint"], sprites["slowdown"], sprites["idle"]) },
                { "Fall", CreateClip("Fall", false, 0.18f, sprites["slowdown"], sprites["fall"]) },
                { "Recover", CreateClip("Recover", false, 0.18f, sprites["fall"], sprites["recover"], sprites["idle"]) },
                { "Turn", CreateClip("Turn", false, 0.4f, sprites["turn"]) },
                { "Interfere", CreateClip("Interfere", false, 0.4f, sprites["interfere"]) },
                { "Celebrate", CreateClip("Celebrate", true, 0.22f, sprites["celebrate"], sprites["idle"], sprites["celebrate"]) },
                { "Lose", CreateClip("Lose", true, 0.28f, sprites["slowdown"], sprites["recover"], sprites["slowdown"]) },
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

            var root = new GameObject("PF_Chicken_flash_2D_Frames", typeof(Animator));
            var animator = root.GetComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            var visual = new GameObject("Visual", typeof(SpriteRenderer));
            visual.transform.SetParent(root.transform, false);
            visual.GetComponent<SpriteRenderer>().sprite = sprites["idle"];
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            if (prefab == null)
                throw new InvalidOperationException("The Flash frame-animation prefab could not be saved.");

            var animatorOverride = new AnimatorOverrideController(controller) { name = "AOC_flash_2D_Frames" };
            AssetDatabase.CreateAsset(animatorOverride, OverridePath);
            var definition = ScriptableObject.CreateInstance<Chicken2DVisualDefinition>();
            definition.name = "CFG_Chicken2D_flash_Frames";
            AssetDatabase.CreateAsset(definition, DefinitionPath);
            var serialized = new SerializedObject(definition);
            serialized.FindProperty("_chickenId").stringValue = "chicken-flash";
            serialized.FindProperty("_prefab").objectReferenceValue = prefab;
            serialized.FindProperty("_animatorOverride").objectReferenceValue = animatorOverride;
            serialized.FindProperty("_portrait").objectReferenceValue = sprites["idle"];
            serialized.FindProperty("_stageScale").floatValue = 1f;
            serialized.FindProperty("_footBaseline").floatValue = 0f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            Debug.Log("PHASE11_FLASH_FRAMES_BUILD_PASS frames=12 states=11");
        }

        [MenuItem("Ground Chicken King/Phase 11/Render Flash Frame Run")]
        public static void RenderFlashFrameRun()
        {
            const int frameCount = 24;
            const int width = 640;
            const int height = 640;
            EnsureAssetDirectory(PreviewDirectory);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
                throw new InvalidOperationException("Build the Flash frame animation before rendering it.");
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            instance.transform.localScale = new Vector3(-1f, 1f, 1f);
            var animator = instance.GetComponent<Animator>();
            animator.Rebind();
            animator.Update(0f);

            var cameraObject = new GameObject("FlashFrameRunCamera", typeof(Camera));
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.aspect = 1f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.065f, 0.08f, 1f);
            camera.allowHDR = false;
            camera.allowMSAA = true;
            var renderer = instance.GetComponentInChildren<SpriteRenderer>();
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographicSize = renderer.sprite.bounds.extents.y * 1.08f;

            for (var frame = 0; frame < frameCount; frame++)
            {
                animator.Play("Base Layer.Run", 0, frame / (float)frameCount);
                animator.Update(0.001f);
                Render(camera, $"{PreviewDirectory}/CH_flash_frame_run_{frame:00}.png", width, height);
            }

            UnityEngine.Object.DestroyImmediate(instance);
            UnityEngine.Object.DestroyImmediate(cameraObject);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log($"PHASE11_FLASH_FRAMES_RUN_PASS frames={frameCount}");
        }

        private static void ExtractFrames()
        {
            var absoluteSource = ToAbsolutePath(SourcePath);
            if (!File.Exists(absoluteSource))
                throw new FileNotFoundException("The approved Flash motion sheet is missing.", absoluteSource);
            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
            if (!source.LoadImage(File.ReadAllBytes(absoluteSource), false))
                throw new InvalidOperationException("The approved Flash motion sheet could not be decoded.");
            if (source.width % 4 != 0 || source.height % 3 != 0)
                throw new InvalidOperationException("The approved Flash motion sheet must remain a 4 by 3 grid.");
            EnsureAssetDirectory(FramesDirectory);
            var cellWidth = source.width / 4;
            var cellHeight = source.height / 3;
            var sourcePixels = source.GetPixels32();
            for (var index = 0; index < FrameNames.Length; index++)
            {
                var column = index % 4;
                var row = index / 4;
                var xMin = column * cellWidth;
                var yMin = source.height - (row + 1) * cellHeight;
                var pixels = new Color32[cellWidth * cellHeight];
                for (var y = 0; y < cellHeight; y++)
                    Array.Copy(sourcePixels, (yMin + y) * source.width + xMin, pixels, y * cellWidth, cellWidth);
                KeepLargestConnectedComponent(pixels, cellWidth, cellHeight);
                var output = new Texture2D(cellWidth, cellHeight, TextureFormat.RGBA32, false, false);
                output.SetPixels32(pixels);
                output.Apply(false, false);
                File.WriteAllBytes(ToAbsolutePath(FramePath(FrameNames[index])), output.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(output);
            }
            UnityEngine.Object.DestroyImmediate(source);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (var name in FrameNames)
                ConfigureFrame(FramePath(name));
        }

        private static void KeepLargestConnectedComponent(Color32[] pixels, int width, int height)
        {
            var visited = new bool[pixels.Length];
            var queue = new Queue<int>();
            var largest = new List<int>();
            for (var start = 0; start < pixels.Length; start++)
            {
                if (visited[start] || pixels[start].a < AlphaThreshold) continue;
                var component = new List<int>();
                visited[start] = true;
                queue.Enqueue(start);
                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    component.Add(current);
                    var x = current % width;
                    var y = current / width;
                    Visit(x - 1, y);
                    Visit(x + 1, y);
                    Visit(x, y - 1);
                    Visit(x, y + 1);

                    void Visit(int nextX, int nextY)
                    {
                        if (nextX < 0 || nextX >= width || nextY < 0 || nextY >= height) return;
                        var next = nextY * width + nextX;
                        if (visited[next] || pixels[next].a < AlphaThreshold) return;
                        visited[next] = true;
                        queue.Enqueue(next);
                    }
                }
                if (component.Count > largest.Count) largest = component;
            }
            var keep = new bool[pixels.Length];
            foreach (var index in largest) keep[index] = true;
            for (var index = 0; index < pixels.Length; index++)
            {
                if (!keep[index]) pixels[index] = new Color32(0, 0, 0, 0);
            }
        }

        private static void ConfigureFrame(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) throw new InvalidOperationException($"Missing TextureImporter: {assetPath}");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.spritePixelsPerUnit = 360f;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(0.5f, 0.5f);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static Sprite LoadFrame(string name)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(FramePath(name));
            if (sprite == null) throw new InvalidOperationException($"Missing Flash animation frame: {name}");
            return sprite;
        }

        private static AnimationClip CreateClip(string stateName, bool loop, float secondsPerFrame, params Sprite[] sprites)
        {
            var clip = new AnimationClip { name = $"AN_flash_{stateName}_2D_Frames", frameRate = 30f };
            var binding = EditorCurveBinding.PPtrCurve("Visual", typeof(SpriteRenderer), "m_Sprite");
            var keys = new ObjectReferenceKeyframe[sprites.Length];
            for (var index = 0; index < sprites.Length; index++)
                keys[index] = new ObjectReferenceKeyframe { time = index * secondsPerFrame, value = sprites[index] };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            settings.stopTime = sprites.Length * secondsPerFrame;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            var path = $"{AnimationDirectory}/{clip.name}.anim";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static void Render(Camera camera, string assetPath, int width, int height)
        {
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
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

        private static string FramePath(string name) => $"{FramesDirectory}/CH_flash_{name}_v1.png";

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
