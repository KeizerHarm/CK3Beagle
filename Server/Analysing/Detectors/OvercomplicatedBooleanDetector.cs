using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources;
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
            public bool Enabled { get; init; }
            public Severity DoubleNegation_Severity { get; set; }
            public Severity Associativity_Severity { get; set; }
            public Severity Distributivity_Severity { get; set; }
            public Severity Idempotency_Severity { get; set; }
            public Severity Complementation_Severity { get; set; }
            public Severity NotIsNotNor_Severity { get; set; }
            public Severity Absorption_Severity { get; set; }
        }

        private Settings _settings;

        public OvercomplicatedBooleanDetector(ILogger logger, Context context, Settings settings) : base(logger, context)
        {
            _settings = settings;
        }

        public override void EnterDeclaration(Declaration declaration)
        {
            if (declaration.DeclarationType != DeclarationType.ScriptedTrigger)
                return;

            AnalyseAsTriggerBlock(declaration, "AND");
        }

        public override void EnterNamedBlock(NamedBlock namedBlock)
        {
            var key = namedBlock.Key.ToUpper();

            if (key == "limit" || key == "trigger" || key == "potential")
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
                var ANDchildren = childNamedBlocks.Where(x => x.Key == "AND");
                foreach (var child in ANDchildren)
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_Associativity,
                        _settings.Associativity_Severity,
                        "AND containing AND",
                        child);
                }

                var ORchildren = childNamedBlocks.Where(x => x.Key == "OR");
                if (HasRepeatedString(ORchildren.Select(x => x.Children.OfType<BinaryExpression>().Select(y => y.Raw))))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_Distributivity,
                        _settings.Distributivity_Severity,
                        "AND with ORs that share a trigger",
                        namedBlock);
                }
                foreach (var orChild in ORchildren)
                {
                    if (HasRepeatedString(childBinaryExpressions.Select(x => x.Raw),
                        orChild.Children.OfType<BinaryExpression>().Select(x => x.Raw)))
                    {
                        logger.Log(
                            Smell.OvercomplicatedBoolean_Absorption,
                            _settings.Absorption_Severity,
                            "AND with OR that includes a trigger in the parent AND",
                            orChild);
                    }
                }
            }

            if (key == "OR")
            {
                var ORChildren = childNamedBlocks.Where(x => x.Key == "OR");

                foreach (var orChild in ORChildren)
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_Associativity,
                        _settings.Associativity_Severity,
                        "OR containing OR",
                        orChild);
                }


                var ANDchildren = childNamedBlocks.Where(x => x.Key == "AND");
                if (HasRepeatedString(ANDchildren.Select(x => x.Children.OfType<BinaryExpression>().Select(y => y.Raw))))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_Distributivity,
                        _settings.Distributivity_Severity,
                        "OR with ANDs that share a trigger",
                        namedBlock);
                }
                foreach (var andChild in ANDchildren)
                {
                    if (HasRepeatedString(childBinaryExpressions.Select(x => x.Raw),
                        andChild.Children.OfType<BinaryExpression>().Select(x => x.Raw)))
                    {
                        logger.Log(
                            Smell.OvercomplicatedBoolean_Absorption,
                            _settings.Absorption_Severity,
                            "OR with AND that includes a trigger in the parent OR",
                            namedBlock);
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
                        _settings.Idempotency_Severity,
                        $"Duplicate trigger: {item.Raw}",
                        item);
                    continue;
                }

                if (!seenKeys.Add(item.Key) && (item.Value.Equals("yes", StringComparison.OrdinalIgnoreCase) || item.Value.Equals("no", StringComparison.OrdinalIgnoreCase)))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_Complementation,
                        _settings.Complementation_Severity,
                        $"Complementary triggers: {item.Raw} and its inverse",
                        namedBlock);
                }
            }

            if (key == "NOR")
            {
                if (AllChildrenAreNegated(namedBlock))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_DoubleNegation,
                        _settings.DoubleNegation_Severity,
                        "All children of NOR are negated themselves",
                        namedBlock);
                }
            }

            if (key == "NAND")
            {
                if (AllChildrenAreNegated(namedBlock))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_DoubleNegation,
                        _settings.DoubleNegation_Severity,
                        "All children of NAND are negated themselves",
                        namedBlock);
                }
            }

            if (key == "NOT")
            {
                if (childBlockKeys.Count() + childBinaryExpressionKeys.Count() > 1)
                {
                    logger.Log(
                        Smell.NotIsNotNor,
                        _settings.NotIsNotNor_Severity,
                        "NOT containing multiple elements",
                        namedBlock);
                }

                if (AllChildrenAreNegated(namedBlock))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_DoubleNegation,
                        _settings.DoubleNegation_Severity,
                        $"All children of NOT are negated themselves",
                        namedBlock);
                }

                if (childBlockKeys.Contains("NOR"))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_DoubleNegation,
                        _settings.DoubleNegation_Severity,
                        "NOT contains NOR",
                        namedBlock);
                }

                if (childBlockKeys.Contains("NAND"))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_DoubleNegation,
                        _settings.DoubleNegation_Severity,
                        "NOT contains NAND",
                        namedBlock);
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
