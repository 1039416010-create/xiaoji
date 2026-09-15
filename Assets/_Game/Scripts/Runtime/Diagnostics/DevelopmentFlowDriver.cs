using System;
using UnityEngine;
using UnityEngine.UI;

namespace GroundChickenKing.Diagnostics
{
    public sealed class DevelopmentFlowDriver : MonoBehaviour
    {
        private const string VerificationStatePrefix = "--verification-state=";

        private void Start()
        {
            if (!Debug.isDebugBuild)
                return;

            var requestedState = string.Empty;
            foreach (var argument in Environment.GetCommandLineArgs())
            {
                if (argument.StartsWith(VerificationStatePrefix, StringComparison.OrdinalIgnoreCase))
                    requestedState = argument.Substring(VerificationStatePrefix.Length);
            }

            if (string.Equals(requestedState, "playerjoin", StringComparison.OrdinalIgnoreCase))
            {
                Click("UI_Button_StartGame");
                return;
            }

            if (string.Equals(requestedState, "warmup", StringComparison.OrdinalIgnoreCase))
            {
                Click("UI_Button_StartGame");
                Click("UI_Button_Join_Player1");
                Click("UI_Button_Join_Player2");
                Click("UI_Button_ContinueToWarmup");
                return;
            }

            if (string.Equals(requestedState, "betting", StringComparison.OrdinalIgnoreCase))
            {
                NavigateToBetting();
                return;
            }

            if (string.Equals(requestedState, "locked", StringComparison.OrdinalIgnoreCase))
            {
                NavigateToBetting();
                LockExampleBet("Player1", 0, 10);
                LockExampleBet("Player2", 1, 5);
                return;
            }

            if (string.Equals(requestedState, "settings", StringComparison.OrdinalIgnoreCase)) Click("UI_Button_Settings");
            else if (string.Equals(requestedState, "diagnostics", StringComparison.OrdinalIgnoreCase)) Click("UI_Button_Diagnostics");
            else if (string.Equals(requestedState, "confirmation", StringComparison.OrdinalIgnoreCase))
            {
                Click("UI_Button_StartGame"); Click("UI_Button_Join_Player1"); Click("UI_Button_JoinBack");
            }
            else if (string.Equals(requestedState, "calibration", StringComparison.OrdinalIgnoreCase))
            {
                Click("UI_Button_Diagnostics"); Click("UI_Button_TouchCalibration");
            }
        }

        private static void NavigateToBetting()
        {
            Click("UI_Button_StartGame");
            Click("UI_Button_Join_Player1");
            Click("UI_Button_Join_Player2");
            Click("UI_Button_ContinueToWarmup");
            Click("UI_Button_EnterBetting");
        }

        private static void LockExampleBet(string playerName, int chickenIndex, int stake)
        {
            Click($"UI_Button_BetChicken_{playerName}_{chickenIndex}");
            Click($"UI_Button_BetStake_{playerName}_{stake}");
            Click($"UI_Button_LockBet_{playerName}");
        }

        private static void Click(string objectName)
        {
            var button = GameObject.Find(objectName)?.GetComponent<Button>();
            if (button == null)
                throw new InvalidOperationException($"Verification button not found: {objectName}.");
            button.onClick.Invoke();
        }
    }
}
