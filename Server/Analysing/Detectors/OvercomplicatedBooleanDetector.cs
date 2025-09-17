using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Analysis.Detectors
{
    public class OvercomplicatedBooleanDetector : BaseDetector
    {
        private static readonly IEnumerable<string> Operators =
        [
            "AND", "OR", "NOT", "NOR", "NAND"
        ];


        public struct Settings
        {
            public Severity Severity_DoubleNegation { get; set; }
            public Severity Severity_DoubleAndOr { get; set; }
            public Severity Severity_Distributivity { get; set; }
            public Severity Severity_RepeatedTriggers { get; set; }
            public Severity Severity_ComplementaryTriggers { get; set; }
            public Severity Severity_NotWithMultipleChildren { get; set; }

        }

        private Settings _settings;

        public OvercomplicatedBooleanDetector(Action<LogEntry> logFunc, Settings settings) : base(logFunc)
        {
            _settings = settings;
        }

        public override void AnalyseDeclaration(Declaration declaration)
        {
            if (declaration.EntityType != EntityType.ScriptedTrigger)
                return;

            AnalyseAsTriggerBlock(declaration, "AND");
        }

        public override void AnalyseNamedBlock(NamedBlock namedBlock)
        {
            var key = namedBlock.Key.ToUpper();

            if (!Operators.Contains(key))
                return;

            AnalyseAsTriggerBlock(namedBlock, key);
        }

        public void AnalyseAsTriggerBlock(NamedBlock namedBlock, string key) { 

            var namedBlockChildren = namedBlock.Children.OfType<NamedBlock>();
            var childBlockKeys = namedBlockChildren.Select(x => x.Key.ToUpper());
            var keyValuePairChildren = namedBlock.Children.OfType<Core.Domain.KeyValuePair>();
            var childKeyValuePairKeys = keyValuePairChildren.Select(x => x.Key.ToLower());

            if (key == "AND")
            {
                if (childBlockKeys.Contains("AND"))
                {
                    var entry = new LogEntry
                    {
                        Location = namedBlock.GetIdentifier(),
                        Message = "AND containing AND",
                        Severity = _settings.Severity_DoubleAndOr
                    };
                    LogFunc(entry);
                }

                var ORchildren = namedBlockChildren.Where(x => x.Key == "OR");
                if (HasRepeatedString(ORchildren.Select(x => x.Children.OfType<Core.Domain.KeyValuePair>().Select(y => y.Raw))))
                {
                    var entry = new LogEntry
                    {
                        Location = namedBlock.GetIdentifier(),
                        Message = "AND with ORs that share a trigger",
                        Severity = _settings.Severity_Distributivity
                    };
                    LogFunc(entry);
                }
            }

            if (key == "OR")
            {
                if (childBlockKeys.Contains("OR"))
                {
                    var entry = new LogEntry
                    {
                        Location = namedBlock.GetIdentifier(),
                        Message = "OR containing OR",
                        Severity = _settings.Severity_DoubleAndOr
                    };
                    LogFunc(entry);
                }

                var ANDchildren = namedBlockChildren.Where(x => x.Key == "AND");
                if (HasRepeatedString(ANDchildren.Select(x => x.Children.OfType<Core.Domain.KeyValuePair>().Select(y => y.Raw))))
                {
                    var entry = new LogEntry
                    {
                        Location = namedBlock.GetIdentifier(),
                        Message = "OR with ANDs that share a trigger",
                        Severity = _settings.Severity_Distributivity
                    };
                    LogFunc(entry);
                }
            }

            var seenKeys = new HashSet<string>();
            var seenRaws = new HashSet<string>();
            foreach (var item in namedBlock.Children.OfType<Core.Domain.KeyValuePair>())
            {
                if (!seenRaws.Add(item.Raw))
                {
                    var entry = new LogEntry
                    {
                        Location = namedBlock.GetIdentifier(),
                        Message = $"Duplicate trigger: {item.Raw}",
                        Severity = _settings.Severity_RepeatedTriggers
                    };
                    LogFunc(entry);
                    continue;
                }

                if (!seenKeys.Add(item.Key) && (item.Value.Equals("yes", StringComparison.OrdinalIgnoreCase) || item.Value.Equals("no", StringComparison.OrdinalIgnoreCase)))
                {
                    var entry = new LogEntry
                    {
                        Location = namedBlock.GetIdentifier(),
                        Message = $"Duplicate triggers: {item.Raw}",
                        Severity = _settings.Severity_ComplementaryTriggers
                    };
                    LogFunc(entry);
                }
            }

            if (key == "NOT")
            {
                if (childBlockKeys.Count() > 1)
                {
                    var entry = new LogEntry
                    {
                        Location = namedBlock.GetIdentifier(),
                        Message = $"NOT containing multiple elements",
                        Severity = _settings.Severity_NotWithMultipleChildren
                    };
                    LogFunc(entry);
                }

                if (childBlockKeys.Contains("NOT"))
                {
                    var entry = new LogEntry
                    {
                        Location = namedBlock.GetIdentifier(),
                        Message = $"Double negation - NOT contains NOT",
                        Severity = _settings.Severity_DoubleNegation
                    };
                    LogFunc(entry);
                }

                if (keyValuePairChildren.Any(x => x.Value.Equals("no", StringComparison.OrdinalIgnoreCase)))
                {
                    var entry = new LogEntry
                    {
                        Location = namedBlock.GetIdentifier(),
                        Message = $"Double negation - NOT contains trigger = no",
                        Severity = _settings.Severity_DoubleNegation
                    };
                    LogFunc(entry);
                }

                if (childBlockKeys.Contains("NOR"))
                {
                    var entry = new LogEntry
                    {
                        Location = namedBlock.GetIdentifier(),
                        Message = $"Double negation - NOT contains NOR",
                        Severity = _settings.Severity_DoubleNegation
                    };
                    LogFunc(entry);
                }

                if (childBlockKeys.Contains("NAND"))
                {
                    var entry = new LogEntry
                    {
                        Location = namedBlock.GetIdentifier(),
                        Message = $"Double negation - NOT contains NAND",
                        Severity = _settings.Severity_DoubleNegation
                    };
                    LogFunc(entry);
                }
            }
        }

        static bool HasRepeatedString(IEnumerable<IEnumerable<string>> listOfLists)
        {
            foreach (var sublist in listOfLists)
            {
                var seen = new HashSet<string>();
                foreach (var str in sublist)
                {
                    if (!seen.Add(str))
                    {
                        return true;
                    }
                }
            }
            return false; // No repeats found
        }
    }
}
