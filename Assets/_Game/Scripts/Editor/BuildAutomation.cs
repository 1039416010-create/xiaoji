using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GroundChickenKing.Editor
{
    public static class BuildAutomation
    {
        public static void BuildWindowsDevelopmentFromCommandLine()
        {
            const string outputPath = "Builds/Windows/Development/GroundChickenKing.exe";
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
                throw new InvalidOperationException("No enabled scenes are configured for the Windows build.");

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development,
            });

            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException($"Windows Development Build failed: {report.summary.result}.");

            Debug.Log($"[Build] Windows Development Build succeeded: {outputPath}, {report.summary.totalSize} bytes.");
        }

        public static void BuildWindowsReleaseFromCommandLine()
        {
            const string outputPath = "Builds/Windows/Release/GroundChickenKing.exe";
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            if (scenes.Length == 0) throw new InvalidOperationException("No enabled scenes are configured for the Windows build.");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = scenes, locationPathName = outputPath, target = BuildTarget.StandaloneWindows64, options = BuildOptions.None });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException($"Windows Release Build failed: {report.summary.result}.");
            File.WriteAllText("Builds/Windows/Release/RELEASE_INFO.txt", $"走地鸡王 {Application.version}\nUnity {Application.unityVersion}\nWindows x64 Release\nBuilt UTC {DateTime.UtcNow:O}\nSize {report.summary.totalSize} bytes\n");
            Debug.Log($"[Build] Windows Release Build succeeded: {outputPath}, {report.summary.totalSize} bytes.");
        }
    }
}
