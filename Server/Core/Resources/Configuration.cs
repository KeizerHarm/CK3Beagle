using Antlr4.Runtime.Dfa;
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
        private readonly JsonElement _rawSettings;

        public Configuration(JsonElement rawSettings)
        {
            _rawSettings = rawSettings;
        }

        public Configuration(bool useDefault)
        {
            _rawSettings = new JsonElement();
            if (useDefault)
            {
                LargeUnitSettings =
                    new LargeUnitSettings
                    {
                        File_Severity = Severity.Info,
                        File_MaxSize = 10000,
                        Macro_Severity = Severity.Info,
                        Macro_MaxSize = 50,
                        NonMacroBlock_Severity = Severity.Info,
                        NonMacroBlock_MaxSize = 50
                    };

                OvercomplicatedBooleanSettings =
                    new OvercomplicatedBooleanSettings
                    {
                        Absorption_Severity = Severity.Info,
                        Associativity_Severity = Severity.Warning,
                        Complementation_Severity = Severity.Critical,
                        Distributivity_Severity = Severity.Info,
                        DoubleNegation_Severity = Severity.Warning,
                        Idempotency_Severity = Severity.Warning,
                        NotIsNotNor_Severity = Severity.Warning
                    };

                InconsistentIndentationSettings =
                    new InconsistentIndentationSettings
                    {
                        Enabled = true,
                        AbberatingLines_Severity = Severity.Warning,
                        UnexpectedType_Severity = Severity.Warning,
                        CommentHandling = CommentHandling.NoSpecialTreatment,
                        AllowedIndentationTypes = [IndentationType.Tab, IndentationType.FourSpaces]
                    };

                DuplicationSettings =
                    new DuplicationSettings
                    {
                        Severity = Severity.Warning,
                        MinSize = 5
                    };

                HiddenDependenciesSettings =
                    new HiddenDependenciesSettings
                    {
                        Enabled = true,
                        UseOfPrev_Severity = Severity.Warning,
                        UseOfRoot_Severity = Severity.Warning,
                        UseOfSavedScope_Severity = Severity.Warning,
                        UseOfVariable_Severity = Severity.Warning,
                        UseOfPrev_AllowedIfInComment = true,
                        UseOfPrev_AllowedIfInName = true,
                        UseOfPrev_AllowedIfInEventFile = false,
                        UseOfRoot_AllowedIfInComment = true,
                        UseOfRoot_AllowedIfInName = true,
                        UseOfRoot_AllowedIfInEventFile = false,
                        UseOfSavedScope_AllowedIfInComment = true,
                        UseOfSavedScope_AllowedIfInName = false,
                        UseOfSavedScope_AllowedIfInEventFile = false,
                        UseOfSavedScope_Whitelist = [],
                        UseOfVariable_AllowedIfInComment = true,
                        UseOfVariable_AllowedIfInName = false,
                        UseOfVariable_AllowedIfInEventFile = false,
                        UseOfVariable_Whitelist = []
                    };
            }
        }

        private LargeUnitSettings? _largeUnitSettings;
        public LargeUnitSettings LargeUnitSettings
        {
            get
            {
                _largeUnitSettings = GetSettings(_largeUnitSettings, "largeUnit");
                return _largeUnitSettings.Value;
            }
            set
            {
                _largeUnitSettings = value;
            }
        }

        private OvercomplicatedBooleanSettings? _overcomplicatedBooleanSettings;
        public OvercomplicatedBooleanSettings OvercomplicatedBooleanSettings
        {
            get
            {
                _overcomplicatedBooleanSettings = GetSettings(_overcomplicatedBooleanSettings, "overcomplicatedBoolean");
                return _overcomplicatedBooleanSettings.Value;
            }
            set
            {
                _overcomplicatedBooleanSettings = value;
            }
        }

        private DuplicationSettings? _duplicationSettings;
        public DuplicationSettings DuplicationSettings
        {
            get
            {
                _duplicationSettings = GetSettings(_duplicationSettings, "duplication");
                return _duplicationSettings.Value;
            }
            set
            {
                _duplicationSettings = value;
            }
        }

        private HiddenDependenciesSettings? _hiddenDependenciesSettings;
        public HiddenDependenciesSettings HiddenDependenciesSettings
        {
            get
            {
                _hiddenDependenciesSettings = GetSettings(_hiddenDependenciesSettings, "hiddenDependencies");
                return _hiddenDependenciesSettings.Value;
            }
            set
            {
                _hiddenDependenciesSettings = value;
            }
        }

        private InconsistentIndentationSettings? _inconsistentIndentationSettings;
        public InconsistentIndentationSettings InconsistentIndentationSettings
        {
            get
            {
                _inconsistentIndentationSettings = GetSettings(_inconsistentIndentationSettings, "inconsistentIndentation");
                return _inconsistentIndentationSettings.Value;
            }
            set
            {
                _inconsistentIndentationSettings = value;
            }
        }

        public override string ToString()
        {
            return "Configuration: {"
                + $"{nameof(LargeUnitSettings)}: {LargeUnitSettings.ToString()} "
                + $"{nameof(OvercomplicatedBooleanSettings)}: {OvercomplicatedBooleanSettings.ToString()} "
                + $"{nameof(DuplicationSettings)}: {DuplicationSettings.ToString()} "
                + $"{nameof(HiddenDependenciesSettings)}: {HiddenDependenciesSettings.ToString()} "
                + $"{nameof(InconsistentIndentationSettings)}: {InconsistentIndentationSettings.ToString()} "
                + "}";
        }

        private T? GetSettings<T>(T? backingfield, string key)
            where T : struct, IGenericSettings
        {
            if (backingfield.HasValue)
                return backingfield;

            if (_rawSettings.TryGetProperty(key, out var settings))
            {
                var transformed = Pretransform(settings);
                backingfield = JsonSerializer.Deserialize<T>(transformed);
            }
            else
            {
                backingfield = new T
                {
                    Enabled = false
                };
            }
            return backingfield;
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
