using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources.DetectorSettings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Analysis.Detectors
{
    public class OvercomplicatedTriggerDetector : BaseDetector
    {
        private static readonly HashSet<string> Operators =
        [
            "AND", "OR", "NOT", "NOR", "NAND"
        ];

        private OvercomplicatedTriggerSettings _settings;

        public OvercomplicatedTriggerDetector(ILogger logger, Context context, OvercomplicatedTriggerSettings settings) : base(logger, context)
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

            if (key == "limit" || key == "trigger" || key == "potential" || key == "is_shown")
            {
                key = "AND";
            }

            if (!Operators.Contains(key))
                return;

            AnalyseAsTriggerBlock(namedBlock, key);
        }

        public void AnalyseAsTriggerBlock(NamedBlock namedBlock, string key) {

            var triggerChildren = namedBlock.Children.Where(x => x.NodeType == NodeType.Trigger);
            var childNamedBlocks = namedBlock.Children.OfType<NamedBlock>();
            var ANDchildren = childNamedBlocks.Where(x => x.Key == "AND");
            var ORchildren = childNamedBlocks.Where(x => x.Key == "OR");

            var childBlockKeys = childNamedBlocks.Select(x => x.Key.ToUpper());
            var childBinaryExpressions = namedBlock.Children.OfType<BinaryExpression>();
            var childBinaryExpressionKeys = childBinaryExpressions.Select(x => x.Key.ToLower());

            if (key == "AND")
            {
                foreach (var child in ANDchildren)
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_Associativity,
                        _settings.Associativity_Severity,
                        "This AND-block is inside another AND",
                        child);
                }

                var chilrenOfORChildren = ORchildren.Select(x => x.Children.Where(x => x.NodeType == NodeType.Trigger).Select(x => x.StringRepresentation));
                if (ORchildren.Count() > 1 && ORchildren.Count() == triggerChildren.Count() &&
                    StringOccursInEverySublist(chilrenOfORChildren) is string duplicate
                    && chilrenOfORChildren.All(x => x.Count() < 3)
                    )
                {
                    var secondaryLogEntries = new List<LogEntry>();
                    foreach (var binExp in ORchildren.SelectMany(x => x.Children.Where(x => x.NodeType == NodeType.Trigger)))
                    {
                        if (binExp.StringRepresentation == duplicate)
                        {
                            secondaryLogEntries.Add(
                                LogEntry.MinimalLogEntry(
                                    "Repeated trigger",
                                    namedBlock.File.AbsolutePath,
                                    binExp.Start,
                                    binExp.End));
                        }
                    }

                    var logEntry = new LogEntry(
                        Smell.OvercomplicatedBoolean_Distributivity,
                        _settings.Distributivity_Severity,
                        "AND with ORs that share a trigger",
                        namedBlock);

                    logger.Log(
                        logEntry,
                        [.. secondaryLogEntries]);
                }
                foreach (var orChild in ORchildren)
                {
                    if (HasRepeatedString(childBinaryExpressions.Select(x => x.StringRepresentation),
                        orChild.Children.OfType<BinaryExpression>().Select(x => x.StringRepresentation)))
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
                foreach (var orChild in ORchildren)
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_Associativity,
                        _settings.Associativity_Severity,
                        "This OR-block is inside another OR",
                        orChild);
                }

                var childrenOfANDChildren = ANDchildren.Select(x => x.Children.Where(x => x.NodeType == NodeType.Trigger).Select(x => x.StringRepresentation));
                if (ANDchildren.Count() > 1 && ANDchildren.Count() == triggerChildren.Count() &&
                    StringOccursInEverySublist(
                        childrenOfANDChildren
                        ) is string duplicate
                    && childrenOfANDChildren.All(x => x.Count() < 3))
                {
                    var secondaryLogEntries = new List<LogEntry>();
                    foreach (var binExp in ANDchildren.SelectMany(x => x.Children.Where(x => x.NodeType == NodeType.Trigger)))
                    {
                        if (binExp.StringRepresentation == duplicate)
                        {
                            secondaryLogEntries.Add(
                                LogEntry.MinimalLogEntry(
                                    "Repeated trigger",
                                    namedBlock.File.AbsolutePath,
                                    binExp.Start,
                                    binExp.End));
                        }
                    }

                    var logEntry = new LogEntry(
                        Smell.OvercomplicatedBoolean_Distributivity,
                        _settings.Distributivity_Severity,
                        "OR with ANDs that share a trigger",
                        namedBlock);
                    
                    logger.Log(
                        logEntry,
                        [.. secondaryLogEntries]);
                }
                foreach (var andChild in ANDchildren)
                {
                    if (HasRepeatedString(childBinaryExpressions.Select(x => x.StringRepresentation),
                        andChild.Children.OfType<BinaryExpression>().Select(x => x.StringRepresentation)))
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
                if (!seenRaws.Add(item.StringRepresentation))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_Idempotency,
                        _settings.Idempotency_Severity,
                        $"Duplicate trigger: {item.StringRepresentation}",
                        item);
                    continue;
                }

                if (!seenKeys.Add(item.Key) && (item.Value.Equals("yes", StringComparison.OrdinalIgnoreCase) || item.Value.Equals("no", StringComparison.OrdinalIgnoreCase)))
                {
                    logger.Log(
                        Smell.OvercomplicatedBoolean_Complementation,
                        _settings.Complementation_Severity,
                        $"Complementary triggers: {item.StringRepresentation} and its inverse",
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

        static string GetRepeatedString(IEnumerable<string> list)
        {
            var seen = new HashSet<string>();
            foreach (var str in list)
            {
                if (!seen.Add(str))
                {
                    return str;
                }
            }
            return null;
        }

        static string StringOccursInEverySublist(IEnumerable<IEnumerable<string>> list)
        {
            var sublists = list as IList<IEnumerable<string>> ?? list.ToList();
            if (sublists.Count == 0) return null;

            var sets = sublists
                .Select(s => s is HashSet<string> hs ? hs : new HashSet<string>(s))
                .ToList();

            // Start with the smallest set to minimise work
            var common = new HashSet<string>(sets.OrderBy(s => s.Count).First());

            foreach (var set in sets)
            {
                common.IntersectWith(set);
                if (common.Count == 0)
                    return null;
            }

            return common.FirstOrDefault();
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
