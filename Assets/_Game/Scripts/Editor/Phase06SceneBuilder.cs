using System;
using System.Collections.Generic;
using GroundChickenKing.Chickens;
using GroundChickenKing.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GroundChickenKing.Editor
{
    public static class Phase06SceneBuilder
    {
        private const string ScenePath = "Assets/_Game/Scenes/SCN_Main.unity";
        private static readonly string[] ExtraIds = { "chicken-thunder", "chicken-dumpling", "chicken-captain", "chicken-sleepy", "chicken-rocket", "chicken-ninja", "chicken-scholar", "chicken-lucky" };
        private static readonly string[] ExtraNames = { "雷鸣鸡", "小笼包", "鸡队长", "瞌睡鸡", "火箭鸡", "忍者鸡", "学霸鸡", "幸运鸡" };

        [MenuItem("Ground Chicken King/Configure Phase 06")]
        public static void ConfigurePhase06()
        {
            var definitions = new List<ChickenDefinition>();
            for (var i = 1; i <= 5; i++) definitions.Add(AssetDatabase.LoadAssetAtPath<ChickenDefinition>($"Assets/_Game/Config/Chickens/CFG_Chicken_{i}.asset"));
            var sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Chickens/PF_Chicken_1.prefab");
            if (definitions.Contains(null) || sourcePrefab == null) throw new InvalidOperationException("Complete Phase 05 before Phase 06.");
            for (var index = 0; index < ExtraIds.Length; index++)
            {
                var number = index + 6; var prefabPath = $"Assets/_Game/Prefabs/Chickens/PF_Chicken_{number}.prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null) AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(sourcePrefab), prefabPath);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                var configPath = $"Assets/_Game/Config/Chickens/CFG_Chicken_{number}.asset";
                var definition = AssetDatabase.LoadAssetAtPath<ChickenDefinition>(configPath);
                if (definition == null) { definition = ScriptableObject.CreateInstance<ChickenDefinition>(); AssetDatabase.CreateAsset(definition, configPath); }
                var hue = (index * 0.117f + 0.08f) % 1f;
                definition.ConfigureEditor(ExtraIds[index], ExtraNames[index], "挑战者", prefab, Color.HSVToRGB(hue, 0.7f, 0.95f), new Vector2(0.85f + index % 3 * 0.12f, 0.85f + index % 2 * 0.18f));
                EditorUtility.SetDirty(definition); definitions.Add(definition);
            }
            var catalogPath = "Assets/_Game/Config/Chickens/CFG_ChickenRosterCatalog.asset";
            var catalog = AssetDatabase.LoadAssetAtPath<ChickenRosterCatalogConfig>(catalogPath);
            if (catalog == null) { catalog = ScriptableObject.CreateInstance<ChickenRosterCatalogConfig>(); AssetDatabase.CreateAsset(catalog, catalogPath); }
            catalog.ConfigureEditor(definitions.ToArray()); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var safeArea = GameObject.Find("UI_Panel_SafeArea")?.GetComponent<RectTransform>();
            var presenter = GameObject.Find("GameSession")?.GetComponent<GameSessionPresenter>();
            if (safeArea == null || presenter == null) throw new InvalidOperationException("Main scene is incomplete.");
            DestroyIfPresent("UI_Panel_Settlement"); DestroyIfPresent("UI_Panel_GameOver");

            var settlement = CreatePanel("UI_Panel_Settlement", safeArea);
            CreateText("UI_Text_SettlementTitle", settlement.transform, "本局结算", 66, new Vector2(0f, 335f), new Vector2(1100f, 90f));
            var settlementSummary = CreateText("UI_Text_SettlementSummary", settlement.transform, "等待比赛结果", 31, new Vector2(0f, 130f), new Vector2(1300f, 300f));
            var curry = CreateText("UI_Text_CurryResult", settlement.transform, "失败小鸡将成为咖喱鸡排饭", 28, new Vector2(0f, -105f), new Vector2(1500f, 120f));
            var next = CreateButton("UI_Button_NextRound", settlement.transform, "下一局", new Vector2(-225f, -310f));
            var settlementMain = CreateButton("UI_Button_SettlementMainMenu", settlement.transform, "返回主菜单", new Vector2(225f, -310f));

            var gameOver = CreatePanel("UI_Panel_GameOver", safeArea);
            CreateText("UI_Text_GameOverTitle", gameOver.transform, "游戏结束", 76, new Vector2(0f, 310f), new Vector2(1100f, 100f));
            var gameOverSummary = CreateText("UI_Text_GameOverSummary", gameOver.transform, "最终排名", 35, new Vector2(0f, 55f), new Vector2(1200f, 380f));
            var restart = CreateButton("UI_Button_Restart", gameOver.transform, "再来一局", new Vector2(-225f, -285f));
            var gameOverMain = CreateButton("UI_Button_GameOverMainMenu", gameOver.transform, "返回主菜单", new Vector2(225f, -285f));
            settlement.SetActive(false); gameOver.SetActive(false);
            presenter.ConfigureLoop(catalog, settlement, settlementSummary, curry, next, settlementMain, gameOver, gameOverSummary, restart, gameOverMain);
            EditorUtility.SetDirty(presenter); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene, ScenePath); AssetDatabase.SaveAssets();
            Debug.Log("[Bootstrap] Phase 06 settlement, 13-chicken roster catalog, loop, and GameOver UI configured.");
        }

        public static void ConfigurePhase06FromCommandLine() => ConfigurePhase06();
        private static GameObject CreatePanel(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>(); image.color = new Color(0.07f, 0.14f, 0.09f, 1f); image.raycastTarget = true; return go;
        }
        private static Text CreateText(string name, Transform parent, string value, int size, Vector2 position, Vector2 dimensions)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); rect.anchoredPosition = position; rect.sizeDelta = dimensions;
            var text = go.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = TextAnchor.MiddleCenter; text.color = Color.white; text.raycastTarget = false; return text;
        }
        private static Button CreateButton(string name, Transform parent, string label, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(360f, 92f);
            go.GetComponent<Image>().color = new Color(1f, 0.48f, 0.04f, 1f); var button = go.GetComponent<Button>();
            CreateText($"UI_Text_{name}", go.transform, label, 30, Vector2.zero, rect.sizeDelta); return button;
        }
        private static void DestroyIfPresent(string name) { var go = FindIncludingInactive(name); if (go != null) UnityEngine.Object.DestroyImmediate(go); }
        private static GameObject FindIncludingInactive(string name)
        {
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects()) foreach (var transform in root.GetComponentsInChildren<Transform>(true)) if (transform.name == name) return transform.gameObject;
            return null;
        }
    }
}
