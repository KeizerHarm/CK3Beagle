using CK3Analyser.Analysis.Detectors;
using System.Text.Json;

namespace CK3Analyser.Analysis
{
    public class AnalysisSettings
    {
        private JsonElement rawSettings { get; }

        public AnalysisSettings(JsonElement rawSettings)
        {
            this.rawSettings = rawSettings;
        }

        public LargeUnitDetector.Settings GetLargeUnitSettings()
        {
            if (rawSettings.TryGetProperty("large_unit", out var largeUnitSettings))
            {
                return JsonSerializer.Deserialize<LargeUnitDetector.Settings>(largeUnitSettings);
            }
            return new LargeUnitDetector.Settings
            {
                Enabled = false
            };
        }
    }
}
