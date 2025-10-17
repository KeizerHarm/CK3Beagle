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
                        _settings.Severity_Associativity,
                        "AND containing AND",
                        child.GetIdentifier(),
                        child.StartIndex,
                        child.EndIndex);
                }

                var ORchildren = childNamedBlocks.Where(x => x.Key == "OR");
                if (HasRepeatedString(ORchildren.Select(x => x.Children.OfType<BinaryExpression>().Select(y => y.Raw))))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_Distributivity,
                        _settings.Severity_Distributivity,
                        "AND with ORs that share a trigger",
                        namedBlock.GetIdentifier(),
                        namedBlock.StartIndex,
                        namedBlock.EndIndex);
                }
                foreach (var orChild in ORchildren)
                {
                    if (HasRepeatedString(childBinaryExpressions.Select(x => x.Raw),
                        orChild.Children.OfType<BinaryExpression>().Select(x => x.Raw)))
                    {
                        logger.Log(
                            Smell.OvercomplicatedBoolean_Absorption,
                            _settings.Severity_Absorption,
                            "AND with OR that includes a trigger in the parent AND",
                            namedBlock.GetIdentifier(),
                            orChild.StartIndex,
                            orChild.EndIndex);
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
                        _settings.Severity_Associativity,
                        "OR containing OR",
                        orChild.GetIdentifier(),
                        orChild.StartIndex,
                        orChild.EndIndex);
                }


                var ANDchildren = childNamedBlocks.Where(x => x.Key == "AND");
                if (HasRepeatedString(ANDchildren.Select(x => x.Children.OfType<BinaryExpression>().Select(y => y.Raw))))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_Distributivity,
                        _settings.Severity_Distributivity,
                        "OR with ANDs that share a trigger",
                        namedBlock.GetIdentifier(),
                        namedBlock.StartIndex,
                        namedBlock.EndIndex);
                }
                foreach (var andChild in ANDchildren)
                {
                    if (HasRepeatedString(childBinaryExpressions.Select(x => x.Raw),
                        andChild.Children.OfType<BinaryExpression>().Select(x => x.Raw)))
                    {
                        logger.Log(
                            Smell.OvercomplicatedBoolean_Absorption,
                            _settings.Severity_Absorption,
                            "OR with AND that includes a trigger in the parent OR",
                            namedBlock.GetIdentifier(),
                            namedBlock.StartIndex,
                            namedBlock.EndIndex);
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
                        namedBlock.GetIdentifier(),
                        item.StartIndex,
                        item.EndIndex);
                    continue;
                }

                if (!seenKeys.Add(item.Key) && (item.Value.Equals("yes", StringComparison.OrdinalIgnoreCase) || item.Value.Equals("no", StringComparison.OrdinalIgnoreCase)))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_Complementation,
                        _settings.Severity_Complementation,
                        $"Complementary triggers: {item.Raw} and its inverse",
                        namedBlock.GetIdentifier(),
                        namedBlock.StartIndex,
                        namedBlock.EndIndex);
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
                        namedBlock.GetIdentifier(),
                        namedBlock.StartIndex,
                        namedBlock.EndIndex);
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
                        namedBlock.GetIdentifier(),
                        namedBlock.StartIndex,
                        namedBlock.EndIndex);
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
                        namedBlock.GetIdentifier(),
                        namedBlock.StartIndex,
                        namedBlock.EndIndex);
                }

                if (AllChildrenAreNegated(namedBlock))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_DoubleNegation,
                        _settings.Severity_DoubleNegation,
                        $"All children of NOT are negated themselves",
                        namedBlock.GetIdentifier(),
                        namedBlock.StartIndex,
                        namedBlock.EndIndex);
                }

                if (childBlockKeys.Contains("NOR"))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_DoubleNegation,
                        _settings.Severity_DoubleNegation,
                        "NOT contains NOR",
                        namedBlock.GetIdentifier(),
                        namedBlock.StartIndex,
                        namedBlock.EndIndex);
                }

                if (childBlockKeys.Contains("NAND"))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_DoubleNegation,
                        _settings.Severity_DoubleNegation,
                        "NOT contains NAND",
                        namedBlock.GetIdentifier(),
                        namedBlock.StartIndex,
                        namedBlock.EndIndex);
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
