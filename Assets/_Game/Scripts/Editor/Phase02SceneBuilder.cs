using System;
using System.Collections.Generic;
using GroundChickenKing.Core;
using GroundChickenKing.Diagnostics;
using GroundChickenKing.Players;
using GroundChickenKing.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GroundChickenKing.Editor
{
    public static class Phase02SceneBuilder
    {
        private const string MainScenePath = "Assets/_Game/Scenes/SCN_Main.unity";
        private const string RulesPath = "Assets/_Game/Config/Economy/CFG_GameRules_Default.asset";

        [MenuItem("Ground Chicken King/Configure Phase 02")]
        public static void ConfigurePhase02()
        {
            var scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            var safeArea = GameObject.Find("UI_Panel_SafeArea")?.GetComponent<RectTransform>();
            if (safeArea == null)
                throw new InvalidOperationException("Phase 01 safe area is missing from SCN_Main.");

            for (var index = safeArea.childCount - 1; index >= 0; index--)
                UnityEngine.Object.DestroyImmediate(safeArea.GetChild(index).gameObject);

            var existingSession = GameObject.Find("GameSession");
            if (existingSession != null)
                UnityEngine.Object.DestroyImmediate(existingSession);

            var existingDriver = GameObject.Find("DevelopmentFlowDriver");
            if (existingDriver != null)
                UnityEngine.Object.DestroyImmediate(existingDriver);

            var applicationActions = GameObject.Find("ApplicationActions");
            var exitHandler = applicationActions != null
                ? applicationActions.GetComponent<AppExitButton>()
                : new GameObject("ApplicationActions", typeof(AppExitButton)).GetComponent<AppExitButton>();

            var mainMenu = CreatePanel("UI_Panel_MainMenu", safeArea, Color.clear);
            CreateText("UI_Text_MainTitle", mainMenu.transform, "走地鸡王", 82, new Vector2(0f, 230f), new Vector2(900f, 130f));
            CreateText("UI_Text_MainSubtitle", mainMenu.transform, "1–4 人桌面触控竞猜", 34, new Vector2(0f, 120f), new Vector2(900f, 70f));
            var startButton = CreateButton("UI_Button_StartGame", mainMenu.transform, "开始游戏", new Vector2(0f, -10f), new Vector2(430f, 115f), out _);
            var settingsButton = CreateButton("UI_Button_Settings", mainMenu.transform, "设置（Phase 07）", new Vector2(0f, -155f), new Vector2(430f, 105f), out _);
            settingsButton.interactable = false;
            var exitButton = CreateButton("UI_Button_ExitGame", mainMenu.transform, "退出游戏", new Vector2(0f, -290f), new Vector2(430f, 105f), out _);
            UnityEventTools.AddPersistentListener(exitButton.onClick, exitHandler.ExitApplication);

            var playerJoin = CreatePanel("UI_Panel_PlayerJoin", safeArea, Color.clear);
            CreateText("UI_Text_PlayerJoinTitle", playerJoin.transform, "点击自己面前的按钮加入", 40, new Vector2(0f, 25f), new Vector2(900f, 65f));
            var joinSummary = CreateText("UI_Text_JoinSummary", playerJoin.transform, "请至少加入 1 名玩家", 28, new Vector2(0f, -55f), new Vector2(850f, 60f));
            var joinBackButton = CreateButton("UI_Button_JoinBack", playerJoin.transform, "返回主菜单", new Vector2(-265f, -125f), new Vector2(360f, 90f), out _);
            var continueButton = CreateButton("UI_Button_ContinueToWarmup", playerJoin.transform, "开始热身", new Vector2(265f, -125f), new Vector2(360f, 90f), out _);
            continueButton.interactable = false;

            var seatViews = new List<PlayerSeatView>
            {
                CreateSeatView(playerJoin.transform, PlayerSeat.Player1, new Vector2(-455f, -315f), false, new Color(0.58f, 0.16f, 0.16f, 0.97f)),
                CreateSeatView(playerJoin.transform, PlayerSeat.Player2, new Vector2(455f, -315f), false, new Color(0.10f, 0.30f, 0.58f, 0.97f)),
                CreateSeatView(playerJoin.transform, PlayerSeat.Player3, new Vector2(-455f, 315f), true, new Color(0.58f, 0.47f, 0.08f, 0.97f)),
                CreateSeatView(playerJoin.transform, PlayerSeat.Player4, new Vector2(455f, 315f), true, new Color(0.10f, 0.46f, 0.25f, 0.97f)),
            };

            var warmup = CreatePanel("UI_Panel_Warmup", safeArea, Color.clear);
            CreateText("UI_Text_WarmupTitle", warmup.transform, "小鸡热身中", 76, new Vector2(0f, 145f), new Vector2(900f, 130f));
            var warmupSummary = CreateText("UI_Text_WarmupSummary", warmup.transform, "玩家已准备", 34, new Vector2(0f, 20f), new Vector2(950f, 80f));
            CreateText("UI_Text_WarmupNotice", warmup.transform, "本阶段不会自动进入下注", 26, new Vector2(0f, -65f), new Vector2(950f, 60f));
            var warmupBackButton = CreateButton("UI_Button_WarmupBack", warmup.transform, "返回玩家加入", new Vector2(-255f, -205f), new Vector2(380f, 100f), out _);
            var warmupMainMenuButton = CreateButton("UI_Button_WarmupMainMenu", warmup.transform, "返回主菜单", new Vector2(255f, -205f), new Vector2(380f, 100f), out _);

            mainMenu.SetActive(true);
            playerJoin.SetActive(false);
            warmup.SetActive(false);

            var sessionObject = new GameObject("GameSession", typeof(GameSessionPresenter));
            var presenter = sessionObject.GetComponent<GameSessionPresenter>();
            var rules = AssetDatabase.LoadAssetAtPath<GameRulesConfig>(RulesPath);
            if (rules == null)
                throw new InvalidOperationException($"Game rules asset is missing at {RulesPath}.");

            presenter.Configure(
                rules,
                mainMenu,
                playerJoin,
                warmup,
                startButton,
                continueButton,
                joinBackButton,
                warmupBackButton,
                warmupMainMenuButton,
                joinSummary,
                warmupSummary,
                seatViews.ToArray());

            new GameObject("DevelopmentFlowDriver", typeof(DevelopmentFlowDriver));
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MainScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[Bootstrap] Phase 02 scene configuration completed successfully.");
        }

        public static void ConfigurePhase02FromCommandLine()
        {
            ConfigurePhase02();
        }

        private static PlayerSeatView CreateSeatView(Transform parent, PlayerSeat seat, Vector2 position, bool rotateForTopPlayer, Color color)
        {
            var panel = CreatePanel($"UI_Panel_{seat}", parent, color);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(760f, 220f);
            rect.localRotation = Quaternion.Euler(0f, 0f, rotateForTopPlayer ? 180f : 0f);

            var title = CreateText("UI_Text_PlayerTitle", panel.transform, string.Empty, 34, new Vector2(0f, 72f), new Vector2(600f, 48f));
            var status = CreateText("UI_Text_PlayerStatus", panel.transform, "等待加入", 25, new Vector2(-175f, 18f), new Vector2(300f, 42f));
            var coins = CreateText("UI_Text_PlayerCoins", panel.transform, "初始金币：50", 25, new Vector2(-175f, -30f), new Vector2(300f, 42f));
            var joinButton = CreateButton($"UI_Button_Join_{seat}", panel.transform, "点击加入", new Vector2(205f, -10f), new Vector2(285f, 105f), out var joinLabel);
            var view = panel.AddComponent<PlayerSeatView>();
            view.Configure(seat, title, status, coins, joinLabel, joinButton);
            return view;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = panel.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = color.a > 0f;
            return panel;
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

        private static Button CreateButton(string name, Transform parent, string label, Vector2 position, Vector2 size, out Text labelText)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            gameObject.transform.SetParent(parent, false);
            var rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = gameObject.GetComponent<Image>();
            image.color = new Color(0.95f, 0.55f, 0.1f, 1f);
            var button = gameObject.GetComponent<Button>();
            var colors = button.colors;
            colors.pressedColor = new Color(1f, 0.72f, 0.3f, 1f);
            colors.disabledColor = new Color(0.28f, 0.28f, 0.28f, 0.85f);
            button.colors = colors;
            labelText = CreateText("UI_Text_Label", gameObject.transform, label, Mathf.Min(36, Mathf.RoundToInt(size.y * 0.35f)), Vector2.zero, size);
            return button;
        }
    }
}
