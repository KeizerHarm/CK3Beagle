using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
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
            public Severity Severity_Associativity { get; set; }
            public Severity Severity_Distributivity { get; set; }
            public Severity Severity_Idempotency { get; set; }
            public Severity Severity_Complementation { get; set; }
            public Severity Severity_NotIsNotNor { get; set; }
            public Severity Severity_Absorption { get; set; }
        }

        private Settings _settings;

        public OvercomplicatedBooleanDetector(ILogger logger, Settings settings) : base(logger)
        {
            _settings = settings;
        }

        public override void AnalyseDeclaration(Declaration declaration)
        {
            if (declaration.DeclarationType != DeclarationType.ScriptedTrigger)
                return;

            AnalyseAsTriggerBlock(declaration, "AND");
        }

        public override void AnalyseNamedBlock(NamedBlock namedBlock)
        {
            var key = namedBlock.Key.ToUpper();

            if (key == "limit" || key == "trigger")
            {
                key = "AND";
            }

            if (!Operators.Contains(key))
                return;

            AnalyseAsTriggerBlock(namedBlock, key);
        }

        public void AnalyseAsTriggerBlock(NamedBlock namedBlock, string key) { 

            var childNamedBlocks = namedBlock.Children.OfType<NamedBlock>();
            var childBlockKeys = childNamedBlocks.Select(x => x.Key.ToUpper());
            var childBinaryExpressions = namedBlock.Children.OfType<BinaryExpression>();
            var childBinaryExpressionKeys = childBinaryExpressions.Select(x => x.Key.ToLower());

            if (key == "AND")
            {
                if (childBlockKeys.Contains("AND"))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_Associativity,
                        _settings.Severity_Associativity,
                        "AND containing AND",
                        namedBlock.GetIdentifier());
                }

                var ORchildren = childNamedBlocks.Where(x => x.Key == "OR");
                if (HasRepeatedString(ORchildren.Select(x => x.Children.OfType<BinaryExpression>().Select(y => y.Raw))))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_Distributivity,
                        _settings.Severity_Distributivity,
                        "AND with ORs that share a trigger",
                        namedBlock.GetIdentifier());
                }

                if (childBlockKeys.Contains("OR"))
                {
                    foreach (var orChild in childNamedBlocks.Where(x => x.Key == "OR"))
                    {
                        if (HasRepeatedString(childBinaryExpressions.Select(x => x.Raw),
                            orChild.Children.OfType<BinaryExpression>().Select(x => x.Raw)))
                        {
                            logger.Log(
                                Smell.OvercomplicatedBoolean_Absorption,
                                _settings.Severity_Absorption,
                                "AND with OR that includes a trigger in the parent AND",
                                namedBlock.GetIdentifier());
                        }
                    }
                }
            }

            if (key == "OR")
            {
                if (childBlockKeys.Contains("OR"))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_Associativity,
                        _settings.Severity_Associativity,
                        "OR containing OR",
                        namedBlock.GetIdentifier());
                }

                var ANDchildren = childNamedBlocks.Where(x => x.Key == "AND");
                if (HasRepeatedString(ANDchildren.Select(x => x.Children.OfType<BinaryExpression>().Select(y => y.Raw))))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_Distributivity,
                        _settings.Severity_Distributivity,
                        "OR with ANDs that share a trigger",
                        namedBlock.GetIdentifier());
                }

                if (childBlockKeys.Contains("AND"))
                {
                    foreach (var andChild in childNamedBlocks.Where(x => x.Key == "AND"))
                    {
                        if (HasRepeatedString(childBinaryExpressions.Select(x => x.Raw),
                            andChild.Children.OfType<BinaryExpression>().Select(x => x.Raw)))
                        {
                            logger.Log(
                                Smell.OvercomplicatedBoolean_Absorption,
                                _settings.Severity_Absorption,
                                "OR with AND that includes a trigger in the parent OR",
                                namedBlock.GetIdentifier());
                        }
                    }
                }
            }

            var seenKeys = new HashSet<string>();
            var seenRaws = new HashSet<string>();
            foreach (var item in namedBlock.Children.OfType<BinaryExpression>())
            {
                if (!seenRaws.Add(item.Raw))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_Idempotency,
                        _settings.Severity_Idempotency,
                        $"Duplicate trigger: {item.Raw}",
                        namedBlock.GetIdentifier());
                    continue;
                }

                if (!seenKeys.Add(item.Key) && (item.Value.Equals("yes", StringComparison.OrdinalIgnoreCase) || item.Value.Equals("no", StringComparison.OrdinalIgnoreCase)))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_Complementation,
                        _settings.Severity_Complementation,
                        $"Complementary triggers: {item.Raw} and its inverse",
                        namedBlock.GetIdentifier());
                }
            }

            if (key == "NOR")
            {
                if (AllChildrenAreNegated(namedBlock))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_DoubleNegation,
                        _settings.Severity_DoubleNegation,
                        "All children of NOR are negated themselves",
                        namedBlock.GetIdentifier());
                }
            }

            if (key == "NAND")
            {
                if (AllChildrenAreNegated(namedBlock))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_DoubleNegation,
                        _settings.Severity_DoubleNegation,
                        "All children of NAND are negated themselves",
                        namedBlock.GetIdentifier());
                }
            }

            if (key == "NOT")
            {
                if (childBlockKeys.Count() + childBinaryExpressionKeys.Count() > 1)
                {
                    logger.Log(
                        Smell.NotIsNotNor,
                        _settings.Severity_NotIsNotNor,
                        "NOT containing multiple elements",
                        namedBlock.GetIdentifier());
                }

                if (AllChildrenAreNegated(namedBlock))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_DoubleNegation,
                        _settings.Severity_DoubleNegation,
                        $"All children of NOT are negated themselves",
                        namedBlock.GetIdentifier());
                }

                if (childBlockKeys.Contains("NOR"))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_DoubleNegation,
                        _settings.Severity_DoubleNegation,
                        "NOT contains NOR",
                        namedBlock.GetIdentifier());
                }

                if (childBlockKeys.Contains("NAND"))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_DoubleNegation,
                        _settings.Severity_DoubleNegation,
                        "NOT contains NAND",
                        namedBlock.GetIdentifier());
                }
            }
        }

        private bool AllChildrenAreNegated(NamedBlock namedBlock)
        {
            var blockChildrenToConsider = namedBlock.Children.OfType<NamedBlock>();
            var binaryExpressionChildrenToConsider = namedBlock.Children.OfType<BinaryExpression>();

            return blockChildrenToConsider.All(x => x.Key.ToUpper() == "NOT" || x.Key.ToUpper() == "NOR" || x.Key.ToUpper() == "NAND")
                && binaryExpressionChildrenToConsider.All(x => (x.Value.ToLower() == "no") || x.Scoper == "!=")
                && blockChildrenToConsider.Count() + binaryExpressionChildrenToConsider.Count() > 0;
        }

        static bool HasRepeatedString(IEnumerable<string> list1, IEnumerable<string> list2)
        {
            var seen = new HashSet<string>(list1);
            foreach (var str in list2)
            {
                if (seen.Contains(str))
                {
                    return true;
                }
            }
            return false;
        }


        static bool HasRepeatedString(IEnumerable<IEnumerable<string>> listOfLists)
        {
            var seen = new HashSet<string>();
            foreach (var sublist in listOfLists)
            {
                foreach (var str in sublist)
                {
                    if (!seen.Add(str))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
