using System;
using System.IO;
using UnityEngine;
using GroundChickenKing.UI;

namespace GroundChickenKing.Diagnostics
{
    public sealed class BoundedLogWriter : MonoBehaviour
    {
        private const long MaximumBytes = 1024 * 1024;
        private readonly object _gate = new();
        private string _path;
        private bool _isAvailable;
        public static string MostRecentError { get; private set; } = UiTextCatalog.NoRecentError;
        public static string LogPath { get; private set; }

        private void Awake()
        {
            _path = Path.Combine(Application.persistentDataPath, "ExhibitLogs", "GroundChickenKing.log"); LogPath = _path;
            try { Directory.CreateDirectory(Path.GetDirectoryName(_path)); RotateIfNeeded(); _isAvailable = true; }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException) { MostRecentError = "日志不可写，游戏继续运行"; }
            Application.logMessageReceivedThreaded += HandleLog;
        }

        private void OnDestroy() => Application.logMessageReceivedThreaded -= HandleLog;

        private void HandleLog(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) MostRecentError = condition;
            if (!_isAvailable) return;
            var line = $"{DateTime.UtcNow:O} [{type}] {condition}{Environment.NewLine}";
            lock (_gate)
            {
                try { RotateIfNeeded(); File.AppendAllText(_path, line); }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException) { _isAvailable = false; MostRecentError = "日志写入已停用，游戏继续运行"; }
            }
        }

        private void RotateIfNeeded()
        {
            if (!File.Exists(_path) || new FileInfo(_path).Length < MaximumBytes) return;
            var backup = _path + ".1"; if (File.Exists(backup)) File.Delete(backup); File.Move(_path, backup);
        }
    }
}
