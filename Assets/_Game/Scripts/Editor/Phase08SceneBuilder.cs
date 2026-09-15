using System;
using GroundChickenKing.Input;
using GroundChickenKing.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GroundChickenKing.Editor
{
    public static class Phase08SceneBuilder
    {
        private const string ScenePath = "Assets/_Game/Scenes/SCN_Main.unity";

        [MenuItem("Ground Chicken King/Configure Phase 08")]
        public static void ConfigurePhase08()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var safeArea = Find("UI_Panel_SafeArea")?.GetComponent<RectTransform>(); var diagnostics = Find("UI_Panel_Diagnostics")?.transform;
            var operations = Find("ExhibitOperations");
            if (safeArea == null || diagnostics == null || operations == null) throw new InvalidOperationException("Complete Phase 07 before Phase 08.");
            Destroy("UI_Panel_TouchCalibration"); Destroy("UI_Button_TouchCalibration"); Destroy("UI_Button_TouchVisualization");
            var oldCalibration = operations.GetComponent<TouchCalibrationController>(); if (oldCalibration != null) UnityEngine.Object.DestroyImmediate(oldCalibration);
            var oldOverlay = operations.GetComponent<TouchVisualizationOverlay>(); if (oldOverlay != null) UnityEngine.Object.DestroyImmediate(oldOverlay);

            var close = Find("UI_Button_DiagnosticsClose")?.GetComponent<Button>(); if (close == null) throw new InvalidOperationException("Diagnostics close button is missing.");
            var closeRect = close.GetComponent<RectTransform>(); closeRect.anchoredPosition = new Vector2(0, -355); closeRect.sizeDelta = new Vector2(300, 85);
            var calibrationOpen = Button("UI_Button_TouchCalibration", diagnostics, "五点校准", new Vector2(-470, -355), new Vector2(330, 85), out _);
            var visualize = Button("UI_Button_TouchVisualization", diagnostics, "开启触点显示", new Vector2(470, -355), new Vector2(360, 85), out var visualizeLabel);

            var panel = Panel("UI_Panel_TouchCalibration", safeArea, new Color(.025f, .08f, .06f, .995f));
            Text("UI_Text_TouchCalibrationTitle", panel.transform, "触控覆盖检查", 52, new Vector2(0, 345), new Vector2(900, 80));
            var status = Text("UI_Text_TouchCalibrationStatus", panel.transform, "依次触摸四角与中心的目标", 30, new Vector2(0, 275), new Vector2(1050, 60));
            var targets = new[]
            {
                Button("UI_Button_CalibrationTopLeft", panel.transform, "1", new Vector2(-720, 250), new Vector2(160, 130), out _),
                Button("UI_Button_CalibrationTopRight", panel.transform, "2", new Vector2(720, 250), new Vector2(160, 130), out _),
                Button("UI_Button_CalibrationCenter", panel.transform, "3", Vector2.zero, new Vector2(180, 150), out _),
                Button("UI_Button_CalibrationBottomLeft", panel.transform, "4", new Vector2(-720, -250), new Vector2(160, 130), out _),
                Button("UI_Button_CalibrationBottomRight", panel.transform, "5", new Vector2(720, -250), new Vector2(160, 130), out _),
            };
            var calibrationClose = Button("UI_Button_TouchCalibrationClose", panel.transform, "返回诊断", new Vector2(0, -365), new Vector2(340, 80), out _); panel.SetActive(false);
            var controller = operations.AddComponent<TouchCalibrationController>(); controller.Configure(panel, calibrationOpen, calibrationClose, targets, status);
            var overlay = operations.AddComponent<TouchVisualizationOverlay>(); overlay.Configure(safeArea, visualize, visualizeLabel);
            foreach (var button in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (button.GetComponent<TouchButtonFeedback>() == null) button.gameObject.AddComponent<TouchButtonFeedback>();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene, ScenePath); AssetDatabase.SaveAssets();
            Debug.Log("[Bootstrap] Phase 08 touch calibration and Development touch visualization configured.");
        }

        public static void ConfigurePhase08FromCommandLine() => ConfigurePhase08();
        private static GameObject Panel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); go.transform.SetParent(parent, false); var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; var image = go.GetComponent<Image>(); image.color = color; image.raycastTarget = true; return go;
        }
        private static Text Text(string name, Transform parent, string value, int size, Vector2 position, Vector2 dimensions)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); go.transform.SetParent(parent, false); var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = dimensions; var text = go.GetComponent<Text>(); text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = TextAnchor.MiddleCenter; text.color = Color.white; text.raycastTarget = false; return text;
        }
        private static Button Button(string name, Transform parent, string label, Vector2 position, Vector2 size, out Text labelText)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false); var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = size; go.GetComponent<Image>().color = new Color(1f, .48f, .04f, 1f);
            labelText = Text("UI_Text_Label", go.transform, label, Mathf.Min(32, Mathf.RoundToInt(size.y * .34f)), Vector2.zero, size); return go.GetComponent<Button>();
        }
        private static void Destroy(string name) { var target = Find(name); if (target != null) UnityEngine.Object.DestroyImmediate(target); }
        private static GameObject Find(string name)
        {
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects()) foreach (var transform in root.GetComponentsInChildren<Transform>(true)) if (transform.name == name) return transform.gameObject; return null;
        }
    }
}
