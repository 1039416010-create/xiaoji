using System;
using System.Collections.Generic;
using GroundChickenKing.Players;
using GroundChickenKing.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GroundChickenKing.Editor
{
    public static class Phase03SceneBuilder
    {
        private const string MainScenePath = "Assets/_Game/Scenes/SCN_Main.unity";
        private static readonly string[] ChickenIds =
        {
            "chicken-flash", "chicken-chubby", "chicken-tiny", "chicken-bro", "chicken-slacker",
        };
        private static readonly string[] ChickenLabels = { "闪电", "胖墩", "豆丁", "鸡哥", "摸鱼" };
        private static readonly int[] Stakes = { 1, 5, 10 };

        [MenuItem("Ground Chicken King/Configure Phase 03")]
        public static void ConfigurePhase03()
        {
            var scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            var safeArea = GameObject.Find("UI_Panel_SafeArea")?.GetComponent<RectTransform>();
            var presenter = GameObject.Find("GameSession")?.GetComponent<GameSessionPresenter>();
            var warmupPanel = FindSceneObject("UI_Panel_Warmup");
            if (safeArea == null || presenter == null || warmupPanel == null)
                throw new InvalidOperationException("Phase 02 scene objects are missing. Complete Phase 02 first.");

            DestroyIfPresent("UI_Button_EnterBetting");
            DestroyIfPresent("UI_Panel_Betting");
            DestroyIfPresent("UI_Panel_RaceCountdown");

            Move("UI_Button_WarmupBack", new Vector2(-255f, -270f));
            Move("UI_Button_WarmupMainMenu", new Vector2(255f, -270f));
            Move("UI_Text_WarmupNotice", new Vector2(0f, -55f));
            var enterBettingButton = CreateButton("UI_Button_EnterBetting", warmupPanel.transform, "进入下注", new Vector2(0f, -150f), new Vector2(400f, 95f), out _);

            var bettingPanel = CreatePanel("UI_Panel_Betting", safeArea, Color.clear);
            var bettingStatus = CreateText("UI_Text_BettingStatus", bettingPanel.transform, "等待玩家下注", 27, new Vector2(0f, 55f), new Vector2(900f, 55f));
            var bettingMainMenu = CreateButton("UI_Button_BettingMainMenu", bettingPanel.transform, "返回主菜单", new Vector2(0f, -55f), new Vector2(330f, 82f), out _);
            var bettingViews = new List<PlayerBettingView>
            {
                CreateBettingView(bettingPanel.transform, PlayerSeat.Player1, new Vector2(-450f, -305f), false, new Color(0.58f, 0.16f, 0.16f, 0.97f)),
                CreateBettingView(bettingPanel.transform, PlayerSeat.Player2, new Vector2(450f, -305f), false, new Color(0.10f, 0.30f, 0.58f, 0.97f)),
                CreateBettingView(bettingPanel.transform, PlayerSeat.Player3, new Vector2(-450f, 305f), true, new Color(0.58f, 0.47f, 0.08f, 0.97f)),
                CreateBettingView(bettingPanel.transform, PlayerSeat.Player4, new Vector2(450f, 305f), true, new Color(0.10f, 0.46f, 0.25f, 0.97f)),
            };

            var raceCountdownPanel = CreatePanel("UI_Panel_RaceCountdown", safeArea, Color.clear);
            CreateText("UI_Text_RaceCountdownTitle", raceCountdownPanel.transform, "下注已锁定", 72, new Vector2(0f, 145f), new Vector2(1000f, 120f));
            var raceCountdownSummary = CreateText("UI_Text_RaceCountdownSummary", raceCountdownPanel.transform, "等待 Phase 04", 34, Vector2.zero, new Vector2(1000f, 140f));
            var raceCountdownMainMenu = CreateButton("UI_Button_RaceCountdownMainMenu", raceCountdownPanel.transform, "返回主菜单", new Vector2(0f, -180f), new Vector2(380f, 100f), out _);

            bettingPanel.SetActive(false);
            raceCountdownPanel.SetActive(false);
            presenter.ConfigureBetting(
                bettingPanel,
                raceCountdownPanel,
                enterBettingButton,
                bettingMainMenu,
                raceCountdownMainMenu,
                bettingStatus,
                raceCountdownSummary,
                bettingViews.ToArray());

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MainScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[Bootstrap] Phase 03 scene configuration completed successfully. No race result system was created.");
        }

        public static void ConfigurePhase03FromCommandLine()
        {
            ConfigurePhase03();
        }

        private static PlayerBettingView CreateBettingView(Transform parent, PlayerSeat seat, Vector2 position, bool rotateForTopPlayer, Color color)
        {
            var panel = CreatePanel($"UI_Panel_Betting_{seat}", parent, color);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(840f, 250f);
            rect.localRotation = Quaternion.Euler(0f, 0f, rotateForTopPlayer ? 180f : 0f);

            CreateText("UI_Text_BettingTitle", panel.transform, $"{SeatIcon(seat)} 玩家 {(int)seat}", 28, new Vector2(-230f, 98f), new Vector2(340f, 42f));
            var balance = CreateText("UI_Text_Balance", panel.transform, "余额：50", 25, new Vector2(245f, 98f), new Vector2(260f, 42f));
            var selection = CreateText("UI_Text_Selection", panel.transform, "选择：未选鸡 · 未选金额", 23, new Vector2(0f, 58f), new Vector2(700f, 38f));

            var chickenButtons = new Button[ChickenIds.Length];
            for (var index = 0; index < ChickenIds.Length; index++)
            {
                var x = -264f + index * 132f;
                chickenButtons[index] = CreateButton($"UI_Button_BetChicken_{seat}_{index}", panel.transform, ChickenLabels[index], new Vector2(x, 10f), new Vector2(118f, 58f), out _);
            }

            var stakeButtons = new Button[Stakes.Length];
            for (var index = 0; index < Stakes.Length; index++)
            {
                var x = -260f + index * 122f;
                stakeButtons[index] = CreateButton($"UI_Button_BetStake_{seat}_{Stakes[index]}", panel.transform, Stakes[index].ToString(), new Vector2(x, -70f), new Vector2(105f, 62f), out _);
            }

            var lockButton = CreateButton($"UI_Button_LockBet_{seat}", panel.transform, "确认下注", new Vector2(240f, -70f), new Vector2(250f, 70f), out var lockLabel);
            lockButton.interactable = false;
            var view = panel.AddComponent<PlayerBettingView>();
            view.Configure(seat, ChickenIds, chickenButtons, Stakes, stakeButtons, lockButton, selection, balance, lockLabel);
            return view;
        }

        private static string SeatIcon(PlayerSeat seat)
        {
            return seat switch
            {
                PlayerSeat.Player1 => "◆",
                PlayerSeat.Player2 => "●",
                PlayerSeat.Player3 => "▲",
                PlayerSeat.Player4 => "■",
                _ => "?",
            };
        }

        private static void Move(string name, Vector2 position)
        {
            var rect = FindSceneObject(name)?.GetComponent<RectTransform>();
            if (rect == null) throw new InvalidOperationException($"Required UI object not found: {name}.");
            rect.anchoredPosition = position;
        }

        private static void DestroyIfPresent(string name)
        {
            var existing = FindSceneObject(name);
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing);
        }

        private static GameObject FindSceneObject(string name)
        {
            foreach (var transform in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (string.Equals(transform.name, name, StringComparison.Ordinal))
                    return transform.gameObject;
            }
            return null;
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
            colors.selectedColor = new Color(1f, 0.82f, 0.2f, 1f);
            colors.disabledColor = new Color(0.28f, 0.28f, 0.28f, 0.85f);
            button.colors = colors;
            labelText = CreateText("UI_Text_Label", gameObject.transform, label, Mathf.Min(30, Mathf.RoundToInt(size.y * 0.38f)), Vector2.zero, size);
            return button;
        }
    }
}
