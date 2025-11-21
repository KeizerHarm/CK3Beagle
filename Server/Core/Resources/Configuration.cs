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

    public enum VanillaFileHandling
    {
        TreatAsRegularFiles,
        IgnoreEntirely,
        AnalyseModsAdditions
    }

    public class Configuration
    {
        private readonly JsonElement _rawSettings;
        public bool ReadVanilla { get; set; }
        public VanillaFileHandling VanillaFileHandling { get; set; }

        public Configuration(JsonElement rawSettings)
        {
            _rawSettings = rawSettings;
            HandleGlobalSettings();
        }

        private void HandleGlobalSettings()
        {
            ReadVanilla = false;
            if (_rawSettings.ValueKind != JsonValueKind.Undefined && _rawSettings.TryGetProperty("readVanilla", out var readVanilla))
            {
                ReadVanilla = readVanilla.GetBoolean();
            }

            VanillaFileHandling = VanillaFileHandling.TreatAsRegularFiles;
            if (_rawSettings.ValueKind != JsonValueKind.Undefined && _rawSettings.TryGetProperty("vanillaFileHandling", out var vanillaFileHandling))
            {
                VanillaFileHandling = (VanillaFileHandling)vanillaFileHandling.GetInt32();
                if (VanillaFileHandling == VanillaFileHandling.AnalyseModsAdditions)
                    ReadVanilla = true;
            }
        }

        public Configuration(bool useDefault = false)
        {
            _rawSettings = new JsonElement();
            if (useDefault)
            {
                HandleGlobalSettings();
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

                OvercomplicatedTriggerSettings =
                    new OvercomplicatedTriggerSettings
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

                MagicNumberSettings =
                    new MagicNumberSettings
                    {
                        Enabled = true,
                        Severity = Severity.Info,
                        KeysToConsider = 
                            ["reserved_gold",
                            "short_term_gold",
                            "war_chest_gold",
                            "add_gold",
                            "add_long_term_gold",
                            "add_reserved_gold",
                            "add_short_term_gold",
                            "add_treasury_or_gold",
                            "add_war_chest_gold",
                            "remove_long_term_gold",
                            "remove_reserved_gold",
                            "remove_short_term_gold",
                            "remove_treasury_or_gold",
                            "remove_war_chest_gold",
                            "invest_gold",
                            "prestige",
                            "add_prestige",
                            "add_prestige_and_experience",
                            "add_prestige_experience",
                            "add_prestige_no_experience",
                            "dynasty_prestige",
                            "add_dynasty_prestige",
                            "piety",
                            "add_piety",
                            "add_piety_and_experience",
                            "add_piety_experience",
                            "add_piety_no_experience",
                            "stress",
                            "add_stress",
                            "dread",
                            "add_dread",
                            "tyranny",
                            "add_tyranny",
                            "influence",
                            "change_influence",
                            "change_influence_and_experience",
                            "change_influence_experience",
                            "change_influence_no_experience",
                            "provisions",
                            "change_provisions",
                            "herd",
                            "change_herd",
                            "barter_goods",
                            "add_barter_goods",
                            "remove_barter_goods",
                            "merit",
                            "change_merit",
                            "change_merit_and_experience",
                            "change_merit_experience",
                            "change_merit_no_experience",
                            "treasury",
                            "long_term_treasury",
                            "long_term_treasury_or_gold",
                            "reserved_treasury",
                            "reserved_treasury_or_gold",
                            "short_term_treasury",
                            "short_term_treasury_or_gold",
                            "war_chest_treasury",
                            "war_chest_treasury_or_gold",
                            "add_treasury",
                            "add_treasury_or_gold",
                            "remove_treasury",
                            "remove_treasury_or_gold",
                            "remove_long_term_treasury",
                            "remove_reserved_treasury",
                            "remove_short_term_treasury",
                            "remove_war_chest_treasury"]
                    };

                KeywordAsScopeNameSettings =
                    new KeywordAsScopeNameSettings
                    {
                        Enabled = true,
                        RootOrPrev_Severity = Severity.Warning,
                        ScopeLink_Severity = Severity.Warning,
                        ScopeType_Severity = Severity.Warning,
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

        private OvercomplicatedTriggerSettings? _overcomplicatedTriggerSettings;
        public OvercomplicatedTriggerSettings OvercomplicatedTriggerSettings
        {
            get
            {
                _overcomplicatedTriggerSettings = GetSettings(_overcomplicatedTriggerSettings, "overcomplicatedTrigger");
                return _overcomplicatedTriggerSettings.Value;
            }
            set
            {
                _overcomplicatedTriggerSettings = value;
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

        private MagicNumberSettings? _magicNumberSettings;
        public MagicNumberSettings MagicNumberSettings
        {
            get
            {
                _magicNumberSettings = GetSettings(_magicNumberSettings, "magicNumber");
                return _magicNumberSettings.Value;
            }
            set
            {
                _magicNumberSettings = value;
            }
        }

        private KeywordAsScopeNameSettings? _keywordAsScopeNameSettings;
        public KeywordAsScopeNameSettings KeywordAsScopeNameSettings
        {
            get
            {
                _keywordAsScopeNameSettings = GetSettings(_keywordAsScopeNameSettings, "keywordAsScopeName");
                return _keywordAsScopeNameSettings.Value;
            }
            set
            {
                _keywordAsScopeNameSettings = value;
            }
        }

        public override string ToString()
        {
            return "Configuration: {"
                + $"[ {nameof(LargeUnitSettings)}: {LargeUnitSettings.ToString()} ], "
                + $"[ {nameof(OvercomplicatedTriggerSettings)}: {OvercomplicatedTriggerSettings.ToString()} ], "
                + $"[ {nameof(DuplicationSettings)}: {DuplicationSettings.ToString()} ], "
                + $"[ {nameof(HiddenDependenciesSettings)}: {HiddenDependenciesSettings.ToString()} ], "
                + $"[ {nameof(InconsistentIndentationSettings)}: {InconsistentIndentationSettings.ToString()} ], "
                + $"[ {nameof(MagicNumberSettings)}: {MagicNumberSettings.ToString()} ], "
                + $"[ {nameof(KeywordAsScopeNameSettings)}: {KeywordAsScopeNameSettings.ToString()} ], "
                + $"[ {nameof(ReadVanilla)}: {ReadVanilla.ToString()} ], "
                + $"[ {nameof(VanillaFileHandling)}: {VanillaFileHandling.ToString()} ]"
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
