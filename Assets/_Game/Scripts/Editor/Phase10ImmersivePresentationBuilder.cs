using System;
using System.Collections.Generic;
using System.IO;
using GroundChickenKing.Chickens;
using GroundChickenKing.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GroundChickenKing.Editor
{
    public static class Phase10ImmersivePresentationBuilder
    {
        private const string ScenePath = "Assets/_Game/Scenes/SCN_Main.unity";
        private const string SettlementBackgroundPath = "Assets/_Game/Art/Environment/Generated/BG_SettlementPodium_v2.png";
        private const string CurryAtlasPath = "Assets/_Game/Art/UI/Generated/SPR_CurryCutletAtlas_v2.png";
        private const string ButtonPath = "Assets/_Game/Art/UI/Generated/SPR_UI_WoodButton.png";
        private const string MaterialPath = "Assets/_Game/Art/Materials/MAT_Chicken3D_Runtime.mat";

        [MenuItem("Ground Chicken King/Configure Phase 10 Immersive Presentation")]
        public static void ConfigurePhase10()
        {
            Require(SettlementBackgroundPath);
            Require(CurryAtlasPath);
            var settlementBackground = ImportSprite(SettlementBackgroundPath, false);
            var curryAtlas = ImportTexture(CurryAtlasPath, true);
            var buttonSprite = ImportSprite(ButtonPath, true);
            var materialTemplate = EnsureMaterial();

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var warmup = Find("UI_Panel_Warmup")?.transform;
            var betting = Find("UI_Panel_Betting")?.transform;
            var raceTrack = Find("UI_Panel_RaceTrack")?.transform;
            var settlement = Find("UI_Panel_Settlement")?.transform;
            var settings = Find("UI_Panel_Settings")?.transform;
            var racePresenter = UnityEngine.Object.FindFirstObjectByType<ChickenRacePresenter>(FindObjectsInactive.Include);
            var session = UnityEngine.Object.FindFirstObjectByType<GameSessionPresenter>(FindObjectsInactive.Include);
            var settingsController = UnityEngine.Object.FindFirstObjectByType<ExhibitSettingsController>(FindObjectsInactive.Include);
            if (warmup == null || betting == null || raceTrack == null || settlement == null || settings == null || racePresenter == null || session == null || settingsController == null)
                throw new InvalidOperationException("Phase 10 requires the completed Phase 09 scene.");

            Destroy("ImmersiveChickenStage");
            foreach (var name in new[] { "UI_3D_Warmup", "UI_3D_Betting", "UI_3D_Race", "UI_3D_Settlement", "UI_Image_SettlementStage", "UI_Panel_SettlementCelebration", "UI_Button_Fullscreen", "UI_Text_Fullscreen" })
                Destroy(name);
            RemoveLaneBackdrops(warmup);
            RemoveLaneBackdrops(betting);

            AddLaneBackdrops(warmup, new Vector2(1540f, 520f));
            AddLaneBackdrops(betting, new Vector2(1540f, 500f));
            var warmupOutput = Output("UI_3D_Warmup", warmup, new Vector2(1540f, 520f), 7);
            var bettingOutput = Output("UI_3D_Betting", betting, new Vector2(1540f, 500f), 7);
            var raceOutput = Output("UI_3D_Race", raceTrack, Vector2.zero, raceTrack.childCount);
            Stretch(raceOutput.rectTransform);

            foreach (var controller in UnityEngine.Object.FindObjectsByType<ChickenController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var artwork = controller.transform.Find("Visual/Artwork");
                if (artwork != null) artwork.gameObject.SetActive(false);
            }

            var stageBackground = CreateImage("UI_Image_SettlementStage", settlement, settlementBackground);
            Stretch(stageBackground.rectTransform);
            stageBackground.transform.SetSiblingIndex(0);
            var oldFrame = settlement.Find("UI_Image_ArtFrame");
            if (oldFrame != null) oldFrame.gameObject.SetActive(false);
            var settlementOutput = Output("UI_3D_Settlement", settlement, Vector2.zero, 1);
            Stretch(settlementOutput.rectTransform);

            var stageObject = new GameObject("ImmersiveChickenStage", typeof(Chicken3DStage));
            var stage = stageObject.GetComponent<Chicken3DStage>();
            stage.Configure(warmupOutput, bettingOutput, raceOutput, settlementOutput, materialTemplate);
            racePresenter.ConfigureImmersiveStage(stage);

            var celebration = BuildSettlementCelebration(settlement, curryAtlas);
            session.ConfigureImmersivePresentation(celebration);
            ReflowSettlement(settlement);
            ReflowWarmup(warmup);

            var fullscreenLabel = Text("UI_Text_Fullscreen", settings, "显示模式：全屏", 32, new Vector2(0f, -170f), new Vector2(520f, 70f));
            var fullscreenButton = Button("UI_Button_Fullscreen", settings, "切换全屏 / 窗口", new Vector2(0f, -245f), new Vector2(440f, 82f), buttonSprite);
            settingsController.ConfigureFullscreen(fullscreenButton, fullscreenLabel);
            Move("UI_Button_SettingsBack", new Vector2(-250f, -350f));
            Move("UI_Button_SettingsSave", new Vector2(250f, -350f));

            EditorUtility.SetDirty(racePresenter);
            EditorUtility.SetDirty(session);
            EditorUtility.SetDirty(settingsController);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Presentation] Phase 10 procedural 3D chickens, live betting warmup, fullscreen option, and animated podium configured.");
        }

        public static void ConfigurePhase10FromCommandLine() => ConfigurePhase10();

        private static SettlementCelebrationView BuildSettlementCelebration(Transform settlement, Texture curryAtlas)
        {
            var root = new GameObject("UI_Panel_SettlementCelebration", typeof(RectTransform), typeof(SettlementCelebrationView));
            root.transform.SetParent(settlement, false);
            Stretch(root.GetComponent<RectTransform>());
            root.transform.SetSiblingIndex(2);
            var champion = Text("UI_Text_ChampionReveal", root.transform, "冠军即将登台", 54, new Vector2(0f, 340f), new Vector2(1100f, 84f));
            champion.color = new Color(1f, .76f, .12f);
            var loserCaption = Text("UI_Text_LoserMeals", root.transform, "其他选手成为今日特餐", 25, new Vector2(0f, -185f), new Vector2(1300f, 60f));
            var meals = new RawImage[4];
            for (var i = 0; i < meals.Length; i++)
            {
                meals[i] = AtlasImage($"UI_Image_CurryMeal_{i + 1}", root.transform, curryAtlas, new Rect(i * .25f, 0f, .25f, 1f), new Vector2(-510f + i * 340f, -290f), new Vector2(300f, 155f));
                meals[i].transform.localScale = Vector3.zero;
            }
            var confetti = new RectTransform[30];
            var colors = new[] { new Color(1f, .72f, .12f), new Color(.90f, .19f, .16f), new Color(.08f, .62f, .58f), new Color(1f, .94f, .70f) };
            for (var i = 0; i < confetti.Length; i++)
            {
                var go = new GameObject($"UI_Image_Confetti_{i + 1:00}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(root.transform, false);
                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
                rect.sizeDelta = new Vector2(10f + i % 3 * 5f, 24f);
                rect.anchoredPosition = new Vector2(-850f + i * 59f, 480f - i % 5 * 45f);
                var image = go.GetComponent<Image>(); image.color = colors[i % colors.Length]; image.raycastTarget = false;
                confetti[i] = rect;
            }
            var view = root.GetComponent<SettlementCelebrationView>();
            view.Configure(champion, loserCaption, meals, confetti);
            return view;
        }

        private static void AddLaneBackdrops(Transform parent, Vector2 area)
        {
            for (var lane = 0; lane < 5; lane++)
            {
                var go = new GameObject($"UI_Image_PreviewLane_{lane + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(parent, false);
                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
                rect.sizeDelta = new Vector2(area.x, area.y / 5f - 8f);
                rect.anchoredPosition = new Vector2(0f, area.y * .4f - lane * area.y / 5f);
                var image = go.GetComponent<Image>();
                image.color = lane % 2 == 0 ? new Color(.18f, .39f, .20f, .72f) : new Color(.42f, .23f, .10f, .72f);
                image.raycastTarget = false;
                go.transform.SetSiblingIndex(Mathf.Min(2 + lane, parent.childCount - 1));
            }
        }

        private static void RemoveLaneBackdrops(Transform parent)
        {
            var targets = new List<GameObject>();
            foreach (Transform child in parent)
                if (child.name.StartsWith("UI_Image_PreviewLane_", StringComparison.Ordinal)) targets.Add(child.gameObject);
            foreach (var target in targets) UnityEngine.Object.DestroyImmediate(target);
        }

        private static void ReflowSettlement(Transform settlement)
        {
            var title = settlement.Find("UI_Text_SettlementTitle")?.GetComponent<Text>();
            if (title != null) { title.text = "本局大奖典礼"; title.rectTransform.anchoredPosition = new Vector2(0f, 420f); title.fontSize = 58; }
            var summary = settlement.Find("UI_Text_SettlementSummary")?.GetComponent<Text>();
            if (summary != null) { summary.rectTransform.anchoredPosition = new Vector2(-610f, 35f); summary.rectTransform.sizeDelta = new Vector2(520f, 260f); summary.fontSize = 25; }
            var curry = settlement.Find("UI_Text_CurryResult"); if (curry != null) curry.gameObject.SetActive(false);
            Move("UI_Button_NextRound", new Vector2(-250f, -400f));
            Move("UI_Button_SettlementMainMenu", new Vector2(250f, -400f));
        }

        private static void ReflowWarmup(Transform warmup)
        {
            var frame = warmup.Find("UI_Image_ArtFrame"); if (frame != null) frame.gameObject.SetActive(false);
            Move("UI_Text_WarmupTitle", new Vector2(0f, 390f));
            Move("UI_Text_WarmupSummary", new Vector2(0f, 320f));
            Move("UI_Text_WarmupNotice", new Vector2(0f, -315f));
            Move("UI_Button_EnterBetting", new Vector2(0f, -395f));
            Move("UI_Button_WarmupBack", new Vector2(-520f, -395f));
            Move("UI_Button_WarmupMainMenu", new Vector2(520f, -395f));
        }

        private static RawImage Output(string name, Transform parent, Vector2 size, int sibling)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.sizeDelta = size;
            var image = go.GetComponent<RawImage>(); image.color = Color.white; image.raycastTarget = false;
            go.transform.SetSiblingIndex(Mathf.Clamp(sibling, 0, parent.childCount - 1));
            return image;
        }

        private static RawImage AtlasImage(string name, Transform parent, Texture texture, Rect uv, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = size;
            var image = go.GetComponent<RawImage>(); image.texture = texture; image.uvRect = uv; image.raycastTarget = false; return image;
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>(); image.sprite = sprite; image.color = Color.white; image.raycastTarget = false; image.preserveAspect = false; return image;
        }

        private static Text Text(string name, Transform parent, string value, int size, Vector2 position, Vector2 dimensions)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Outline)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = dimensions;
            var text = go.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.fontStyle = FontStyle.Bold; text.alignment = TextAnchor.MiddleCenter; text.color = new Color(1f, .94f, .78f); text.raycastTarget = false;
            var outline = go.GetComponent<Outline>(); outline.effectColor = new Color(.10f, .04f, .01f, .95f); outline.effectDistance = new Vector2(2f, -2f); return text;
        }

        private static Button Button(string name, Transform parent, string label, Vector2 position, Vector2 size, Sprite sprite)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(TouchButtonFeedback)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = size;
            var image = go.GetComponent<Image>(); image.sprite = sprite; image.type = Image.Type.Sliced; image.color = Color.white;
            var button = go.GetComponent<Button>(); button.navigation = new Navigation { mode = Navigation.Mode.None };
            Text("UI_Text_Label", go.transform, label, 28, Vector2.zero, size);
            return button;
        }

        private static Sprite ImportSprite(string path, bool alpha)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException($"Texture importer unavailable: {path}");
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.alphaIsTransparency = alpha;
            importer.mipmapEnabled = false; importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.CompressedHQ; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Texture2D ImportTexture(string path, bool alpha)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException($"Texture importer unavailable: {path}");
            importer.textureType = TextureImporterType.Default; importer.alphaIsTransparency = alpha; importer.mipmapEnabled = false; importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 4096; importer.textureCompression = TextureImporterCompression.CompressedHQ; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Material EnsureMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader is unavailable.");
            if (material == null)
            {
                material = new Material(shader) { name = "MAT_Chicken3D_Runtime" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else material.shader = shader;
            material.color = Color.white;
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", .34f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }
        private static void Move(string name, Vector2 position) { var rect = Find(name)?.GetComponent<RectTransform>(); if (rect != null) rect.anchoredPosition = position; }
        private static void Require(string path) { if (!File.Exists(path)) throw new FileNotFoundException("Required generated art is missing.", path); }
        private static void Destroy(string name) { var target = Find(name); if (target != null) UnityEngine.Object.DestroyImmediate(target); }
        private static GameObject Find(string name)
        {
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
                foreach (var child in root.GetComponentsInChildren<Transform>(true))
                    if (child.name == name) return child.gameObject;
            return null;
        }
    }
}
