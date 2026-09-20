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
    public static class Phase11TwoDSpritePipeline
    {
        private const string SourcePath = "Assets/_Game/Art/Characters/Production/2D/Flash/PartsReference/CH_flash_parts_sheet_v1.png";
        private const string LowerLegReferencePath = "Assets/_Game/Art/Characters/Production/2D/Flash/PartsReference/CH_flash_lower_leg_foot_reference_v1.png";
        private const string OutputDirectory = "Assets/_Game/Art/Characters/Production/2D/Flash/Parts";
        private const string AnimationDirectory = "Assets/_Game/Art/Characters/Production/2D/Flash/Animations";
        private const string PrefabPath = "Assets/_Game/Prefabs/Chickens/2D/PF_Chicken_flash_2D_WIP.prefab";
        private const string ControllerPath = AnimationDirectory + "/AC_flash_2D_WIP.controller";
        private const string OverridePath = AnimationDirectory + "/AOC_flash_2D_WIP.overrideController";
        private const string DefinitionPath = "Assets/_Game/Config/Chickens/CFG_Chicken2D_flash_WIP.asset";
        private const string MasterPath = "Assets/_Game/Art/Characters/Production/2D/Flash/Master/CH_flash_side_master_v1.png";
        private const string PreviewDirectory = "Assets/_Game/Art/Characters/Production/2D/Flash/Preview";
        private const int GridSize = 4;
        private const byte AlphaThreshold = 18;
        private const int Padding = 10;

        private static readonly PartSpec[] Parts =
        {
            new("torso", 1),
            new("head_neck", 1),
            new("wing_near", 1),
            new("wing_far", 1),
            new("tail", 1),
            new("upper_leg_near", 1),
            new("lower_leg_foot_near", 1),
            new("leg_far", 1),
            new("eye_white", 1),
            new("iris_pupil", 1),
            new("eyelids", 2),
            new("brow", 1),
            new("beaks", 2),
            new("crest", 1),
            new("headband", 1),
            new("ribbons", 2),
        };

        [MenuItem("Ground Chicken King/Phase 11/Build Flash 2D Parts")]
        public static void BuildFlashParts()
        {
            var absoluteSource = ToAbsolutePath(SourcePath);
            if (!File.Exists(absoluteSource))
                throw new FileNotFoundException("Flash parts sheet is missing.", absoluteSource);

            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
            if (!source.LoadImage(File.ReadAllBytes(absoluteSource), false))
                throw new InvalidOperationException("Flash parts sheet could not be decoded.");

            Directory.CreateDirectory(ToAbsolutePath(OutputDirectory));
            var reports = new List<PartReport>(Parts.Length);
            for (var index = 0; index < Parts.Length; index++)
                reports.Add(ExtractPart(source, index, Parts[index]));
            UnityEngine.Object.DestroyImmediate(source);
            InstallReferencePart(LowerLegReferencePath, "lower_leg_foot_near", reports);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (var report in reports)
                ConfigureSpriteImporter(report.AssetPath);
            ConfigureSpriteImporter(MasterPath);
            AssetDatabase.SaveAssets();

            var reportPath = $"{OutputDirectory}/flash_parts_report.json";
            var reportJson = JsonUtility.ToJson(new PipelineReport
            {
                Source = SourcePath,
                AlphaThreshold = AlphaThreshold,
                Parts = reports.ToArray(),
            }, true);
            File.WriteAllText(ToAbsolutePath(reportPath), reportJson);
            AssetDatabase.ImportAsset(reportPath, ImportAssetOptions.ForceSynchronousImport);
            BuildFlashPrefabAndAnimations();
            Debug.Log($"PHASE11_2D_FLASH_PARTS_PASS parts={reports.Count}");
        }

        [MenuItem("Ground Chicken King/Phase 11/Render Flash 2D Previews")]
        public static void RenderFlashPreviews()
        {
            EnsureAssetDirectory(PreviewDirectory);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
                throw new InvalidOperationException("Build the Flash 2D prefab before rendering previews.");

            var cameraObject = new GameObject("Flash2DPreviewCamera", typeof(Camera));
            var camera = cameraObject.GetComponent<Camera>();
            camera.transform.rotation = Quaternion.identity;
            camera.orthographic = true;
            camera.aspect = 900f / 1100f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.065f, 0.08f, 1f);
            camera.allowHDR = false;
            camera.allowMSAA = true;

            var previews = new[]
            {
                ("Idle", 0.18f),
                ("Run", 0.22f),
                ("Sprint", 0.22f),
                ("Fall", 0.82f),
                ("Recover", 0.72f),
            };
            foreach (var preview in previews)
            {
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance == null)
                    throw new InvalidOperationException("Flash 2D prefab could not be instantiated for preview.");
                instance.transform.localScale = new Vector3(-1f, 1f, 1f);
                var animator = instance.GetComponent<Animator>();
                animator.Rebind();
                animator.Update(0f);
                animator.Play($"Base Layer.{preview.Item1}", 0, preview.Item2);
                animator.Update(0.001f);
                FrameCamera(camera, instance);
                RenderPreview(camera, $"{PreviewDirectory}/CH_flash_2D_WIP_{preview.Item1.ToLowerInvariant()}.png");
                UnityEngine.Object.DestroyImmediate(instance);
            }
            UnityEngine.Object.DestroyImmediate(cameraObject);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("PHASE11_2D_FLASH_PREVIEW_PASS views=5");
        }

        private static void FrameCamera(Camera camera, GameObject subject)
        {
            var renderers = subject.GetComponentsInChildren<SpriteRenderer>();
            if (renderers.Length == 0)
                throw new InvalidOperationException("Flash 2D preview subject has no sprite renderers.");
            var bounds = renderers[0].bounds;
            for (var index = 1; index < renderers.Length; index++)
                bounds.Encapsulate(renderers[index].bounds);
            camera.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);
            var verticalSize = bounds.extents.y * 1.22f;
            var horizontalSize = bounds.extents.x * 1.22f / camera.aspect;
            camera.orthographicSize = Mathf.Max(0.55f, verticalSize, horizontalSize);
        }

        private static void RenderPreview(Camera camera, string assetPath)
        {
            var renderTexture = new RenderTexture(900, 1100, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 4,
                filterMode = FilterMode.Bilinear,
            };
            var previous = RenderTexture.active;
            camera.targetTexture = renderTexture;
            camera.Render();
            camera.Render();
            RenderTexture.active = renderTexture;
            var output = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false, false);
            output.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, false);
            output.Apply(false, false);
            File.WriteAllBytes(ToAbsolutePath(assetPath), output.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(output);
            camera.targetTexture = null;
            RenderTexture.active = previous;
            renderTexture.Release();
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }

        private static void BuildFlashPrefabAndAnimations()
        {
            EnsureAssetDirectory(AnimationDirectory);
            EnsureAssetDirectory(Path.GetDirectoryName(PrefabPath)?.Replace('\\', '/'));
            EnsureAssetDirectory(Path.GetDirectoryName(DefinitionPath)?.Replace('\\', '/'));

            foreach (var path in new[] { ControllerPath, OverridePath, PrefabPath, DefinitionPath })
                if (AssetDatabase.LoadMainAssetAtPath(path) != null)
                    AssetDatabase.DeleteAsset(path);

            var clips = new Dictionary<ChickenVisualState, AnimationClip>();
            clips.Add(ChickenVisualState.Idle, CreateIdleClip());
            clips.Add(ChickenVisualState.Warmup, CreateWarmupClip());
            clips.Add(ChickenVisualState.Run, CreateRunClip(false));
            clips.Add(ChickenVisualState.Sprint, CreateRunClip(true));
            clips.Add(ChickenVisualState.Stop, CreateStopClip());
            clips.Add(ChickenVisualState.Fall, CreateFallClip(false));
            clips.Add(ChickenVisualState.Recover, CreateFallClip(true));
            clips.Add(ChickenVisualState.Turn, CreateTurnClip());
            clips.Add(ChickenVisualState.Interfere, CreateInterfereClip());
            clips.Add(ChickenVisualState.Celebrate, CreateCelebrateClip());
            clips.Add(ChickenVisualState.Lose, CreateLoseClip());

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var stateMachine = controller.layers[0].stateMachine;
            stateMachine.states = Array.Empty<ChildAnimatorState>();
            foreach (var pair in clips)
            {
                var state = stateMachine.AddState(StateName(pair.Key));
                state.motion = pair.Value;
                state.writeDefaultValues = true;
                if (pair.Key == ChickenVisualState.Idle)
                    stateMachine.defaultState = state;
            }

            var root = BuildFlashHierarchy(controller);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            if (prefab == null)
                throw new InvalidOperationException("Flash 2D prefab could not be saved.");

            var animatorOverride = new AnimatorOverrideController(controller) { name = "AOC_flash_2D_WIP" };
            AssetDatabase.CreateAsset(animatorOverride, OverridePath);
            var definition = ScriptableObject.CreateInstance<Chicken2DVisualDefinition>();
            definition.name = "CFG_Chicken2D_flash_WIP";
            AssetDatabase.CreateAsset(definition, DefinitionPath);
            var serialized = new SerializedObject(definition);
            serialized.FindProperty("_chickenId").stringValue = "chicken-flash";
            serialized.FindProperty("_prefab").objectReferenceValue = prefab;
            serialized.FindProperty("_animatorOverride").objectReferenceValue = animatorOverride;
            serialized.FindProperty("_portrait").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(MasterPath);
            serialized.FindProperty("_stageScale").floatValue = 1f;
            serialized.FindProperty("_footBaseline").floatValue = 0f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
        }

        private static GameObject BuildFlashHierarchy(RuntimeAnimatorController controller)
        {
            var root = new GameObject("PF_Chicken_flash_2D_WIP", typeof(Animator));
            var animator = root.GetComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            var artRoot = Child(root.transform, "ArtRoot", Vector3.zero);

            SpritePart(artRoot, "WingFar", "wing_far", new Vector3(-0.02f, 0.92f, 0), -30);
            SpritePart(artRoot, "UpperLegFar", "upper_leg_near", new Vector3(0.08f, 0.57f, 0), -25);
            SpritePart(artRoot, "LowerLegFar", "lower_leg_foot_near", new Vector3(0.08f, 0.29f, 0), -24);
            SpritePart(artRoot, "Tail", "tail", new Vector3(0.12f, 0.84f, 0), -20);
            SpritePart(artRoot, "UpperLegNear", "upper_leg_near", new Vector3(-0.08f, 0.57f, 0), -10);
            SpritePart(artRoot, "LowerLegNear", "lower_leg_foot_near", new Vector3(-0.08f, 0.28f, 0), -9);
            SpritePart(artRoot, "Torso", "torso", new Vector3(0, 0.76f, 0), 0);
            SpritePart(artRoot, "Head", "head_neck", new Vector3(-0.09f, 0.98f, 0), 10);
            SpritePart(artRoot, "Crest", "crest", new Vector3(-0.11f, 1.37f, 0), 11);
            SpritePart(artRoot, "WingNear", "wing_near", new Vector3(-0.02f, 0.92f, 0), 15);
            SpritePart(artRoot, "Ribbons", "ribbons", new Vector3(0.11f, 1.40f, 0), 19);
            SpritePart(artRoot, "Headband", "headband", new Vector3(-0.07f, 1.40f, 0), 20);
            return root;
        }

        private static Transform Child(Transform parent, string name, Vector3 position)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.localPosition = position;
            return child;
        }

        private static void SpritePart(Transform parent, string objectName, string assetName, Vector3 position, int order)
        {
            var path = $"{OutputDirectory}/CH_flash_{assetName}_v1.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                throw new InvalidOperationException($"Missing generated Flash sprite: {path}");
            var child = new GameObject(objectName, typeof(SpriteRenderer));
            child.transform.SetParent(parent, false);
            child.transform.localPosition = position;
            var renderer = child.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
        }

        private static AnimationClip CreateIdleClip()
        {
            var clip = Clip("AN_flash_Idle_2D_WIP", 2.4f, true);
            Curve(clip, "ArtRoot", "m_LocalPosition.y", 0, 0.018f, 0);
            Curve(clip, "ArtRoot/Head", "localEulerAnglesRaw.z", 0, 1.8f, 0);
            Curve(clip, "ArtRoot/Tail", "localEulerAnglesRaw.z", 0, -2.5f, 0);
            Curve(clip, "ArtRoot/Ribbons", "localEulerAnglesRaw.z", 0, 4f, 0);
            return SaveClip(clip);
        }

        private static AnimationClip CreateWarmupClip()
        {
            var clip = Clip("AN_flash_Warmup_2D_WIP", 1.6f, true);
            Curve(clip, "ArtRoot", "m_LocalPosition.y", 0, -0.05f, 0.02f, -0.05f, 0);
            Curve(clip, "ArtRoot", "localEulerAnglesRaw.z", 0, 4f, -2f, 4f, 0);
            Curve(clip, "ArtRoot/WingNear", "localEulerAnglesRaw.z", 0, -8f, 9f, -8f, 0);
            Curve(clip, "ArtRoot/Head", "localEulerAnglesRaw.z", 0, -4f, 5f, -4f, 0);
            return SaveClip(clip);
        }

        private static AnimationClip CreateRunClip(bool sprint)
        {
            var length = sprint ? 0.48f : 0.65f;
            var prefix = sprint ? "Sprint" : "Run";
            var clip = Clip($"AN_flash_{prefix}_2D_WIP", length, true);
            var bounce = sprint ? 0.055f : 0.038f;
            var lean = sprint ? 10f : 4f;
            Curve(clip, "ArtRoot", "m_LocalPosition.y", 0, bounce, 0, bounce, 0);
            Curve(clip, "ArtRoot", "localEulerAnglesRaw.z", lean, lean + 2f, lean, lean - 2f, lean);
            Curve(clip, "ArtRoot/UpperLegFar", "localEulerAnglesRaw.z", -24f, 18f, 24f, -18f, -24f);
            Curve(clip, "ArtRoot/LowerLegFar", "localEulerAnglesRaw.z", 18f, -24f, -18f, 24f, 18f);
            Curve(clip, "ArtRoot/UpperLegNear", "localEulerAnglesRaw.z", 24f, -18f, -24f, 18f, 24f);
            Curve(clip, "ArtRoot/LowerLegNear", "localEulerAnglesRaw.z", -18f, 24f, 18f, -24f, -18f);
            Curve(clip, "ArtRoot/WingNear", "localEulerAnglesRaw.z", 3f, -7f, 3f, 9f, 3f);
            Curve(clip, "ArtRoot/Tail", "localEulerAnglesRaw.z", -5f, 4f, -5f, 3f, -5f);
            Curve(clip, "ArtRoot/Ribbons", "localEulerAnglesRaw.z", -8f, 8f, -8f, 8f, -8f);
            return SaveClip(clip);
        }

        private static AnimationClip CreateStopClip()
        {
            var clip = Clip("AN_flash_Stop_2D_WIP", 0.7f, false);
            Curve(clip, "ArtRoot", "localEulerAnglesRaw.z", 8f, -7f, 0);
            Curve(clip, "ArtRoot", "m_LocalPosition.y", 0, -0.06f, 0);
            Curve(clip, "ArtRoot/WingNear", "localEulerAnglesRaw.z", 0, 15f, 0);
            return SaveClip(clip);
        }

        private static AnimationClip CreateFallClip(bool recover)
        {
            var name = recover ? "Recover" : "Fall";
            var clip = Clip($"AN_flash_{name}_2D_WIP", recover ? 0.75f : 0.7f, false);
            if (recover)
            {
                Curve(clip, "ArtRoot", "localEulerAnglesRaw.z", -78f, -35f, 0);
                Curve(clip, "ArtRoot", "m_LocalPosition.y", -0.42f, -0.16f, 0);
            }
            else
            {
                Curve(clip, "ArtRoot", "localEulerAnglesRaw.z", 8f, -25f, -78f);
                Curve(clip, "ArtRoot", "m_LocalPosition.y", 0, 0.06f, -0.42f);
            }
            Curve(clip, "ArtRoot/WingNear", "localEulerAnglesRaw.z", 0, 28f, -12f);
            Curve(clip, "ArtRoot/Ribbons", "localEulerAnglesRaw.z", 0, -22f, 12f);
            return SaveClip(clip);
        }

        private static AnimationClip CreateTurnClip()
        {
            var clip = Clip("AN_flash_Turn_2D_WIP", 0.65f, false);
            Curve(clip, "ArtRoot", "m_LocalScale.x", 1f, 0.08f, -1f, 0.08f, 1f);
            Curve(clip, "ArtRoot", "m_LocalPosition.y", 0, 0.05f, 0);
            return SaveClip(clip);
        }

        private static AnimationClip CreateInterfereClip()
        {
            var clip = Clip("AN_flash_Interfere_2D_WIP", 0.7f, false);
            Curve(clip, "ArtRoot/WingNear", "localEulerAnglesRaw.z", 0, 38f, 0);
            Curve(clip, "ArtRoot/UpperLegNear", "localEulerAnglesRaw.z", 0, -42f, 0);
            Curve(clip, "ArtRoot", "localEulerAnglesRaw.z", 0, -7f, 0);
            return SaveClip(clip);
        }

        private static AnimationClip CreateCelebrateClip()
        {
            var clip = Clip("AN_flash_Celebrate_2D_WIP", 1.15f, true);
            Curve(clip, "ArtRoot", "m_LocalPosition.y", 0, 0.16f, 0, 0.09f, 0);
            Curve(clip, "ArtRoot/WingNear", "localEulerAnglesRaw.z", 0, 34f, -8f, 38f, 0);
            Curve(clip, "ArtRoot/Head", "localEulerAnglesRaw.z", 0, -8f, 6f, -5f, 0);
            Curve(clip, "ArtRoot/Ribbons", "localEulerAnglesRaw.z", 0, 18f, -12f, 15f, 0);
            return SaveClip(clip);
        }

        private static AnimationClip CreateLoseClip()
        {
            var clip = Clip("AN_flash_Lose_2D_WIP", 1.8f, true);
            Curve(clip, "ArtRoot", "m_LocalPosition.y", 0, -0.07f, -0.05f, -0.07f, 0);
            Curve(clip, "ArtRoot/Head", "localEulerAnglesRaw.z", 0, 14f, 18f, 14f, 0);
            Curve(clip, "ArtRoot/WingNear", "localEulerAnglesRaw.z", 0, -12f, -16f, -12f, 0);
            Curve(clip, "ArtRoot/Tail", "localEulerAnglesRaw.z", 0, 9f, 12f, 9f, 0);
            return SaveClip(clip);
        }

        private static AnimationClip Clip(string name, float length, bool loop)
        {
            var clip = new AnimationClip { name = name, frameRate = 30f };
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

        private static void Curve(AnimationClip clip, string path, string property, params float[] values)
        {
            if (values.Length < 2)
                throw new ArgumentException("At least two curve values are required.", nameof(values));
            var length = AnimationUtility.GetAnimationClipSettings(clip).stopTime;
            var keys = new Keyframe[values.Length];
            for (var index = 0; index < values.Length; index++)
                keys[index] = new Keyframe(length * index / (values.Length - 1), values[index]);
            var curve = new AnimationCurve(keys);
            for (var index = 0; index < curve.length; index++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, index, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(curve, index, AnimationUtility.TangentMode.ClampedAuto);
            }
            clip.SetCurve(path, typeof(Transform), property, curve);
        }

        private static string StateName(ChickenVisualState state)
        {
            return state switch
            {
                ChickenVisualState.Stop => "Stop",
                ChickenVisualState.Fall => "Fall",
                ChickenVisualState.Recover => "Recover",
                ChickenVisualState.Turn => "Turn",
                ChickenVisualState.Interfere => "Interfere",
                ChickenVisualState.Celebrate => "Celebrate",
                ChickenVisualState.Lose => "Lose",
                ChickenVisualState.Warmup => "Warmup",
                ChickenVisualState.Run => "Run",
                ChickenVisualState.Sprint => "Sprint",
                _ => "Idle",
            };
        }

        private static void EnsureAssetDirectory(string assetDirectory)
        {
            if (string.IsNullOrEmpty(assetDirectory) || AssetDatabase.IsValidFolder(assetDirectory))
                return;
            var parent = Path.GetDirectoryName(assetDirectory)?.Replace('\\', '/');
            EnsureAssetDirectory(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(assetDirectory));
        }

        private static PartReport ExtractPart(Texture2D source, int index, PartSpec spec)
        {
            var row = index / GridSize;
            var column = index % GridSize;
            var xMin = Mathf.FloorToInt(column * source.width / (float)GridSize);
            var xMax = Mathf.FloorToInt((column + 1) * source.width / (float)GridSize);
            var yMin = source.height - Mathf.FloorToInt((row + 1) * source.height / (float)GridSize);
            var yMax = source.height - Mathf.FloorToInt(row * source.height / (float)GridSize);
            var width = xMax - xMin;
            var height = yMax - yMin;
            var sourcePixels = source.GetPixels32();
            var pixels = new Color32[width * height];
            for (var y = 0; y < height; y++)
                Array.Copy(sourcePixels, (yMin + y) * source.width + xMin, pixels, y * width, width);

            var components = FindComponents(pixels, width, height);
            if (components.Count < spec.ComponentCount)
                throw new InvalidOperationException($"{spec.Name}: expected {spec.ComponentCount} components but found {components.Count}.");
            components.Sort((left, right) => right.Indices.Count.CompareTo(left.Indices.Count));
            var keep = new bool[pixels.Length];
            for (var componentIndex = 0; componentIndex < spec.ComponentCount; componentIndex++)
                foreach (var pixelIndex in components[componentIndex].Indices)
                    keep[pixelIndex] = true;

            var removedPixels = 0;
            var minX = width;
            var minY = height;
            var maxX = -1;
            var maxY = -1;
            for (var pixelIndex = 0; pixelIndex < pixels.Length; pixelIndex++)
            {
                if (!keep[pixelIndex])
                {
                    if (pixels[pixelIndex].a > 0)
                        removedPixels++;
                    pixels[pixelIndex] = new Color32(0, 0, 0, 0);
                    continue;
                }
                var x = pixelIndex % width;
                var y = pixelIndex / width;
                minX = Mathf.Min(minX, x);
                minY = Mathf.Min(minY, y);
                maxX = Mathf.Max(maxX, x);
                maxY = Mathf.Max(maxY, y);
            }
            if (maxX < minX || maxY < minY)
                throw new InvalidOperationException($"{spec.Name}: no opaque sprite pixels remained after cleanup.");

            minX = Mathf.Max(0, minX - Padding);
            minY = Mathf.Max(0, minY - Padding);
            maxX = Mathf.Min(width - 1, maxX + Padding);
            maxY = Mathf.Min(height - 1, maxY + Padding);
            var outputWidth = maxX - minX + 1;
            var outputHeight = maxY - minY + 1;
            var outputPixels = new Color32[outputWidth * outputHeight];
            for (var y = 0; y < outputHeight; y++)
                Array.Copy(pixels, (minY + y) * width + minX, outputPixels, y * outputWidth, outputWidth);

            var output = new Texture2D(outputWidth, outputHeight, TextureFormat.RGBA32, false, false);
            output.SetPixels32(outputPixels);
            output.Apply(false, false);
            var assetPath = $"{OutputDirectory}/CH_flash_{spec.Name}_v1.png";
            File.WriteAllBytes(ToAbsolutePath(assetPath), output.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(output);
            return new PartReport
            {
                Name = spec.Name,
                AssetPath = assetPath,
                Width = outputWidth,
                Height = outputHeight,
                ComponentsKept = spec.ComponentCount,
                ComponentsFound = components.Count,
                RemovedNonTransparentPixels = removedPixels,
            };
        }

        private static List<Component> FindComponents(Color32[] pixels, int width, int height)
        {
            var result = new List<Component>();
            var visited = new bool[pixels.Length];
            var queue = new Queue<int>();
            for (var start = 0; start < pixels.Length; start++)
            {
                if (visited[start] || pixels[start].a < AlphaThreshold)
                    continue;
                var component = new Component();
                visited[start] = true;
                queue.Enqueue(start);
                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    component.Indices.Add(current);
                    var x = current % width;
                    var y = current / width;
                    Visit(x - 1, y);
                    Visit(x + 1, y);
                    Visit(x, y - 1);
                    Visit(x, y + 1);

                    void Visit(int nextX, int nextY)
                    {
                        if (nextX < 0 || nextX >= width || nextY < 0 || nextY >= height)
                            return;
                        var next = nextY * width + nextX;
                        if (visited[next] || pixels[next].a < AlphaThreshold)
                            return;
                        visited[next] = true;
                        queue.Enqueue(next);
                    }
                }
                result.Add(component);
            }
            return result.Where(component => component.Indices.Count >= 6).ToList();
        }

        private static void ConfigureSpriteImporter(string assetPath)
        {
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                throw new InvalidOperationException($"Sprite importer missing for {assetPath}.");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 2048;
            importer.spritePixelsPerUnit = assetPath.Contains("lower_leg_foot_near", StringComparison.Ordinal) ? 3000f : 600f;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = PivotFor(assetPath);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static Vector2 PivotFor(string assetPath)
        {
            if (assetPath.Contains("upper_leg_near", StringComparison.Ordinal))
                return new Vector2(0.5f, 0.88f);
            if (assetPath.Contains("lower_leg_foot_near", StringComparison.Ordinal))
                return new Vector2(0.5f, 0.92f);
            if (assetPath.Contains("leg_far", StringComparison.Ordinal))
                return new Vector2(0.5f, 0.9f);
            if (assetPath.Contains("wing_", StringComparison.Ordinal))
                return new Vector2(0.5f, 0.78f);
            if (assetPath.Contains("head_neck", StringComparison.Ordinal))
                return new Vector2(0.5f, 0.1f);
            if (assetPath.Contains("crest", StringComparison.Ordinal))
                return new Vector2(0.5f, 0.12f);
            if (assetPath.Contains("tail", StringComparison.Ordinal))
                return new Vector2(0.12f, 0.5f);
            if (assetPath.Contains("ribbons", StringComparison.Ordinal))
                return new Vector2(0.1f, 0.5f);
            return new Vector2(0.5f, 0.5f);
        }

        private static void InstallReferencePart(string referencePath, string partName, List<PartReport> reports)
        {
            var absoluteReference = ToAbsolutePath(referencePath);
            if (!File.Exists(absoluteReference))
                throw new FileNotFoundException($"Reference art for {partName} is missing.", absoluteReference);
            var assetPath = $"{OutputDirectory}/CH_flash_{partName}_v1.png";
            File.Copy(absoluteReference, ToAbsolutePath(assetPath), true);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
            if (!texture.LoadImage(File.ReadAllBytes(absoluteReference), false))
                throw new InvalidOperationException($"Reference art for {partName} could not be decoded.");
            var report = reports.First(item => item.Name == partName);
            report.Width = texture.width;
            report.Height = texture.height;
            report.ComponentsKept = 1;
            report.ComponentsFound = 1;
            report.RemovedNonTransparentPixels = 0;
            UnityEngine.Object.DestroyImmediate(texture);
        }

        private static string ToAbsolutePath(string assetPath)
        {
            if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal))
                throw new ArgumentException("Expected an Assets-relative path.", nameof(assetPath));
            return Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length));
        }

        private readonly struct PartSpec
        {
            public PartSpec(string name, int componentCount)
            {
                Name = name;
                ComponentCount = componentCount;
            }

            public string Name { get; }
            public int ComponentCount { get; }
        }

        private sealed class Component
        {
            public readonly List<int> Indices = new();
        }

        [Serializable]
        private sealed class PipelineReport
        {
            public string Source;
            public int AlphaThreshold;
            public PartReport[] Parts;
        }

        [Serializable]
        private sealed class PartReport
        {
            public string Name;
            public string AssetPath;
            public int Width;
            public int Height;
            public int ComponentsKept;
            public int ComponentsFound;
            public int RemovedNonTransparentPixels;
        }
    }
}
