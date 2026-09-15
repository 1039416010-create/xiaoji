using System;
using System.Collections;
using Unity.Profiling;
using UnityEngine;

namespace GroundChickenKing.Diagnostics
{
    public sealed class DevelopmentPerformanceProbe : MonoBehaviour
    {
        private const string ProbeFlag = "--performance-probe";

        private IEnumerator Start()
        {
            if (!Debug.isDebugBuild || Array.IndexOf(Environment.GetCommandLineArgs(), ProbeFlag) < 0)
                yield break;

            yield return new WaitForSecondsRealtime(4.2f);
            using var mainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 300);
            using var gcAllocated = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 300);
            for (var frame = 0; frame < 300; frame++)
                yield return null;

            var mainSamples = mainThread.ToArray();
            long totalNanoseconds = 0;
            long maximumNanoseconds = 0;
            foreach (var sample in mainSamples) { totalNanoseconds += sample.Value; if (sample.Value > maximumNanoseconds) maximumNanoseconds = sample.Value; }
            var gcSamples = gcAllocated.ToArray();
            long totalGcBytes = 0;
            long maximumGcBytes = 0;
            foreach (var sample in gcSamples) { totalGcBytes += sample.Value; if (sample.Value > maximumGcBytes) maximumGcBytes = sample.Value; }
            var averageMilliseconds = mainSamples.Length == 0 ? 0d : totalNanoseconds / 1_000_000d / mainSamples.Length;
            GameLog.Info("Performance", $"samples={mainSamples.Length} main.avgMs={averageMilliseconds:0.000} main.maxMs={maximumNanoseconds / 1_000_000d:0.000} gc.totalBytes={totalGcBytes} gc.maxFrameBytes={maximumGcBytes}");
            Application.Quit();
        }
    }
}
