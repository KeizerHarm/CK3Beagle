using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Resources.DetectorSettings;
using System.Collections.Generic;
using System.Linq;

namespace CK3BeagleServer.Analysing.Common.Detectors
{
    public class DuplicationDetector : BaseDetector
    {
        private readonly DuplicationSettings _settings;
        private static readonly Dictionary<int, string> SingleCloneMessages = new Dictionary<int, string>(50);
        private static readonly Dictionary<(int, int), string> MultiCloneMessages = new Dictionary<(int, int), string>(50);

        public DuplicationDetector(ILogger logger, Context context, DuplicationSettings settings) : base(logger, context)
        {
            _settings = settings;
        }

        private static string GetMessage(int size, int copies)
        {
            if (copies == 2)
            {
                if (SingleCloneMessages.ContainsKey(size))
                    return SingleCloneMessages[size];

                var singleCloneMessage = SingleCloneMessages[size] = $"{size}-statement block is duplicate";
                if (size < 50)
                    SingleCloneMessages[size] = singleCloneMessage;

                return singleCloneMessage;
            }

            if (MultiCloneMessages.ContainsKey((size, copies)))
                return MultiCloneMessages[(size, copies)];

            var multiCloneMessage = MultiCloneMessages[(size, copies)] = $"{size}-statement block occurs {copies} times";
            if (size < 50 && copies < 50)
                MultiCloneMessages[(size, copies)] = multiCloneMessage;

            return multiCloneMessage;
        }

        private readonly Dictionary<int, List<Node>> nodesByStrictHash = [];

        public override void EnterNamedBlock(NamedBlock namedBlock)
        {
            //Consider node itself as potential duplicate
            if (namedBlock.NodeType != NodeType.NonStatement && namedBlock.GetSize() >= _settings.MinSize)
            {
                nodesByStrictHash.AddToDictCollection((Node)namedBlock, namedBlock.GetDuplicationCheckingHash());
            }
        }

        public override void Finish()
        {
            FilterClones();

            foreach (var nodes in nodesByStrictHash)
            {
                var clones = nodes.Value;
                var totalSize = clones.First().GetSize();
                var msg = GetMessage(totalSize, clones.Count);

                for (var i = 0; i < clones.Count; i++)
                {
                    var clone = clones[i];
                    var originalLog = new LogEntry(Smell.Duplication,
                        _settings.Severity,
                        msg,
                        clone.File.AbsolutePath,
                        clone.Start,
                        clone.End);

                    var relatedLogs = new List<LogEntry>();

                    for (var j = 0; j < clones.Count; j++)
                    {
                        if (i == j) continue;
                        var copy = clones[j];
                        relatedLogs.Add(LogEntry.MinimalLogEntry("Other location", copy.File.AbsolutePath, copy.Start, copy.End));
                    }
                    logger.Log(originalLog, [.. relatedLogs]);
                }
            }
        }

        private void FilterClones()
        {
            var listsToRemove = new List<int>();
            foreach (var list in nodesByStrictHash)
            {
                var elementsToRemove = new HashSet<Node>();
                foreach (var item in list.Value)
                {
                    if (nodesByStrictHash.ContainsKey(item.Parent?.GetDuplicationCheckingHash() ?? 0))
                        elementsToRemove.Add(item);
                    if (nodesByStrictHash.ContainsKey(item.Parent?.Parent?.GetDuplicationCheckingHash() ?? 0))
                        elementsToRemove.Add(item);
                    if (nodesByStrictHash.ContainsKey(item.Parent?.Parent?.Parent?.GetDuplicationCheckingHash() ?? 0))
                        elementsToRemove.Add(item);
                    if (nodesByStrictHash.ContainsKey(item.Parent?.Parent?.Parent?.Parent?.GetDuplicationCheckingHash() ?? 0))
                        elementsToRemove.Add(item);
                    if (nodesByStrictHash.ContainsKey(item.Parent?.Parent?.Parent?.Parent?.Parent?.GetDuplicationCheckingHash() ?? 0))
                        elementsToRemove.Add(item);
                    if (nodesByStrictHash.ContainsKey(item.Parent?.Parent?.Parent?.Parent?.Parent?.Parent?.GetDuplicationCheckingHash() ?? 0))
                        elementsToRemove.Add(item);
                }
                
                list.Value.RemoveAll(x => elementsToRemove.Any(y => y.GetDuplicationCheckingHash() == x.GetDuplicationCheckingHash()));

                if (list.Value.Count <= 1)
                {
                    listsToRemove.Add(list.Key);
                }
            }

            foreach (var listToRemove in listsToRemove)
            {
                nodesByStrictHash.Remove(listToRemove);
            }
        }
    }
}
