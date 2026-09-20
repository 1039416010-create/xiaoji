using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace GroundChickenKing.Editor
{
    /// <summary>Creates isolated WIP review prefabs; does not modify the playable scene.</summary>
    public static class Phase11BenchmarkImporter
    {
        private const string Root = "Assets/_Game/Art/Characters/Production/VisualV3";
        private static readonly string[] Characters = { "flash", "chubby", "slacker" };
        private static readonly string[] Actions = {
            "Idle", "Warmup", "Run", "Sprint", "Stop", "Fall", "Recover", "Turn", "Interfere", "Celebrate", "Lose"
        };

        [MenuItem("Ground Chicken King/Phase 11/Import and Validate WIP Benchmarks")]
        public static void ImportAndValidate()
        {
            EnsureFolder(Root + "/Unity");
            var reports = new List<CharacterReport>();
            foreach (var id in Characters) reports.Add(ImportCharacter(id));
            AssetDatabase.SaveAssets();
            var report = new Report { unityVersion = Application.unityVersion, characters = reports.ToArray() };
            File.WriteAllText(Root + "/unity_validation_report.json", JsonUtility.ToJson(report, true));
            AssetDatabase.ImportAsset(Root + "/unity_validation_report.json");
            Debug.Log("PHASE11_V3_UNITY_IMPORT_PASS " + JsonUtility.ToJson(report));
        }

        private static CharacterReport ImportCharacter(string id)
        {
            var path = Root + "/FBX/CH_" + id + "_visual_wip_v3.fbx";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            Require(importer != null, "Model importer missing: " + id);
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.motionNodeName = "Root";
            importer.importAnimation = true;
            importer.importBlendShapes = true;
            importer.optimizeGameObjects = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.animationCompression = ModelImporterAnimationCompression.Off;
            importer.isReadable = true;
            var clips = importer.defaultClipAnimations;
            Require(clips.Length == 11, id + ": expected 11 FBX takes, got " + clips.Length);
            foreach (var clip in clips)
            {
                clip.name = clip.takeName.Split('|').Last();
                clip.lockRootRotation = true;
                clip.lockRootHeightY = true;
                clip.lockRootPositionXZ = true;
                clip.keepOriginalOrientation = true;
                clip.keepOriginalPositionY = true;
                clip.keepOriginalPositionXZ = true;
                clip.loopTime = clip.name == "Idle_Blocking" || clip.name == "Run_Blocking" || clip.name == "Sprint_Blocking";
            }
            importer.clipAnimations = clips;
            importer.SaveAndReimport();

            var baseColor = ImportTexture(id, "BaseColor", true, false);
            var normal = ImportTexture(id, "Normal", false, true);
            var mask = ImportTexture(id, "Mask", false, false);
            var materialPath = Root + "/Unity/MAT_" + id + "_WIP.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                Require(shader != null, "URP Lit shader missing");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }
            material.SetTexture("_BaseMap", baseColor);
            material.SetColor("_BaseColor", Color.white);
            material.SetTexture("_BumpMap", normal);
            material.SetFloat("_BumpScale", 1);
            material.SetTexture("_MetallicGlossMap", mask);
            material.SetFloat("_Smoothness", 1);
            material.EnableKeyword("_NORMALMAP");
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            EditorUtility.SetDirty(material);

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Require(model != null, "Imported model missing: " + id);
            var instance = UnityEngine.Object.Instantiate(model);
            try
            {
                instance.name = "PF_Chicken_" + id + "_WIP";
                var animator = instance.GetComponent<Animator>();
                Require(animator != null && animator.avatar != null && animator.avatar.isValid, id + ": invalid Generic Avatar");
                animator.applyRootMotion = false;
                var renderers = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                Require(renderers.Length == 6, id + ": missing body/accessory LOD mesh");
                foreach (var renderer in renderers)
                {
                    Require(renderer.sharedMesh != null && renderer.bones.All(bone => bone != null), id + ": missing mesh or bone reference");
                    renderer.sharedMaterials = new[] { material };
                    renderer.gameObject.SetActive(true);
                    renderer.enabled = true;
                    renderer.updateWhenOffscreen = true;
                }
                var body = renderers.Single(renderer => renderer.name.EndsWith("Body_LOD0", StringComparison.Ordinal));
                Require(body.sharedMesh.blendShapeCount == 9, id + ": morph target count differs from source");
                var lods = new LOD[3];
                var triangles = new int[3];
                for (var level = 0; level < 3; level++)
                {
                    var group = renderers.Where(renderer => renderer.name.EndsWith("LOD" + level, StringComparison.Ordinal)).ToArray();
                    Require(group.Length == 2, id + ": LOD renderer count");
                    triangles[level] = group.Sum(renderer => (int)renderer.sharedMesh.GetIndexCount(0) / 3);
                    lods[level] = new LOD(level == 0 ? .5f : level == 1 ? .22f : .05f, group);
                }
                var lodGroup = instance.GetComponent<LODGroup>() ?? instance.AddComponent<LODGroup>();
                lodGroup.SetLODs(lods);
                lodGroup.RecalculateBounds();
                var animations = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
                    .Where(clip => !clip.name.StartsWith("__preview__", StringComparison.Ordinal)).ToArray();
                Require(animations.Length == 11, id + ": imported animation count");
                foreach (var action in Actions)
                    Require(animations.Any(clip => clip.name == action + "_Blocking"), id + ": missing " + action);

                var controllerPath = Root + "/Unity/AC_" + id + "_WIP.controller";
                var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
                if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                var stateMachine = controller.layers[0].stateMachine;
                foreach (var action in Actions)
                {
                    var state = stateMachine.states.FirstOrDefault(entry => entry.state.name == action).state;
                    if (state == null) state = stateMachine.AddState(action);
                    state.motion = animations.Single(clip => clip.name == action + "_Blocking");
                    if (action == "Idle") stateMachine.defaultState = state;
                }
                animator.runtimeAnimatorController = controller;
                EditorUtility.SetDirty(controller);
                PrefabUtility.SaveAsPrefabAsset(instance, Root + "/Unity/PF_Chicken_" + id + "_WIP.prefab");
                return new CharacterReport { id = id, clips = animations.Length, shapes = body.sharedMesh.blendShapeCount, lodTriangles = triangles, validGenericAvatar = true, applyRootMotion = animator.applyRootMotion };
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }

        private static Texture2D ImportTexture(string id, string suffix, bool srgb, bool normal)
        {
            var path = Root + "/Textures/T_" + id + "_" + suffix + ".png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = srgb;
            importer.alphaIsTransparency = false;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        [Serializable] private sealed class Report
        {
            public string unityVersion;
            public CharacterReport[] characters;
            public string scope = "Isolated WIP prefab import; no gameplay, projection, or final animation acceptance";
        }

        [Serializable] private sealed class CharacterReport
        {
            public string id;
            public int clips;
            public int shapes;
            public int[] lodTriangles;
            public bool validGenericAvatar;
            public bool applyRootMotion;
        }
    }
}
