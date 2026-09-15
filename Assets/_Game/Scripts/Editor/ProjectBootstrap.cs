using System;
using System.IO;
using GroundChickenKing.Core;
using GroundChickenKing.Diagnostics;
using GroundChickenKing.Flow;
using GroundChickenKing.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GroundChickenKing.Editor
{
    public static class ProjectBootstrap
    {
        private const string Root = "Assets/_Game";
        private const string Settings = Root + "/Settings";
        private const string Config = Root + "/Config";
        private const string Scenes = Root + "/Scenes";
        private const string InputActionsPath = Settings + "/GroundChickenKingUI.inputactions";

        [MenuItem("Ground Chicken King/Configure Phase 01")]
        public static void ConfigurePhase01()
        {
            CreateDirectories();
            ConfigurePlayerSettings();
            ConfigureRenderPipeline();
            CreateDefaultConfigs();
            var inputActions = CreateInputActions();
            CreateBootScene();
            CreateMainScene(inputActions);
            ConfigureBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var errors = ProjectConfigurationValidator.ValidateAllConfigs();
            if (errors.Count > 0)
                throw new InvalidOperationException(string.Join(Environment.NewLine, errors));

            Debug.Log("[Bootstrap] Phase 01 project configuration completed successfully.");
        }

        public static void ConfigurePhase01FromCommandLine()
        {
            ConfigurePhase01();
        }

        private static void CreateDirectories()
        {
            var directories = new[]
            {
                "Art/Characters", "Art/Environment", "Art/Materials", "Art/UI", "Art/VFX",
                "Audio/Music", "Audio/SFX", "Audio/Mixers",
                "Config/Chickens", "Config/Race", "Config/Economy", "Config/Presentation",
                "Prefabs/Chickens", "Prefabs/Race", "Prefabs/UI", "Scenes", "Settings",
                "Scripts/Runtime/Betting", "Scripts/Runtime/Race", "Scripts/Runtime/Chickens",
                "Scripts/Runtime/Input", "Scripts/Runtime/Audio", "Scripts/Runtime/Persistence",
            };

            foreach (var relativePath in directories)
                Directory.CreateDirectory(Path.Combine(Root, relativePath));
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "GroundChickenKing";
            PlayerSettings.productName = "走地鸡王";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 960;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.resizableWindow = false;
            PlayerSettings.runInBackground = true;
            PlayerSettings.forceSingleInstance = true;
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        }

        private static void ConfigureRenderPipeline()
        {
            var rendererPath = Settings + "/GCK_UniversalRenderer.asset";
            var pipelinePath = Settings + "/GCK_UniversalRenderPipeline.asset";
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, rendererPath);
            }

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, pipelinePath);
            }

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
        }

        private static void CreateDefaultConfigs()
        {
            CreateAssetIfMissing<GameRulesConfig>(Config + "/Economy/CFG_GameRules_Default.asset");
            CreateAssetIfMissing<RaceConfig>(Config + "/Race/CFG_Race_Default.asset");
            CreateAssetIfMissing<PresentationConfig>(Config + "/Presentation/CFG_Presentation_Default.asset");
            CreateAssetIfMissing<TouchConfig>(Config + "/Presentation/CFG_Touch_Default.asset");
        }

        private static InputActionAsset CreateInputActions()
        {
            AssetDatabase.ImportAsset(InputActionsPath, ImportAssetOptions.ForceSynchronousImport);
            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (asset == null)
                throw new InvalidOperationException($"Unable to import input actions at {InputActionsPath}.");
            return asset;
        }

        private static void CreateBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "SCN_Boot";
            new GameObject("GameBootstrapper", typeof(GameBootstrapper));
            EditorSceneManager.SaveScene(scene, Scenes + "/SCN_Boot.unity");
        }

        private static void CreateMainScene(InputActionAsset inputActions)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "SCN_Main";

            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(AspectRatioController));
            cameraObject.tag = "MainCamera";
            cameraObject.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            cameraObject.GetComponent<Camera>().backgroundColor = Color.black;
            new GameObject("DevelopmentVisualVerifier", typeof(DevelopmentScreenshotCapture));

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
            inputModule.move = null;
            inputModule.submit = null;
            inputModule.cancel = null;
            inputModule.scrollWheel = null;
            inputModule.trackedDevicePosition = null;
            inputModule.trackedDeviceOrientation = null;

            var canvasObject = new GameObject("UI_Canvas_Root", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = DisplayAspectUtility.ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var backdrop = CreateImage("UI_Image_Backdrop", canvasObject.transform, Color.black);
            Stretch(backdrop.rectTransform);

            var content = CreateImage("UI_Panel_ContentRoot", canvasObject.transform, new Color(0.12f, 0.18f, 0.12f, 1f));
            Stretch(content.rectTransform);
            var fitter = content.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = DisplayAspectUtility.TargetAspect;

            var safeArea = new GameObject("UI_Panel_SafeArea", typeof(RectTransform));
            safeArea.transform.SetParent(content.transform, false);
            var safeRect = safeArea.GetComponent<RectTransform>();
            safeRect.anchorMin = new Vector2(0.03f, 0.05f);
            safeRect.anchorMax = new Vector2(0.97f, 0.95f);
            safeRect.offsetMin = Vector2.zero;
            safeRect.offsetMax = Vector2.zero;

            CreateText("UI_Text_Title", safeArea.transform, "走地鸡王", 82, new Vector2(0f, 180f), new Vector2(900f, 140f));
            CreateText("UI_Text_Status", safeArea.transform, "Phase 01 · 1920 × 960 · Touch UI Ready", 34, new Vector2(0f, 45f), new Vector2(1000f, 90f));

            var exitHandler = new GameObject("ApplicationActions", typeof(AppExitButton)).GetComponent<AppExitButton>();
            var exitButton = CreateButton("UI_Button_ExitGame", safeArea.transform, "退出游戏", new Vector2(0f, -190f));
            UnityEventTools.AddPersistentListener(exitButton.onClick, exitHandler.ExitApplication);

            EditorSceneManager.SaveScene(scene, Scenes + "/SCN_Main.unity");
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            gameObject.transform.SetParent(parent, false);
            var image = gameObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text CreateText(string name, Transform parent, string value, int fontSize, Vector2 position, Vector2 size)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            gameObject.transform.SetParent(parent, false);
            var rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var text = gameObject.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 position)
        {
            var image = CreateImage(name, parent, new Color(0.95f, 0.55f, 0.1f, 1f));
            image.raycastTarget = true;
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(360f, 120f);
            var button = image.gameObject.AddComponent<Button>();
            CreateText("UI_Text_Label", image.transform, label, 42, Vector2.zero, rect.sizeDelta);
            return button;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void ConfigureBuildScenes()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(Scenes + "/SCN_Boot.unity", true),
                new EditorBuildSettingsScene(Scenes + "/SCN_Main.unity", true),
            };
        }

        private static void CreateAssetIfMissing<T>(string path) where T : ScriptableObject
        {
            if (AssetDatabase.LoadAssetAtPath<T>(path) != null)
                return;

            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
        }
    }
}
