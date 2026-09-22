using System;
using System.Collections.Generic;
using System.IO;
using GroundChickenKing.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GroundChickenKing.Editor
{
    public static class Phase11ALineupBuilder
    {
        public const string ScenePath = "Assets/_Game/Scenes/SCN_Phase11A_2DLineup.unity";
        public const string PrefabPath = "Assets/_Game/Prefabs/Race/PF_Race_Phase11A_2DLineup.prefab";
        public const string PreviewPath = "Assets/_Game/Art/Characters/Production/2D/Phase11A/Preview/PHASE11A_2D_Lineup_1920x960.png";

        private const string LaneSpritePath = "Assets/_Game/Art/Characters/Production/2D/Phase11A/T_LineupPixel.png";
        private const string CharacterMaterialDirectory = "Assets/_Game/Art/Materials";
        private const float CameraSize = 4.8f;
        private const float LaneSpacing = 1.72f;
        private const float LaneBaseline = -0.56f;

        private static readonly string[] CharacterKeys =
        {
            "flash", "chubby", "slacker", "flash", "chubby",
        };

        private static readonly float[] CharacterX =
        {
            -6.2f, -3.1f, 0f, 3.1f, 6.2f,
        };

        [MenuItem("Ground Chicken King/Phase 11/Build Phase 11A Five Chicken Lineup")]
        public static void BuildLineup()
        {
            EnsureLaneSprite();
            var characterMaterials = EnsureCharacterMaterials();
            ApplyCharacterMaterials(characterMaterials);
            BuildLineupPrefab();
            BuildLineupScene();
            RenderLineupPreview();
            AssetDatabase.SaveAssets();
            Debug.Log("PHASE11A_LINEUP_BUILD_PASS lanes=5 resolution=1920x960");
        }

        private static void BuildLineupPrefab()
        {
            EnsureAssetDirectory(Path.GetDirectoryName(PrefabPath)?.Replace('\\', '/'));
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
                AssetDatabase.DeleteAsset(PrefabPath);

            var laneSprite = AssetDatabase.LoadAssetAtPath<Sprite>(LaneSpritePath);
            if (laneSprite == null)
                throw new InvalidOperationException("Phase 11A lane sprite is missing.");
            var root = new GameObject("PF_Race_Phase11A_2DLineup");
            for (var laneIndex = 0; laneIndex < CharacterKeys.Length; laneIndex++)
            {
                var lane = new GameObject($"Lane_{laneIndex + 1}").transform;
                lane.SetParent(root.transform, false);
                lane.localPosition = new Vector3(0f, 3.44f - laneIndex * LaneSpacing, 0f);

                CreateLaneBar(lane, laneSprite, "Track", Vector3.zero, new Vector2(19.2f, 1.54f),
                    laneIndex % 2 == 0
                        ? new Color(0.11f, 0.22f, 0.18f, 1f)
                        : new Color(0.15f, 0.28f, 0.22f, 1f), 0);
                CreateLaneBar(lane, laneSprite, "Baseline", new Vector3(0f, LaneBaseline, 0f),
                    new Vector2(18.6f, 0.025f), new Color(0.69f, 0.78f, 0.60f, 0.42f), 2);
                CreateLaneBar(lane, laneSprite, "Finish", new Vector3(8.35f, 0f, 0f),
                    new Vector2(0.08f, 1.40f), new Color(0.92f, 0.86f, 0.63f, 0.82f), 3);

                var key = CharacterKeys[laneIndex];
                var characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    $"Assets/_Game/Prefabs/Chickens/2D/PF_Chicken_{key}_2D_Frames.prefab");
                if (characterPrefab == null)
                    throw new InvalidOperationException($"Missing Phase 11A character prefab: {key}");
                var character = PrefabUtility.InstantiatePrefab(characterPrefab, lane) as GameObject;
                if (character == null)
                    throw new InvalidOperationException($"Could not instantiate Phase 11A character: {key}");
                character.name = $"Chicken_{laneIndex + 1}_{key}";
                character.transform.localRotation = Quaternion.identity;
                character.transform.localScale = Vector3.one;
                var renderer = character.GetComponentInChildren<SpriteRenderer>(true);
                if (renderer == null || renderer.sprite == null)
                    throw new InvalidOperationException($"Character {key} has no complete-character SpriteRenderer.");
                var characterMaterial = AssetDatabase.LoadAssetAtPath<Material>(CharacterMaterialPath(key));
                if (characterMaterial == null)
                    throw new InvalidOperationException($"Character {key} has no unlit material.");
                renderer.sharedMaterial = characterMaterial;
                character.transform.localPosition = new Vector3(
                    CharacterX[laneIndex],
                    LaneBaseline - renderer.localBounds.min.y,
                    -0.1f);
                renderer.sortingOrder = 20 - laneIndex;
                var animator = character.GetComponent<Animator>();
                if (animator == null)
                    throw new InvalidOperationException($"Character {key} has no Animator.");
                animator.applyRootMotion = false;
            }

            var saved = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            if (saved == null)
                throw new InvalidOperationException("Phase 11A lineup prefab could not be saved.");
        }

        private static void BuildLineupScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "SCN_Phase11A_2DLineup";
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AspectRatioController));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = CameraSize;
            camera.aspect = 2f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.05f, 0.06f, 1f);
            camera.allowHDR = false;
            camera.allowMSAA = false;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var lineup = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (lineup == null)
                throw new InvalidOperationException("Phase 11A lineup could not be instantiated into its scene.");
            lineup.name = "Phase11A_2DLineup";
            EditorSceneManager.MarkSceneDirty(scene);
            EnsureAssetDirectory(Path.GetDirectoryName(ScenePath)?.Replace('\\', '/'));
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Phase 11A lineup scene could not be saved.");
        }

        private static void RenderLineupPreview()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (camera == null)
                throw new InvalidOperationException("Phase 11A lineup scene has no camera.");
            const int width = 1920;
            const int height = 960;
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 1,
            };
            renderTexture.Create();
            var previous = RenderTexture.active;
            camera.targetTexture = renderTexture;
            camera.Render();
            RenderTexture.active = renderTexture;
            var output = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
            output.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, false);
            output.Apply(false, false);
            EnsureAssetDirectory(Path.GetDirectoryName(PreviewPath)?.Replace('\\', '/'));
            File.WriteAllBytes(ToAbsolutePath(PreviewPath), output.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = previous;
            renderTexture.Release();
            UnityEngine.Object.DestroyImmediate(output);
            UnityEngine.Object.DestroyImmediate(renderTexture);
            AssetDatabase.ImportAsset(PreviewPath, ImportAssetOptions.ForceSynchronousImport);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void EnsureLaneSprite()
        {
            EnsureAssetDirectory(Path.GetDirectoryName(LaneSpritePath)?.Replace('\\', '/'));
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false, false);
            var pixels = new Color32[16];
            for (var index = 0; index < pixels.Length; index++)
                pixels[index] = new Color32(255, 255, 255, 255);
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            File.WriteAllBytes(ToAbsolutePath(LaneSpritePath), texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(LaneSpritePath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(LaneSpritePath) as TextureImporter;
            if (importer == null)
                throw new InvalidOperationException("Phase 11A lane TextureImporter is missing.");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 4f;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static Dictionary<string, Material> EnsureCharacterMaterials()
        {
            EnsureAssetDirectory(CharacterMaterialDirectory);
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
                         ?? Shader.Find("Sprites/Default");
            if (shader == null)
                throw new InvalidOperationException("No unlit Sprite shader is available for Phase 11A.");
            var materials = new Dictionary<string, Material>();
            foreach (var key in new[] { "flash", "chubby", "slacker" })
            {
                var path = CharacterMaterialPath(key);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(shader) { name = $"MAT_Chicken2D_{key}_Unlit" };
                    AssetDatabase.CreateAsset(material, path);
                }
                else if (material.shader != shader)
                {
                    material.shader = shader;
                    EditorUtility.SetDirty(material);
                }
                materials.Add(key, material);
            }
            const string obsoletePath = CharacterMaterialDirectory + "/MAT_Chicken2D_Unlit.mat";
            if (AssetDatabase.LoadMainAssetAtPath(obsoletePath) != null)
                AssetDatabase.DeleteAsset(obsoletePath);
            return materials;
        }

        private static void ApplyCharacterMaterials(IReadOnlyDictionary<string, Material> materials)
        {
            foreach (var key in new[] { "flash", "chubby", "slacker" })
            {
                var path = $"Assets/_Game/Prefabs/Chickens/2D/PF_Chicken_{key}_2D_Frames.prefab";
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
                    if (renderers.Length != 1)
                        throw new InvalidOperationException($"Character {key} must have exactly one SpriteRenderer.");
                    renderers[0].sharedMaterial = materials[key];
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        private static string CharacterMaterialPath(string key)
        {
            return $"{CharacterMaterialDirectory}/MAT_Chicken2D_{key}_Unlit.mat";
        }

        private static void CreateLaneBar(
            Transform parent,
            Sprite sprite,
            string name,
            Vector3 position,
            Vector2 size,
            Color color,
            int sortingOrder)
        {
            var bar = new GameObject(name, typeof(SpriteRenderer));
            bar.transform.SetParent(parent, false);
            bar.transform.localPosition = position;
            var renderer = bar.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = size;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
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
