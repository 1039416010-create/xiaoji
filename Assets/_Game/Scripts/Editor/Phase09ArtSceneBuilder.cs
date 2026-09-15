using System;
using System.Collections.Generic;
using System.IO;
using GroundChickenKing.Chickens;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GroundChickenKing.Editor
{
    public static class Phase09ArtSceneBuilder
    {
        private const string ScenePath = "Assets/_Game/Scenes/SCN_Main.unity";
        private const string BackgroundPath = "Assets/_Game/Art/Environment/Generated/BG_TabletopFarmFestival.png";
        private const string ChickenAtlasPath = "Assets/_Game/Art/Characters/Generated/SPR_ChickenRoster_Atlas.png";
        private const string ButtonPath = "Assets/_Game/Art/UI/Generated/SPR_UI_WoodButton.png";
        private const string PanelPath = "Assets/_Game/Art/UI/Generated/SPR_UI_ParchmentPanel.png";
        private const string IconAtlasPath = "Assets/_Game/Art/UI/Generated/SPR_UI_IconAtlas.png";

        private static readonly Color Cream = new(1f, 0.94f, 0.76f, 1f);
        private static readonly Color Gold = new(1f, 0.74f, 0.18f, 1f);
        private static readonly Color Ink = new(0.16f, 0.075f, 0.035f, 1f);
        private static readonly Color Teal = new(0.43f, 0.84f, 0.78f, 1f);
        private static readonly Color Tomato = new(0.96f, 0.48f, 0.36f, 1f);

        [MenuItem("Ground Chicken King/Configure Phase 09 Art")]
        public static void ConfigurePhase09()
        {
            RequireFile(BackgroundPath);
            RequireFile(ChickenAtlasPath);
            RequireFile(ButtonPath);
            RequireFile(PanelPath);
            RequireFile(IconAtlasPath);

            var background = ConfigureSprite(BackgroundPath, Vector4.zero, false);
            var buttonSprite = ConfigureSprite(ButtonPath, new Vector4(190f, 150f, 190f, 150f), true);
            var panelSprite = ConfigureSprite(PanelPath, new Vector4(175f, 150f, 175f, 150f), true);
            var chickenAtlas = ConfigureAtlas(ChickenAtlasPath);
            var iconAtlas = ConfigureAtlas(IconAtlasPath);

            ConfigureChickenPrefabs(chickenAtlas);
            ConfigureAnimationClips();

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var safeArea = Find("UI_Panel_SafeArea")?.GetComponent<RectTransform>();
            if (safeArea == null)
                throw new InvalidOperationException("Phase 09 requires the completed main scene.");

            DestroyIfPresent("UI_Image_GeneratedBackground");
            var backgroundImage = CreateImage("UI_Image_GeneratedBackground", safeArea, background);
            Stretch(backgroundImage.rectTransform);
            backgroundImage.color = Color.white;
            backgroundImage.preserveAspect = false;
            backgroundImage.transform.SetAsFirstSibling();

            DecorateScreens(panelSprite);
            DecorateMainMenu(chickenAtlas, iconAtlas);
            DecorateRaceTrack(panelSprite, iconAtlas);
            DecorateBalanceLabels(iconAtlas);
            StyleButtons(buttonSprite);
            StyleText();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Art] Phase 09 generated art, UI skin, chicken atlas, lane dressing, and animation curves configured.");
        }

        public static void ConfigurePhase09FromCommandLine() => ConfigurePhase09();

        private static void ConfigureChickenPrefabs(Texture2D atlas)
        {
            for (var i = 0; i < 5; i++)
            {
                var path = $"Assets/_Game/Prefabs/Chickens/PF_Chicken_{i + 1}.prefab";
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var visual = root.transform.Find("Visual");
                    if (visual == null)
                        throw new InvalidOperationException($"Chicken prefab is missing Visual: {path}");

                    foreach (var oldName in new[] { "Body", "Head", "Beak", "Comb", "Eye", "Artwork" })
                    {
                        var old = visual.Find(oldName);
                        if (old != null)
                            UnityEngine.Object.DestroyImmediate(old.gameObject);
                    }

                    var artworkObject = new GameObject("Artwork", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
                    artworkObject.transform.SetParent(visual, false);
                    artworkObject.transform.SetAsFirstSibling();
                    var rect = artworkObject.GetComponent<RectTransform>();
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = new Vector2(0f, 6f);
                    rect.sizeDelta = new Vector2(128f, 142f);
                    var image = artworkObject.GetComponent<RawImage>();
                    image.texture = atlas;
                    image.uvRect = new Rect(i * 0.2f, 0f, 0.2f, 1f);
                    image.color = Color.white;
                    image.raycastTarget = false;

                    var name = visual.Find("Name")?.GetComponent<Text>();
                    if (name != null)
                    {
                        name.gameObject.SetActive(false);
                    }

                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        private static void ConfigureAnimationClips()
        {
            SetClip("Idle", 1.2f, Curve("m_AnchoredPosition.y", 0f, 2f, 0f), Curve("m_LocalScale.y", 1f, 1.025f, 1f));
            SetClip("Warmup", 0.7f, Curve("localEulerAnglesRaw.z", -5f, 5f, -5f), Curve("m_LocalScale.x", 1f, 1.05f, 1f));
            SetClip("Run", 0.32f, Curve("m_AnchoredPosition.y", 0f, 7f, 0f), Curve("localEulerAnglesRaw.z", -3f, 3f, -3f));
            SetClip("Sprint", 0.22f, Curve("m_AnchoredPosition.y", 0f, 5f, 0f), Curve("m_LocalScale.x", 1.08f, 0.94f, 1.08f), Curve("m_LocalScale.y", 0.94f, 1.08f, 0.94f));
            SetClip("Stop", 0.7f, Curve("localEulerAnglesRaw.z", 4f, -4f, 4f));
            SetClip("Fall", 0.55f, Curve("localEulerAnglesRaw.z", 0f, -76f, -72f), Curve("m_AnchoredPosition.y", 0f, -15f, -15f));
            SetClip("Recover", 0.55f, Curve("localEulerAnglesRaw.z", -72f, -20f, 0f), Curve("m_AnchoredPosition.y", -15f, 4f, 0f));
            SetClip("Turn", 0.7f, Curve("m_LocalScale.x", 1f, 0.08f, -1f));
            SetClip("Interfere", 0.5f, Curve("localEulerAnglesRaw.z", -9f, 13f, -9f), Curve("m_AnchoredPosition.x", 0f, 10f, 0f));
            SetClip("Celebrate", 0.55f, Curve("m_AnchoredPosition.y", 0f, 20f, 0f), Curve("m_LocalScale.x", 1f, 1.13f, 1f), Curve("m_LocalScale.y", 1f, 0.9f, 1f));
            SetClip("Lose", 1.1f, Curve("localEulerAnglesRaw.z", 0f, 9f, 0f), Curve("m_AnchoredPosition.y", 0f, -5f, 0f));
        }

        private static KeyValuePair<string, AnimationCurve> Curve(string property, float start, float middle, float end)
        {
            return new KeyValuePair<string, AnimationCurve>(property, AnimationCurve.EaseInOut(0f, start, 0.5f, middle).Append(1f, end));
        }

        private static void SetClip(string stateName, float duration, params KeyValuePair<string, AnimationCurve>[] curves)
        {
            var path = $"Assets/_Game/Animations/Chickens/AN_Chicken_{stateName}.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
                throw new InvalidOperationException($"Animation clip is missing: {path}");
            AnimationUtility.SetAnimationClipSettings(clip, new AnimationClipSettings { loopTime = stateName is not "Fall" and not "Recover" });
            clip.frameRate = 30f;
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                AnimationUtility.SetEditorCurve(clip, binding, null);
            foreach (var curve in curves)
            {
                var scaled = new AnimationCurve();
                foreach (var key in curve.Value.keys)
                    scaled.AddKey(new Keyframe(key.time * duration, key.value, key.inTangent, key.outTangent));
                var binding = EditorCurveBinding.FloatCurve("Artwork", typeof(RectTransform), curve.Key);
                AnimationUtility.SetEditorCurve(clip, binding, scaled);
            }
            EditorUtility.SetDirty(clip);
        }

        private static void DecorateScreens(Sprite panelSprite)
        {
            var screenBoards = new Dictionary<string, Vector2>
            {
                ["UI_Panel_MainMenu"] = new Vector2(940f, 850f),
                ["UI_Panel_PlayerJoin"] = new Vector2(1780f, 860f),
                ["UI_Panel_Warmup"] = new Vector2(1420f, 700f),
                ["UI_Panel_Settlement"] = new Vector2(1420f, 760f),
                ["UI_Panel_GameOver"] = new Vector2(1420f, 760f),
                ["UI_Panel_Settings"] = new Vector2(1320f, 780f),
                ["UI_Panel_Diagnostics"] = new Vector2(1580f, 820f),
                ["UI_Panel_Confirmation"] = new Vector2(1220f, 590f),
            };

            foreach (var item in screenBoards)
            {
                var panel = Find(item.Key)?.transform;
                if (panel == null)
                    continue;
                DestroyChild(panel, "UI_Image_ArtFrame");
                var frame = CreateImage("UI_Image_ArtFrame", panel, panelSprite);
                frame.type = Image.Type.Sliced;
                frame.rectTransform.anchorMin = frame.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                frame.rectTransform.sizeDelta = item.Value;
                frame.color = Color.white;
                frame.transform.SetAsFirstSibling();
            }

            foreach (var panel in UnityEngine.Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var isPlayerSeat = panel.name.Length == "UI_Panel_Player1".Length &&
                                   panel.name.StartsWith("UI_Panel_Player", StringComparison.Ordinal) &&
                                   char.IsDigit(panel.name[^1]);
                if (!isPlayerSeat && !panel.name.StartsWith("UI_Panel_Betting_Player", StringComparison.Ordinal))
                    continue;
                panel.sprite = panelSprite;
                panel.type = Image.Type.Sliced;
                panel.color = Color.Lerp(panel.color, Color.white, 0.42f);
            }
        }

        private static void DecorateMainMenu(Texture2D chickenAtlas, Texture2D iconAtlas)
        {
            var menu = Find("UI_Panel_MainMenu")?.transform;
            if (menu == null)
                return;
            DestroyChild(menu, "UI_Image_MainChickenLeft");
            DestroyChild(menu, "UI_Image_MainChickenRight");
            DestroyChild(menu, "UI_Icon_TitleCrown");
            var left = CreateAtlasImage("UI_Image_MainChickenLeft", menu, chickenAtlas, new Rect(0f, 0f, 0.2f, 1f), new Vector2(-620f, -20f), new Vector2(245f, 390f));
            left.transform.SetSiblingIndex(1);
            var right = CreateAtlasImage("UI_Image_MainChickenRight", menu, chickenAtlas, new Rect(0.6f, 0f, 0.2f, 1f), new Vector2(620f, -20f), new Vector2(245f, 390f));
            right.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
            right.transform.SetSiblingIndex(2);
            CreateAtlasImage("UI_Icon_TitleCrown", menu, iconAtlas, new Rect(0.25f, 0f, 0.25f, 1f), new Vector2(0f, 345f), new Vector2(105f, 105f));

            var gearButton = Find("UI_Button_Settings")?.transform;
            if (gearButton != null)
            {
                DestroyChild(gearButton, "UI_Icon_Gear");
                CreateAtlasImage("UI_Icon_Gear", gearButton, iconAtlas, new Rect(0.75f, 0f, 0.25f, 1f), new Vector2(-155f, 0f), new Vector2(58f, 58f));
            }
        }

        private static void DecorateRaceTrack(Sprite panelSprite, Texture2D iconAtlas)
        {
            var track = Find("UI_Panel_RaceTrack")?.GetComponent<Image>();
            if (track == null)
                return;
            Move("UI_Text_RaceCountdownTitle", new Vector2(0f, 340f));
            Move("UI_Text_RaceCountdownSummary", new Vector2(0f, 270f));
            track.sprite = panelSprite;
            track.type = Image.Type.Sliced;
            track.color = Color.white;
            var trackTransform = track.transform;
            DestroyChild(trackTransform, "UI_Icon_FinishFlag");
            CreateAtlasImage("UI_Icon_FinishFlag", trackTransform, iconAtlas, new Rect(0.5f, 0f, 0.25f, 1f), new Vector2(770f, 0f), new Vector2(88f, 88f));

            for (var lane = 0; lane < 5; lane++)
            {
                var laneImage = Find($"UI_Image_Lane_{lane + 1}")?.GetComponent<Image>();
                if (laneImage != null)
                    laneImage.color = lane % 2 == 0 ? new Color(0.28f, 0.46f, 0.25f, 0.88f) : new Color(0.50f, 0.30f, 0.14f, 0.88f);
                var finish = Find($"UI_Image_Finish_{lane + 1}")?.GetComponent<Image>();
                if (finish != null)
                    finish.color = lane % 2 == 0 ? Cream : Ink;
            }
        }

        private static void DecorateBalanceLabels(Texture2D iconAtlas)
        {
            foreach (var text in UnityEngine.Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (text.name is not "UI_Text_Balance" and not "UI_Text_PlayerCoins")
                    continue;
                var icon = CreateAtlasImage("UI_Icon_Coin", text.transform.parent, iconAtlas, new Rect(0f, 0f, 0.25f, 1f), text.rectTransform.anchoredPosition + new Vector2(-text.rectTransform.sizeDelta.x * 0.44f, 0f), new Vector2(36f, 36f));
                icon.transform.SetSiblingIndex(text.transform.GetSiblingIndex());
            }
        }

        private static void StyleButtons(Sprite buttonSprite)
        {
            foreach (var button in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var image = button.GetComponent<Image>();
                if (image == null)
                    continue;
                image.sprite = buttonSprite;
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = 1f;
                image.color = ButtonTint(button.name);
                var colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = Color.white;
                colors.selectedColor = Color.white;
                colors.pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
                colors.disabledColor = new Color(0.34f, 0.34f, 0.34f, 0.78f);
                colors.colorMultiplier = 1f;
                colors.fadeDuration = 0.08f;
                button.colors = colors;
                button.transition = Selectable.Transition.ColorTint;
            }
        }

        private static Color ButtonTint(string name)
        {
            if (name.Contains("Exit", StringComparison.Ordinal) || name.Contains("Cancel", StringComparison.Ordinal) || name.Contains("Back", StringComparison.Ordinal))
                return Tomato;
            if (name.Contains("Stake", StringComparison.Ordinal) || name.Contains("Chicken", StringComparison.Ordinal) || name.Contains("Diagnostics", StringComparison.Ordinal))
                return Teal;
            return Color.white;
        }

        private static void StyleText()
        {
            foreach (var text in UnityEngine.Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                text.color = text.fontSize >= 48 ? Gold : Cream;
                text.fontStyle = text.fontSize >= 28 ? FontStyle.Bold : FontStyle.Normal;
                AddOutline(text.gameObject, text.fontSize >= 48 ? 3f : 1.5f);
            }
        }

        private static void AddOutline(GameObject target, float distance)
        {
            var outline = target.GetComponent<Outline>() ?? target.AddComponent<Outline>();
            outline.effectColor = new Color(Ink.r, Ink.g, Ink.b, 0.92f);
            outline.effectDistance = new Vector2(distance, -distance);
            outline.useGraphicAlpha = true;
        }

        private static Sprite ConfigureSprite(string path, Vector4 border, bool alpha)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                throw new InvalidOperationException($"Texture importer unavailable: {path}");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.spriteBorder = border;
            var textureSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(textureSettings);
            textureSettings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(textureSettings);
            importer.alphaIsTransparency = alpha;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Texture2D ConfigureAtlas(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                throw new InvalidOperationException($"Texture importer unavailable: {path}");
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        private static RawImage CreateAtlasImage(string name, Transform parent, Texture texture, Rect uv, Vector2 position, Vector2 size)
        {
            var existing = parent.Find(name);
            if (existing != null)
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = go.GetComponent<RawImage>();
            image.texture = texture;
            image.uvRect = uv;
            image.raycastTarget = false;
            return image;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Move(string name, Vector2 position)
        {
            var rect = Find(name)?.GetComponent<RectTransform>();
            if (rect != null)
                rect.anchoredPosition = position;
        }

        private static void RequireFile(string assetPath)
        {
            if (!File.Exists(assetPath))
                throw new FileNotFoundException("Generated art asset is missing.", assetPath);
        }

        private static void DestroyIfPresent(string name)
        {
            var target = Find(name);
            if (target != null)
                UnityEngine.Object.DestroyImmediate(target);
        }

        private static void DestroyChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
                UnityEngine.Object.DestroyImmediate(child.gameObject);
        }

        private static GameObject Find(string name)
        {
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                    if (transform.name == name)
                        return transform.gameObject;
            return null;
        }
    }

    internal static class AnimationCurveExtensions
    {
        public static AnimationCurve Append(this AnimationCurve curve, float time, float value)
        {
            curve.AddKey(time, value);
            return curve;
        }
    }
}
