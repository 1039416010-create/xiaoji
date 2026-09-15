using System;
using GroundChickenKing.Core;
using GroundChickenKing.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GroundChickenKing.Editor
{
    public static class Phase04SceneBuilder
    {
        private const string MainScenePath = "Assets/_Game/Scenes/SCN_Main.unity";
        private const string RaceConfigPath = "Assets/_Game/Config/Race/CFG_Race_Default.asset";

        [MenuItem("Ground Chicken King/Configure Phase 04")]
        public static void ConfigurePhase04()
        {
            var scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            var presenter = GameObject.Find("GameSession")?.GetComponent<GameSessionPresenter>();
            var raceConfig = AssetDatabase.LoadAssetAtPath<RaceConfig>(RaceConfigPath);
            var title = FindIncludingInactive("UI_Text_RaceCountdownTitle")?.GetComponent<Text>();
            var summary = FindIncludingInactive("UI_Text_RaceCountdownSummary")?.GetComponent<Text>();
            if (presenter == null || raceConfig == null || title == null || summary == null)
                throw new InvalidOperationException("Phase 03 scene or RaceConfig is missing.");

            presenter.ConfigureRace(raceConfig);
            title.text = "受控随机比赛";
            summary.text = "全员锁定后生成比赛剧本";
            EditorUtility.SetDirty(presenter);
            EditorUtility.SetDirty(title);
            EditorUtility.SetDirty(summary);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MainScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[Bootstrap] Phase 04 deterministic race configuration completed successfully.");
        }

        public static void ConfigurePhase04FromCommandLine() => ConfigurePhase04();

        private static GameObject FindIncludingInactive(string objectName)
        {
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                if (transform.name == objectName) return transform.gameObject;
            return null;
        }
    }
}
