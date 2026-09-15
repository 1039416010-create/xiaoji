using System;
using System.Reflection;
using GroundChickenKing.Audio;
using GroundChickenKing.Diagnostics;
using GroundChickenKing.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace GroundChickenKing.Editor
{
    public static class Phase07SceneBuilder
    {
        private const string ScenePath = "Assets/_Game/Scenes/SCN_Main.unity";
        private const string MixerPath = "Assets/_Game/Audio/Mixers/MIX_Exhibit.mixer";

        [MenuItem("Ground Chicken King/Configure Phase 07")]
        public static void ConfigurePhase07()
        {
            var mixer = EnsureMixer();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var safeArea = Find("UI_Panel_SafeArea")?.GetComponent<RectTransform>();
            var mainMenu = Find("UI_Panel_MainMenu")?.transform;
            var presenter = Find("GameSession")?.GetComponent<GameSessionPresenter>();
            if (safeArea == null || mainMenu == null || presenter == null) throw new InvalidOperationException("Complete Phase 06 before Phase 07.");

            Destroy("UI_Panel_Settings"); Destroy("UI_Panel_Diagnostics"); Destroy("UI_Panel_Confirmation"); Destroy("UI_Image_VisualIntensity"); Destroy("ExhibitOperations");
            var settingsOpen = Find("UI_Button_Settings")?.GetComponent<Button>();
            if (settingsOpen == null) throw new InvalidOperationException("Settings button is missing.");
            settingsOpen.interactable = true; SetLabel(settingsOpen, "设置");

            var settings = Panel("UI_Panel_Settings", safeArea, new Color(0.04f, 0.11f, 0.08f, 0.99f));
            Text("UI_Text_SettingsTitle", settings.transform, "展项设置", 62, new Vector2(0, 340), new Vector2(1000, 90));
            var volumeValue = Text("UI_Text_VolumeValue", settings.transform, "音量 80%", 34, new Vector2(0, 190), new Vector2(500, 70));
            var volumeDown = Button("UI_Button_VolumeDown", settings.transform, "－", new Vector2(-350, 190), new Vector2(150, 90));
            var volumeUp = Button("UI_Button_VolumeUp", settings.transform, "＋", new Vector2(350, 190), new Vector2(150, 90));
            var intensityValue = Text("UI_Text_IntensityValue", settings.transform, "视觉强度 100%", 34, new Vector2(0, 55), new Vector2(500, 70));
            var intensityDown = Button("UI_Button_IntensityDown", settings.transform, "－", new Vector2(-350, 55), new Vector2(150, 90));
            var intensityUp = Button("UI_Button_IntensityUp", settings.transform, "＋", new Vector2(350, 55), new Vector2(150, 90));
            var roundsValue = Text("UI_Text_RoundsValue", settings.transform, "最大局数 20", 34, new Vector2(0, -80), new Vector2(500, 70));
            var roundsDown = Button("UI_Button_RoundsDown", settings.transform, "－", new Vector2(-350, -80), new Vector2(150, 90));
            var roundsUp = Button("UI_Button_RoundsUp", settings.transform, "＋", new Vector2(350, -80), new Vector2(150, 90));
            var back = Button("UI_Button_SettingsBack", settings.transform, "取消", new Vector2(-230, -300), new Vector2(360, 100));
            var save = Button("UI_Button_SettingsSave", settings.transform, "保存", new Vector2(230, -300), new Vector2(360, 100)); settings.SetActive(false);

            var diagnosticsOpen = Button("UI_Button_Diagnostics", mainMenu, "设备诊断", new Vector2(680, -350), new Vector2(280, 82));
            var diagnostics = Panel("UI_Panel_Diagnostics", safeArea, new Color(0.035f, 0.075f, 0.11f, 0.995f));
            Text("UI_Text_DiagnosticsTitle", diagnostics.transform, "展项诊断", 60, new Vector2(0, 345), new Vector2(900, 90));
            var diagnosticsSummary = Text("UI_Text_DiagnosticsSummary", diagnostics.transform, "正在读取设备状态", 27, new Vector2(0, 25), new Vector2(1450, 560));
            var diagnosticsClose = Button("UI_Button_DiagnosticsClose", diagnostics.transform, "返回", new Vector2(0, -340), new Vector2(380, 95)); diagnostics.SetActive(false);

            var confirmation = Panel("UI_Panel_Confirmation", safeArea, new Color(0.01f, 0.025f, 0.02f, 0.96f));
            var confirmationMessage = Text("UI_Text_ConfirmationMessage", confirmation.transform, "请确认操作", 38, new Vector2(0, -25), new Vector2(1100, 170));
            var mirroredMessage = Text("UI_Text_ConfirmationMessage_Mirrored", confirmation.transform, "请确认操作", 38, new Vector2(0, 245), new Vector2(1100, 170));
            mirroredMessage.rectTransform.localRotation = Quaternion.Euler(0, 0, 180);
            var cancel = Button("UI_Button_ConfirmationCancel", confirmation.transform, "取消", new Vector2(-235, -150), new Vector2(390, 105));
            var confirm = Button("UI_Button_ConfirmationConfirm", confirmation.transform, "确认", new Vector2(235, -150), new Vector2(390, 105));
            SetLabel(cancel, "✕"); SetLabel(confirm, "✓");
            var dialog = confirmation.AddComponent<ConfirmationDialog>(); dialog.Configure(confirmation, confirmationMessage, mirroredMessage, confirm, cancel); confirmation.SetActive(false);

            var overlayObject = new GameObject("UI_Image_VisualIntensity", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); overlayObject.transform.SetParent(safeArea, false);
            var overlayRect = overlayObject.GetComponent<RectTransform>(); overlayRect.anchorMin = Vector2.zero; overlayRect.anchorMax = Vector2.one; overlayRect.offsetMin = overlayRect.offsetMax = Vector2.zero;
            var overlay = overlayObject.GetComponent<Image>(); overlay.color = new Color(0, 0, 0, 0); overlay.raycastTarget = false; overlayObject.transform.SetAsLastSibling();
            confirmation.transform.SetAsLastSibling();

            var operations = new GameObject("ExhibitOperations");
            var audio = operations.AddComponent<ExhibitAudioController>();
            var master = mixer != null ? mixer.FindMatchingGroups("Master") : Array.Empty<AudioMixerGroup>();
            var music = mixer != null ? mixer.FindMatchingGroups("Music") : Array.Empty<AudioMixerGroup>();
            var sfx = mixer != null ? mixer.FindMatchingGroups("SFX") : Array.Empty<AudioMixerGroup>();
            audio.Configure(mixer, music.Length > 0 ? music[0] : master.Length > 0 ? master[0] : null, sfx.Length > 0 ? sfx[0] : master.Length > 0 ? master[0] : null);
            operations.AddComponent<BoundedLogWriter>();
            var settingsController = operations.AddComponent<ExhibitSettingsController>();
            settingsController.Configure(settings, settingsOpen, volumeDown, volumeUp, intensityDown, intensityUp, roundsDown, roundsUp, save, back, volumeValue, intensityValue, roundsValue, overlay, presenter, audio);
            var diagnosticController = operations.AddComponent<ExhibitDiagnosticsPanel>(); diagnosticController.Configure(diagnostics, diagnosticsSummary, diagnosticsOpen, diagnosticsClose, presenter);
            var idle = operations.AddComponent<ExhibitIdleController>(); idle.Configure(presenter, 120f);
            presenter.ConfigureOperations(dialog, audio);
            var exit = Find("ApplicationActions")?.GetComponent<AppExitButton>(); exit?.Configure(dialog);

            foreach (var button in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (button.GetComponent<TouchButtonFeedback>() == null) button.gameObject.AddComponent<TouchButtonFeedback>();
                var colors = button.colors; colors.normalColor = Color.white; colors.highlightedColor = Color.white; colors.pressedColor = new Color(1f, 0.72f, 0.25f, 1f);
                colors.disabledColor = new Color(0.24f, 0.27f, 0.25f, 0.9f); colors.selectedColor = colors.normalColor; button.colors = colors;
                button.navigation = new Navigation { mode = Navigation.Mode.None };
            }

            EditorUtility.SetDirty(presenter); if (exit != null) EditorUtility.SetDirty(exit); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene, ScenePath); AssetDatabase.SaveAssets();
            Debug.Log("[Bootstrap] Phase 07 settings, confirmation, audio, diagnostics, idle recovery, bounded logging, and touch feedback configured.");
        }

        public static void ConfigurePhase07FromCommandLine() => ConfigurePhase07();

        private static AudioMixer EnsureMixer()
        {
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath); if (mixer != null) return mixer;
            var type = Type.GetType("UnityEditor.Audio.AudioMixerController, UnityEditor");
            var create = type?.GetMethod("CreateMixerControllerAtPath", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var controller = create?.Invoke(null, new object[] { MixerPath });
            var groupMethod = type?.GetMethod("CreateNewGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(bool) }, null);
            groupMethod?.Invoke(controller, new object[] { "Music", true }); groupMethod?.Invoke(controller, new object[] { "SFX", true });
            AssetDatabase.SaveAssets(); return AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
        }

        private static GameObject Panel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>(); image.color = color; image.raycastTarget = color.a > 0; return go;
        }
        private static Text Text(string name, Transform parent, string value, int size, Vector2 position, Vector2 dimensions)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = dimensions;
            var text = go.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = TextAnchor.MiddleCenter; text.color = Color.white; text.raycastTarget = false; return text;
        }
        private static Button Button(string name, Transform parent, string label, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(1f, .48f, .04f, 1f); var button = go.GetComponent<Button>(); Text("UI_Text_Label", go.transform, label, Mathf.Min(34, Mathf.RoundToInt(size.y * .34f)), Vector2.zero, size); return button;
        }
        private static void SetLabel(Button button, string value) { var text = button.GetComponentInChildren<Text>(true); if (text != null) text.text = value; }
        private static void Destroy(string name) { var target = Find(name); if (target != null) UnityEngine.Object.DestroyImmediate(target); }
        private static GameObject Find(string name)
        {
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects()) foreach (var transform in root.GetComponentsInChildren<Transform>(true)) if (transform.name == name) return transform.gameObject;
            return null;
        }
    }
}
