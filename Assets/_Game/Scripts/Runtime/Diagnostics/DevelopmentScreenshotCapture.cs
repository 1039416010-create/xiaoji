using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace GroundChickenKing.Diagnostics
{
    public sealed class DevelopmentScreenshotCapture : MonoBehaviour
    {
        private const string CaptureFlag = "--capture-verification";
        private const string CapturePathPrefix = "--capture-path=";
        private const string CaptureDelayPrefix = "--capture-delay=";

        private IEnumerator Start()
        {
            if (!Debug.isDebugBuild)
                yield break;

            var arguments = Environment.GetCommandLineArgs();
            if (Array.IndexOf(arguments, CaptureFlag) < 0)
                yield break;

            var capturePath = Path.Combine(Application.persistentDataPath, "phase01-layout.png");
            var captureDelay = 0f;
            foreach (var argument in arguments)
            {
                if (argument.StartsWith(CapturePathPrefix, StringComparison.Ordinal))
                    capturePath = argument.Substring(CapturePathPrefix.Length);
                else if (argument.StartsWith(CaptureDelayPrefix, StringComparison.Ordinal) &&
                         float.TryParse(argument.Substring(CaptureDelayPrefix.Length), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsedDelay))
                    captureDelay = Mathf.Max(0f, parsedDelay);
            }

            var directory = Path.GetDirectoryName(capturePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            yield return null;
            if (captureDelay > 0f)
                yield return new WaitForSecondsRealtime(captureDelay);
            yield return new WaitForEndOfFrame();
            CaptureOffscreen(capturePath);
            Application.Quit();
        }

        private static void CaptureOffscreen(string capturePath)
        {
            var camera = Camera.main;
            var canvas = FindFirstObjectByType<Canvas>();
            if (camera == null || canvas == null)
                throw new InvalidOperationException("Visual verification requires a Main Camera and Canvas.");

            var width = Mathf.Max(Screen.width, 1);
            var height = Mathf.Max(Screen.height, 1);
            var renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var previousRenderMode = canvas.renderMode;
            var previousWorldCamera = canvas.worldCamera;

            try
            {
                camera.targetTexture = renderTexture;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1f;
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = renderTexture;
                texture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(capturePath, texture.EncodeToPNG());
                Debug.Log($"[VisualVerification] Captured {width}x{height} to {capturePath}.");
            }
            finally
            {
                camera.targetTexture = previousTarget;
                canvas.renderMode = previousRenderMode;
                canvas.worldCamera = previousWorldCamera;
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(renderTexture);
                Destroy(texture);
            }
        }
    }
}
