using System;
using System.Collections.Generic;
using GroundChickenKing.Chickens;
using GroundChickenKing.Diagnostics;
using GroundChickenKing.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GroundChickenKing.Editor
{
    public static class Phase05SceneBuilder
    {
        private const string ScenePath = "Assets/_Game/Scenes/SCN_Main.unity";
        private const string ControllerPath = "Assets/_Game/Animations/Chickens/AC_Chicken_Presentation.controller";
        private static readonly string[] Ids = { "chicken-flash", "chicken-chubby", "chicken-tiny", "chicken-bro", "chicken-slacker" };
        private static readonly string[] Names = { "闪电鸡", "胖墩", "豆丁", "鸡哥", "摸鱼鸡" };
        private static readonly string[] Traits = { "爆发型", "沉稳型", "灵巧型", "好斗型", "随性型" };
        private static readonly Color[] Colors =
        {
            new(1f, 0.82f, 0.16f), new(0.96f, 0.48f, 0.18f), new(0.45f, 0.86f, 0.95f), new(0.86f, 0.28f, 0.24f), new(0.58f, 0.82f, 0.34f),
        };
        private static readonly Vector2[] Scales = { new(0.88f, 1.12f), new(1.22f, 0.92f), new(0.72f, 0.78f), new(1.08f, 1.08f), new(1f, 0.96f) };
        private static readonly string[] States = { "Idle", "Warmup", "Run", "Sprint", "Stop", "Fall", "Recover", "Turn", "Interfere", "Celebrate", "Lose" };

        [MenuItem("Ground Chicken King/Configure Phase 05")]
        public static void ConfigurePhase05()
        {
            EnsureFolder("Assets/_Game/Animations"); EnsureFolder("Assets/_Game/Animations/Chickens");
            EnsureFolder("Assets/_Game/Prefabs/Chickens");
            var controller = CreateAnimatorController();
            var prefabs = new GameObject[5];
            for (var i = 0; i < 5; i++) { prefabs[i] = CreateChickenPrefab(i, controller); CreateDefinition(i, prefabs[i]); }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var countdown = FindIncludingInactive("UI_Panel_RaceCountdown")?.GetComponent<RectTransform>();
            var presenter = GameObject.Find("GameSession")?.GetComponent<GameSessionPresenter>();
            if (countdown == null || presenter == null) throw new InvalidOperationException("Phase 04 scene is incomplete.");
            DestroyIfPresent("UI_Panel_RaceTrack");

            Move("UI_Text_RaceCountdownTitle", new Vector2(0f, 390f), new Vector2(1000f, 90f));
            Move("UI_Text_RaceCountdownSummary", new Vector2(0f, 310f), new Vector2(1000f, 80f));
            Move("UI_Button_RaceCountdownMainMenu", new Vector2(0f, -405f), new Vector2(340f, 75f));
            var track = CreateImage("UI_Panel_RaceTrack", countdown, new Color(0.05f, 0.10f, 0.07f, 0.98f), Vector2.zero, new Vector2(1700f, 520f));
            var trackRect = track.GetComponent<RectTransform>();
            var controllers = new ChickenController[5];
            for (var lane = 0; lane < 5; lane++)
            {
                var laneY = 205f - lane * 102.5f;
                CreateImage($"UI_Image_Lane_{lane + 1}", track.transform, lane % 2 == 0 ? new Color(0.18f, 0.31f, 0.20f, 1f) : new Color(0.13f, 0.25f, 0.16f, 1f), new Vector2(0f, laneY), new Vector2(1640f, 92f));
                CreateText($"UI_Text_Lane_{lane + 1}", track.transform, (lane + 1).ToString(), 28, new Vector2(-795f, laneY), new Vector2(42f, 42f));
                CreateImage($"UI_Image_Finish_{lane + 1}", track.transform, new Color(0.95f, 0.95f, 0.92f, 1f), new Vector2(720f, laneY), new Vector2(12f, 92f));
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[lane], track.transform);
                instance.name = $"Chicken_{lane + 1}_{Ids[lane]}";
                var root = instance.GetComponent<RectTransform>();
                var visual = instance.transform.Find("Visual");
                var animator = visual.GetComponent<Animator>();
                controllers[lane] = instance.GetComponent<ChickenController>();
                controllers[lane].Configure(Ids[lane], lane, root, visual, animator, -700f, 720f, laneY);
            }
            var racePresenter = track.AddComponent<ChickenRacePresenter>();
            racePresenter.Configure(controllers);
            presenter.ConfigurePresentation(racePresenter);
            if (presenter.GetComponent<DevelopmentPerformanceProbe>() == null)
                presenter.gameObject.AddComponent<DevelopmentPerformanceProbe>();
            EditorUtility.SetDirty(presenter); EditorUtility.SetDirty(racePresenter);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene, ScenePath); AssetDatabase.SaveAssets();
            Debug.Log("[Bootstrap] Phase 05 chicken presentation configured with five stable instances and fallback-safe animation states.");
        }

        public static void ConfigurePhase05FromCommandLine() => ConfigurePhase05();

        private static AnimatorController CreateAnimatorController()
        {
            AssetDatabase.DeleteAsset(ControllerPath);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var machine = controller.layers[0].stateMachine;
            foreach (var stateName in States)
            {
                var clipPath = $"Assets/_Game/Animations/Chickens/AN_Chicken_{stateName}.anim";
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (clip == null) { clip = new AnimationClip { name = $"AN_Chicken_{stateName}" }; AssetDatabase.CreateAsset(clip, clipPath); }
                clip.wrapMode = WrapMode.Loop;
                var state = machine.AddState(stateName); state.motion = clip;
                if (stateName == "Idle") machine.defaultState = state;
            }
            EditorUtility.SetDirty(controller); return controller;
        }

        private static GameObject CreateChickenPrefab(int index, RuntimeAnimatorController controller)
        {
            var root = new GameObject($"PF_Chicken_{Names[index]}", typeof(RectTransform), typeof(ChickenController));
            var rootRect = root.GetComponent<RectTransform>(); rootRect.sizeDelta = new Vector2(118f, 78f);
            var visual = new GameObject("Visual", typeof(RectTransform), typeof(Animator)); visual.transform.SetParent(root.transform, false);
            var visualRect = visual.GetComponent<RectTransform>(); visualRect.sizeDelta = new Vector2(118f, 78f); visualRect.localScale = new Vector3(Scales[index].x, Scales[index].y, 1f);
            var animator = visual.GetComponent<Animator>(); animator.runtimeAnimatorController = controller; animator.applyRootMotion = false;
            CreateImage("Body", visual.transform, Colors[index], new Vector2(-5f, -4f), new Vector2(72f, 54f));
            CreateImage("Head", visual.transform, Color.Lerp(Colors[index], Color.white, 0.15f), new Vector2(34f, 13f), new Vector2(42f, 42f));
            CreateImage("Beak", visual.transform, new Color(1f, 0.48f, 0.08f), new Vector2(60f, 8f), new Vector2(22f, 13f));
            CreateImage("Comb", visual.transform, new Color(0.9f, 0.08f, 0.08f), new Vector2(29f, 38f), new Vector2(24f, 12f));
            CreateImage("Eye", visual.transform, new Color(0.05f, 0.05f, 0.04f), new Vector2(42f, 19f), new Vector2(7f, 7f));
            CreateText("Name", visual.transform, Names[index], 16, new Vector2(0f, -41f), new Vector2(130f, 24f));
            var path = $"Assets/_Game/Prefabs/Chickens/PF_Chicken_{index + 1}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path); UnityEngine.Object.DestroyImmediate(root); return prefab;
        }

        private static void CreateDefinition(int index, GameObject prefab)
        {
            var path = $"Assets/_Game/Config/Chickens/CFG_Chicken_{index + 1}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<ChickenDefinition>(path);
            if (asset == null) { asset = ScriptableObject.CreateInstance<ChickenDefinition>(); AssetDatabase.CreateAsset(asset, path); }
            asset.ConfigureEditor(Ids[index], Names[index], Traits[index], prefab, Colors[index], Scales[index]); EditorUtility.SetDirty(asset);
        }

        private static GameObject CreateImage(string name, Transform parent, Color color, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); rect.anchoredPosition = position; rect.sizeDelta = size;
            var image = go.GetComponent<Image>(); image.color = color; image.raycastTarget = false; return go;
        }

        private static Text CreateText(string name, Transform parent, string value, int fontSize, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); rect.anchoredPosition = position; rect.sizeDelta = size;
            var text = go.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = fontSize; text.alignment = TextAnchor.MiddleCenter; text.color = Color.white; text.raycastTarget = false; return text;
        }

        private static void Move(string name, Vector2 position, Vector2 size)
        {
            var rect = FindIncludingInactive(name)?.GetComponent<RectTransform>(); if (rect == null) throw new InvalidOperationException($"Missing {name}."); rect.anchoredPosition = position; rect.sizeDelta = size;
        }
        private static void DestroyIfPresent(string name) { var go = FindIncludingInactive(name); if (go != null) UnityEngine.Object.DestroyImmediate(go); }
        private static GameObject FindIncludingInactive(string name)
        {
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects()) foreach (var item in root.GetComponentsInChildren<Transform>(true)) if (item.name == name) return item.gameObject;
            return null;
        }
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return; var split = path.LastIndexOf('/'); AssetDatabase.CreateFolder(path.Substring(0, split), path.Substring(split + 1));
        }
    }
}
