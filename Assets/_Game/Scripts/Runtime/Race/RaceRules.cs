using System;
using System.Globalization;
using GroundChickenKing.Core;

namespace GroundChickenKing.Race
{
    public sealed class RaceRules
    {
        public RaceRules(int laneCount, float startProgress, float finishProgress, float minimumDuration, float maximumDuration, float minimumEventInterval, float finishTolerance, int retryLimit, float hardTimeout)
        {
            LaneCount = laneCount; StartProgress = startProgress; FinishProgress = finishProgress; MinimumDuration = minimumDuration;
            MaximumDuration = maximumDuration; MinimumEventInterval = minimumEventInterval; FinishTolerance = finishTolerance;
            RetryLimit = retryLimit; HardTimeout = hardTimeout;
            if (laneCount != 5 || startProgress < 0f || finishProgress > 1f || startProgress >= finishProgress || minimumDuration <= 0f || maximumDuration < minimumDuration || retryLimit <= 0 || hardTimeout < maximumDuration)
                throw new ArgumentException("Race rules are invalid.");
            ConfigHash = StableHash(string.Join("|", laneCount, F(startProgress), F(finishProgress), F(minimumDuration), F(maximumDuration), F(minimumEventInterval), F(finishTolerance), retryLimit, F(hardTimeout)));
        }
        public int LaneCount { get; }
        public float StartProgress { get; }
        public float FinishProgress { get; }
        public float MinimumDuration { get; }
        public float MaximumDuration { get; }
        public float MinimumEventInterval { get; }
        public float FinishTolerance { get; }
        public int RetryLimit { get; }
        public float HardTimeout { get; }
        public string ConfigHash { get; }

        public static RaceRules FromConfig(RaceConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            return new RaceRules(config.LaneCount, config.StartProgress, config.FinishProgress, config.MinimumRaceDurationSeconds,
                config.MaximumRaceDurationSeconds, config.MinimumEventIntervalSeconds, config.FinishTolerance, config.GenerationRetryLimit, config.HardTimeoutSeconds);
        }

        private static string F(float value) => value.ToString("R", CultureInfo.InvariantCulture);
        internal static string StableHash(string value)
        {
            unchecked
            {
                uint hash = 2166136261;
                for (var i = 0; i < value.Length; i++) { hash ^= value[i]; hash *= 16777619; }
                return hash.ToString("x8", CultureInfo.InvariantCulture);
            }
        }
    }
}
