using UnityEngine;

namespace GroundChickenKing.Diagnostics
{
    public static class GameLog
    {
        public static void Info(string category, string message, string sessionId = "none", int round = 0)
        {
            Debug.Log(Format(category, message, sessionId, round));
        }

        public static void Warning(string category, string message, string sessionId = "none", int round = 0)
        {
            Debug.LogWarning(Format(category, message, sessionId, round));
        }

        public static void Error(string category, string message, string sessionId = "none", int round = 0)
        {
            Debug.LogError(Format(category, message, sessionId, round));
        }

        private static string Format(string category, string message, string sessionId, int round)
        {
            return $"[{category}] [session:{sessionId}] [round:{round}] [frame:{Time.frameCount}] {message}";
        }
    }
}
