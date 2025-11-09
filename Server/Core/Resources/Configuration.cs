using CK3Analyser.Core.Resources.DetectorSettings;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CK3Analyser.Core.Resources
{
    public enum Severity
    {
        Debug, Info, Warning, Critical
    }

    public class Configuration
    {
        private JsonElement rawSettings { get; }

        public Configuration(JsonElement rawSettings)
        {
            this.rawSettings = rawSettings;
        }

        public LargeUnitSettings LargeUnitSettings
        {
            get
            {
                if (rawSettings.TryGetProperty("largeUnit", out var largeUnitSettings))
                {
                    var flattened = Pretransform(largeUnitSettings);
                    var flat = flattened.ToString();
                    return JsonSerializer.Deserialize<LargeUnitSettings>(flat);
                }
                return new LargeUnitSettings
                {
                    Enabled = false
                };
            }
        }

        private static JsonElement Pretransform(JsonElement element)
        {
            var dict = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            PretransformElement(element, dict);

            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(dict));
            return doc.RootElement.Clone(); // Clone so it survives beyond the using block
        }

        private static void PretransformElement(JsonElement element, IDictionary<string, JsonElement> dict, string prefix = null)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        var propertyName = ToTitleCase(property.Name);
                        var name = prefix is null ? propertyName : $"{prefix}_{propertyName}";
                        PretransformElement(property.Value, dict, name);
                    }
                    break;

                default:
                    dict[prefix ?? string.Empty] = element;
                    break;
            }
        }

        private static string ToTitleCase(string name)
        {
            return name.Substring(0, 1).ToUpperInvariant() + name.Substring(1, name.Length - 1);
        }
    }
}
