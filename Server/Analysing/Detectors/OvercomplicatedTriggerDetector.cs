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
            if (namedBlock.NodeType != NodeType.Trigger)
                return;

            var key = namedBlock.Key.ToUpper();
            if (!Operators.Contains(key))
                return;

            AnalyseAsTriggerBlock(namedBlock, key);
        }

        public void AnalyseAsTriggerBlock(NamedBlock namedBlock, string key)
        {
            var triggerChildren = namedBlock.Children.Where(x => x.NodeType == NodeType.Trigger);
            if (!triggerChildren.Any())
                return;

            var triggerBlockChildren = namedBlock.Children.OfType<NamedBlock>();
            var ANDchildren = triggerBlockChildren.Where(x => x.Key == "AND");
            var ORchildren = triggerBlockChildren.Where(x => x.Key == "OR");

            var childBlockKeys = triggerBlockChildren.Select(x => x.Key.ToUpper());
            var triggerBinExpChildren = triggerChildren.OfType<BinaryExpression>();
            var triggerBinExpKeys = triggerBinExpChildren.Select(x => x.Key.ToLower());

            if (key == "AND")
            {
                LogAssociativity(ANDchildren, "This AND-block is inside another AND");

                if (triggerChildren.Count() == ORchildren.Count())
                {
                    LogDistributivity(namedBlock, ORchildren, "AND with ORs that share a trigger");
                }
                LogAbsorption(triggerBinExpChildren, ORchildren, "AND with OR that includes a trigger in the parent AND");
            }

            if (key == "OR")
            {
                LogAssociativity(ORchildren, "This OR-block is inside another OR");

                if (triggerChildren.Count() == ANDchildren.Count())
                {
                    LogDistributivity(namedBlock, ANDchildren, "AND with ORs that share a trigger");
                }

                LogAbsorption(triggerBinExpChildren, ANDchildren, "OR with AND that includes a trigger in the parent OR");
            }

            LogIdempotencyAndComplementation(namedBlock, triggerBinExpChildren);

            if (key == "NOR")
            {
                if (AllChildrenAreNegated(triggerBlockChildren, triggerBinExpChildren))
                {
                    LogFullDoubleNegation(namedBlock, key);
                }
                else
                {
                    LogIncidentalDoubleNegation(triggerBlockChildren, key);
                }
            }

            if (key == "NAND")
            {
                if (AllChildrenAreNegated(triggerBlockChildren, triggerBinExpChildren))
                {
                    LogFullDoubleNegation(namedBlock, key);
                }
            }

            if (key == "NOT")
            {
                if (triggerChildren.Count() > 1)
                {
                    LogNotIsNotNor(namedBlock);
                }

                if (AllChildrenAreNegated(triggerBlockChildren, triggerBinExpChildren))
                {
                    LogFullDoubleNegation(namedBlock, key);
                }
                else
                {
                    LogIncidentalDoubleNegation(triggerBlockChildren, key);
                }
            }
        }

        private void LogNotIsNotNor(NamedBlock namedBlock)
        {
            logger.Log(
                Smell.NotIsNotNor,
                _settings.NotIsNotNor_Severity,
                "NOT containing multiple elements",
                namedBlock);
        }

        private void LogFullDoubleNegation(NamedBlock namedBlock, string key)
        {
            logger.Log(
                Smell.OvercomplicatedTrigger_DoubleNegation,
                _settings.DoubleNegation_Severity,
                $"All children of {key} are negated themselves",
                namedBlock);
        }

        private void LogIncidentalDoubleNegation(IEnumerable<NamedBlock> triggerBlockChildren, string parentKey)
        {
            foreach (var childBlock in triggerBlockChildren)
            {
                var childKey = childBlock.Key;
                if (childKey == "NOR")
                {
                    logger.Log(
                        Smell.OvercomplicatedTrigger_DoubleNegation,
                        _settings.DoubleNegation_Severity,
                        $"NOR within {parentKey}-block",
                        childBlock);
                }
                else if (childKey == "NAND")
                {
                    logger.Log(
                        Smell.OvercomplicatedTrigger_DoubleNegation,
                        _settings.DoubleNegation_Severity,
                        $"NAND within {parentKey}-block",
                        childBlock);
                }
                else if (childKey == "NOT")
                {
                    logger.Log(
                        Smell.OvercomplicatedTrigger_DoubleNegation,
                        _settings.DoubleNegation_Severity,
                        $"NOT within {parentKey}-block",
                        childBlock);
                }
            }
        }

        private void LogIdempotencyAndComplementation(NamedBlock namedBlock, IEnumerable<BinaryExpression> childBinaryExpressions)
        {
            var seenKeys = new HashSet<string>();
            var seenRaws = new HashSet<string>();
            var duplicateTriggers = new HashSet<string>();
            var complementaryTriggers = new HashSet<string>();
            foreach (var item in childBinaryExpressions)
            {
                if (!seenRaws.Add(item.StringRepresentation))
                {
                    duplicateTriggers.Add(item.StringRepresentation);
                    continue;
                }

                if (!seenKeys.Add(item.Key) && (item.Value.Equals("yes", StringComparison.OrdinalIgnoreCase) || item.Value.Equals("no", StringComparison.OrdinalIgnoreCase)))
                {
                    complementaryTriggers.Add(item.Key);
                }
            }

            foreach (var dupe in duplicateTriggers)
            {
                var occurences = childBinaryExpressions.Where(x => x.StringRepresentation == dupe);
                var secondaryLogEntries = occurences.Skip(1).Select(
                    x => LogEntry.MinimalLogEntry(
                        "Duplicate", x));

                logger.Log(
                    Smell.OvercomplicatedTrigger_Idempotency,
                    _settings.Idempotency_Severity,
                    $"Duplicate trigger: {dupe}",
                    occurences.First(),
                    [.. secondaryLogEntries]
                    );
            }

            foreach (var compl in complementaryTriggers)
            {
                var occurences = childBinaryExpressions.Where(x => x.Key == compl);
                var secondaryLogEntries = occurences.Skip(1).Select(
                    x => LogEntry.MinimalLogEntry(
                        "Complementation", x));

                logger.Log(
                    Smell.OvercomplicatedTrigger_Complementation,
                    _settings.Complementation_Severity,
                    $"Complementary triggers: {occurences.First().StringRepresentation} and its inverse",
                    occurences.First(),
                    [..secondaryLogEntries]);
            }
        }

        private void LogAssociativity(IEnumerable<NamedBlock> relevantChildren, string msg)
        {
            foreach (var child in relevantChildren)
            {
                logger.Log(
                    Smell.OvercomplicatedTrigger_Associativity,
                    _settings.Associativity_Severity,
                    msg,
                    child);
            }
        }

        private void LogAbsorption(IEnumerable<BinaryExpression> childBinaryExpressions, IEnumerable<NamedBlock> relevantChildren, string msg)
        {
            foreach (var child in relevantChildren)
            {
                if (GetRepeatedString(childBinaryExpressions.Select(x => x.StringRepresentation),
                    child.Children.OfType<BinaryExpression>().Select(x => x.StringRepresentation)) is string duplicate)
                {
                    var firstOccurence = childBinaryExpressions.First(x => x.StringRepresentation == duplicate);

                    var secondOccurence = child.Children.OfType<BinaryExpression>().First(x => x.StringRepresentation == duplicate);

                    logger.Log(
                        Smell.OvercomplicatedTrigger_Absorption,
                        _settings.Absorption_Severity,
                        msg,
                        child,
                        LogEntry.MinimalLogEntry("Trigger occurence", firstOccurence),
                        LogEntry.MinimalLogEntry("Trigger occurence", secondOccurence));
                }
            }
        }

        private void LogDistributivity(NamedBlock namedBlock, IEnumerable<NamedBlock> triggerChildren, string msg)
        {
            var grandChildren = triggerChildren.Select(x => x.Children.Where(x => x.NodeType == NodeType.Trigger).Select(x => x.StringRepresentation));
            if (triggerChildren.Count() > 1 &&
                StringOccursInEverySublist(grandChildren) is string duplicate
                && grandChildren.All(x => x.Count() < 3)
                )
            {
                var secondaryLogEntries = new List<LogEntry>();
                foreach (var trigger in triggerChildren.SelectMany(x => x.Children.Where(x => x.NodeType == NodeType.Trigger)))
                {
                    if (trigger.StringRepresentation == duplicate)
                    {
                        secondaryLogEntries.Add(
                            LogEntry.MinimalLogEntry(
                                "Repeated trigger",
                                namedBlock.File.AbsolutePath,
                                trigger.Start,
                                trigger.End));
                    }
                }

                var logEntry = new LogEntry(
                    Smell.OvercomplicatedTrigger_Distributivity,
                    _settings.Distributivity_Severity,
                    msg,
                    namedBlock);

                logger.Log(
                    logEntry,
                    [.. secondaryLogEntries]);
            }

        }

        private bool AllChildrenAreNegated(IEnumerable<NamedBlock> blocks, IEnumerable<BinaryExpression> binaryExpressions)
        {
            return blocks.All(x => x.Key.ToUpper() == "NOT" || x.Key.ToUpper() == "NOR" || x.Key.ToUpper() == "NAND")
                && binaryExpressions.All(x => (x.Value.ToLower() == "no") || x.Scoper == "!=")
                && blocks.Count() + binaryExpressions.Count() > 0;
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

        static string GetRepeatedString(IEnumerable<string> list1, IEnumerable<string> list2)
        {
            var seen = new HashSet<string>(list1);
            foreach (var str in list2)
            {
                if (seen.Contains(str))
                {
                    return str;
                }
            }
            return null;
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
