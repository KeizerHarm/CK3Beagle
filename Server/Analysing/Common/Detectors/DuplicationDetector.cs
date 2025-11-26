using CK3Analyser.Analysing.Logging;
using CK3Analyser.Core;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources.DetectorSettings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Analysing.Common.Detectors
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


        //private readonly Dictionary<int, List<BinaryExpression>> binaryExpressionsByKeyHash = [];
        //private readonly Dictionary<int, List<BinaryExpression>> binaryExpressionsByStrictHash = [];

        //private readonly Dictionary<int, List<Node>> nodesByKeyHash = [];
        private readonly Dictionary<int, List<Node>> nodesByStrictHash = [];
        private readonly Dictionary<int, List<List<Node>>> sequencesByParentStrictHash = [];

        public override void VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            //if (binaryExpression.NodeType != NodeType.Statement)
            //    return;

            //binaryExpressionsByKeyHash.AddToDictCollection(binaryExpression, binaryExpression.GetLooseHashCode());
            //binaryExpressionsByStrictHash.AddToDictCollection(binaryExpression, binaryExpression.GetStrictHashCode());

            //nodesByKeyHash.AddToDictCollection((Node)binaryExpression, binaryExpression.GetLooseHashCode());

            //nodesByStrictHash.AddToDictCollection((Node)binaryExpression, binaryExpression.GetStrictHashCode());
        }

        //private readonly Dictionary<int, List<NamedBlock>> namedBlocksByKeyHash = [];
        //private readonly Dictionary<int, List<NamedBlock>> namedBlocksByStrictHash = [];
        public override void EnterNamedBlock(NamedBlock namedBlock)
        {
            //namedBlocksByKeyHash.AddToDictCollection(namedBlock, namedBlock.GetLooseHashCode());
            //namedBlocksByStrictHash.AddToDictCollection(namedBlock, namedBlock.GetStrictHashCode());

            //nodesByKeyHash.AddToDictCollection((Node)namedBlock, namedBlock.GetLooseHashCode());

            //Consider node itself as potential duplicate
            if (namedBlock.NodeType != NodeType.NonStatement && namedBlock.GetSize() >= _settings.MinSize)
            {
                nodesByStrictHash.AddToDictCollection((Node)namedBlock, namedBlock.GetStrictHashCode());
            }

            //Consider node's children
            if (nodesByStrictHash.ContainsKey(namedBlock.GetStrictHashCode()))
            {
                //sequencesByParentStrictHash.Remove(namedBlock.GetStrictHashCode());
            }
            else
            {
                //var sequence = namedBlock.Children.Where(x => x.NodeType != NodeType.NonStatement).ToList();
                //if (sequence.Count <= 1) 
                //    return;

                //var totalWeight = sequence.Sum(x => x.GetSize());
                //if (totalWeight > _settings.MinSize)
                //{
                //    sequencesByParentStrictHash.AddToDictCollection(sequence, namedBlock.GetStrictHashCode());
                //}
            }
        }

        //public class KMer
        //{
        //    public List<Node> OriginSequence { get; set; }

        //    public int StartPos { get; set;  }
        //    public int EndPos { get; set;  }

        //    private int _hash;
        //    public int Hash { 
        //        get
        //        {
        //            if (_hash == 0)
        //            {
        //                _hash = OriginSequence.GetRange(StartPos, EndPos).Sum(x => x.GetStrictHashCode());
        //            }
        //            return _hash;
        //        }
        //    }

        //    public bool IsSubsumedBy(KMer other)
        //    {
        //        return OriginSequence == other.OriginSequence
        //            && other.StartPos >= StartPos
        //            && other.StartPos <= EndPos
        //            && other.EndPos >= StartPos
        //            && other.EndPos <= EndPos;
        //    }
        //}


        //public void KMerSequencing()
        //{
        //    var allSequences = sequencesByParentStrictHash.SelectMany(x => x.Value);
        //    var allKMers = new Dictionary<int, List<KMer>>();
        //    foreach (var sequence in allSequences)
        //    {
        //        var rollingSize = sequence[0].GetSize();
        //        int i = 0; int j = 1;

        //        while (i < sequence.Count) 
        //        {
        //            while (j < sequence.Count)
        //            {
        //                if (rollingSize >= _settings.MinSize)
        //                {
        //                    var newKMer = new KMer
        //                    {
        //                        OriginSequence = sequence,
        //                        StartPos = i,
        //                        EndPos = j
        //                    };
        //                    allKMers.AddToDictCollection(newKMer, newKMer.Hash);
        //                }
        //                j++;
        //                rollingSize += sequence[j].GetSize();
        //            }
        //            rollingSize -= sequence[i].GetSize();
        //            i++;
        //        }
        //    }

        //    var allSeedGroups = allKMers.Where(x => x.Value.Count > 2);

        //    foreach (var matchedSeeds in allSeedGroups)
        //    {

        //    }
        //}

        public override void Finish()
        {
            FilterClones();

            //var sequenceLengthHistogram = new Dictionary<int, int>();
            //var sequences = sequencesByParentStrictHash.SelectMany(x => x.Value).ToList();
            //foreach (var sequence in sequences)
            //{
            //    if (sequenceLengthHistogram.TryGetValue(sequence.Count, out _))
            //    {
            //        sequenceLengthHistogram[sequence.Count] += 1;
            //    }
            //    else
            //    {
            //        sequenceLengthHistogram.Add(sequence.Count, 1);
            //    }
            //}

            //var ordered = sequenceLengthHistogram.OrderBy(x => x.Key).ToList();
            //foreach (var item in ordered)
            //{
            //    Console.WriteLine(item.Key + ": " + item.Value + " times");
            //}

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


            //SequenceClones();
            //foreach (var clonedSequence in cloneSequences)
            //{
            //    var totalSize = clonedSequence.Item1.Sum(x => x.GetSize());

            //    logger.Log(Smell.Duplication,
            //        _settings.Severity,
            //        $"{totalSize}-statement sequence duplicates sequence in {clonedSequence.Item2.First().Parent.GetIdentifier()}",
            //        clonedSequence.Item1.First().Parent.GetIdentifier());

            //    logger.Log(Smell.Duplication,
            //        _settings.Severity,
            //        $"{totalSize}-statement sequence duplicates sequence in {clonedSequence.Item1.First().Parent.GetIdentifier()}",
            //        clonedSequence.Item2.First().Parent.GetIdentifier());
            //    //Console.WriteLine($"Detected duplicated {totalSize}-statement sequences:");
            //    //Console.WriteLine("- " + clonedSequence.Item1.First().Parent.GetIdentifier());
            //    //Console.WriteLine("- " + clonedSequence.Item2.First().Parent.GetIdentifier());

            //    //foreach (var dupe in clonedSequence.Item1)
            //    //{
            //    //    Console.WriteLine(dupe.Raw);
            //    //}
            //    //Console.WriteLine();
            //    //Console.WriteLine("---------------------------------------");
            //    //Console.WriteLine();
            //}
            //Console.WriteLine("Found " + cloneSequences.Count);
        }

        private readonly List<Tuple<List<Node>, List<Node>>> cloneSequences = [];

        private void FilterClones()
        {
            var listsToRemove = new List<int>();
            foreach (var list in nodesByStrictHash)
            {
                var elementsToRemove = new HashSet<Node>();
                foreach (var item in list.Value)
                {
                    if (nodesByStrictHash.ContainsKey(item.Parent?.GetStrictHashCode() ?? 0))
                        elementsToRemove.Add(item);
                    if (nodesByStrictHash.ContainsKey(item.Parent?.Parent?.GetStrictHashCode() ?? 0))
                        elementsToRemove.Add(item);
                    if (nodesByStrictHash.ContainsKey(item.Parent?.Parent?.Parent?.GetStrictHashCode() ?? 0))
                        elementsToRemove.Add(item);
                    if (nodesByStrictHash.ContainsKey(item.Parent?.Parent?.Parent?.Parent?.GetStrictHashCode() ?? 0))
                        elementsToRemove.Add(item);
                    if (nodesByStrictHash.ContainsKey(item.Parent?.Parent?.Parent?.Parent?.Parent?.GetStrictHashCode() ?? 0))
                        elementsToRemove.Add(item);
                    if (nodesByStrictHash.ContainsKey(item.Parent?.Parent?.Parent?.Parent?.Parent?.Parent?.GetStrictHashCode() ?? 0))
                        elementsToRemove.Add(item);
                }
                
                list.Value.RemoveAll(x => elementsToRemove.Any(y => y.GetStrictHashCode() == x.GetStrictHashCode()));

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

        private class CloneGroup
        {
            public int FirstNodeStrictHash { get; set; }
            public int SecondNodeStrictHash { get; set; }
            public List<List<Node>> Sequences { get; set; }
        }

        private readonly List<Dictionary<int, CloneGroup>> cloneGroups = [];

        private void SequenceClones()
        {
            var knownClones = new Dictionary<int, HashSet<int>>();

            var markedAsClones = new HashSet<int>();

            foreach (var list in nodesByStrictHash)
            {
                var relevantCloneGroups = new Dictionary<int, CloneGroup>();
                

                for (int i = 0; i < list.Value.Count; i++)
                {
                    var node1 = list.Value[i];

                    //First, try to match with any existing clone groups
                    var nextSibHash = node1.NextNonCommentSibling?.GetStrictHashCode() ?? -1;

                    if (relevantCloneGroups.TryGetValue(nextSibHash, out CloneGroup cloneGroup))
                    {

                    }

                    //if (node1.NextSiblingSkippingComments.GetStrictHashCode())







                    for (int j = i + 1; j < list.Value.Count; j++)
                    {
                        var node2 = list.Value[j];

                        var totalSize = 0;

                        if (knownClones.GetValueOrDefault(node1.GetHashCode())?.Contains(node2.GetHashCode()) ?? false)
                            continue;

                        if (knownClones.GetValueOrDefault(node2.GetHashCode())?.Contains(node1.GetHashCode()) ?? false)
                            continue;

                        if (node1.NextNonCommentSibling == null || node2.NextNonCommentSibling == null)
                            continue;

                        Node nextSib1 = node1.NextNonCommentSibling;
                        Node nextSib2 = node2.NextNonCommentSibling;

                        if (nextSib1.GetStrictHashCode() != nextSib2.GetStrictHashCode())
                            continue;

                        var sequence1 = new List<Node>() { node1 };
                        totalSize = node1.GetSize() + node2.GetSize();
                        var sequence2 = new List<Node>() { node2 };

                        knownClones.AddToDictCollection(node1.GetHashCode(), node1.GetHashCode());

                        while (nextSib1 != null && nextSib2 != null)
                        {
                            if (nextSib1.GetStrictHashCode() == nextSib2.GetStrictHashCode())
                            {
                                sequence1.Add(nextSib1);
                                sequence2.Add(nextSib2);
                                knownClones.AddToDictCollection(nextSib1.GetHashCode(), nextSib2.GetHashCode());
                                totalSize = nextSib1.GetSize() + nextSib2.GetSize();
                            }

                            if (nextSib1.NextNonCommentSibling == null || nextSib2.NextNonCommentSibling == null)
                                break;

                            nextSib1 = nextSib1.NextNonCommentSibling;
                            nextSib2 = nextSib2.NextNonCommentSibling;
                        }

                        if (totalSize / 2 >= _settings.MinSize)
                        {
                            var sequenceClone = new Tuple<List<Node>, List<Node>>(sequence1, sequence2);
                            cloneSequences.Add(sequenceClone);
                        }
                    }
                }
            }
        }

        //private Dictionary<int, List<Node>> looseCloneGroups;

        //private void FindLooseCloneGroups()
        //{
        //    var binExpClonedSequences = new List<Tuple<List<BinaryExpression>, List<BinaryExpression>>>();

        //    //var blackList = new HashSet<int>();
        //    var knownClones = new Dictionary<int, HashSet<int>>();

        //    foreach (var list in binaryExpressionsByKeyHash)
        //    {
        //        for (int i = 0; i < list.Value.Count; i++)
        //        {
        //            BinaryExpression binExp1 = list.Value[i];
        //            for (int j = i + 1; j < list.Value.Count; j++)
        //            {
        //                BinaryExpression binExp2 = list.Value[j];

        //                if (knownClones.GetValueOrDefault(binExp1.GetHashCode())?.Contains(binExp2.GetHashCode()) ?? false)
        //                    continue;

        //                if (knownClones.GetValueOrDefault(binExp2.GetHashCode())?.Contains(binExp1.GetHashCode()) ?? false)
        //                    continue;

        //                if (binExp1.NextSibling == null || binExp2.NextSibling == null)
        //                    continue;

        //                if (binExp1.NextSibling.GetType() != typeof(BinaryExpression) || binExp2.NextSibling.GetType() != typeof(BinaryExpression))
        //                    continue;

        //                BinaryExpression nextSib1 = (BinaryExpression)binExp1.NextSibling, nextSib2 = (BinaryExpression)binExp2.NextSibling;

        //                if (nextSib1.GetHashCode() != nextSib2.GetHashCode())
        //                    continue;

        //                var sequence1 = new List<BinaryExpression>() { binExp1 };
        //                var sequence2 = new List<BinaryExpression>() { binExp2 };
        //                knownClones.AddToDictCollection(binExp1.GetHashCode(), binExp1.GetHashCode());

        //                while (nextSib1 != null && nextSib2 != null)
        //                {
        //                    if (nextSib1.GetHashCode() == nextSib2.GetHashCode())
        //                    {
        //                        sequence1.Add(nextSib1);
        //                        sequence2.Add(nextSib2);
        //                        knownClones.AddToDictCollection(nextSib1.GetHashCode(), nextSib2.GetHashCode());
        //                    }

        //                    if (nextSib1.NextSibling == null || nextSib2.NextSibling == null)
        //                        break;

        //                    if (nextSib1.NextSibling.GetType() != typeof(BinaryExpression) || nextSib2.NextSibling.GetType() != typeof(BinaryExpression))
        //                        break;

        //                    nextSib1 = (BinaryExpression)nextSib1.NextSibling;
        //                    nextSib2 = (BinaryExpression)nextSib2.NextSibling;
        //                }

        //                if (sequence1.Count >= _settings.MinSize)
        //                {
        //                    var sequenceClone = new Tuple<List<BinaryExpression>, List<BinaryExpression>>(sequence1, sequence2);
        //                    binExpClonedSequences.Add(sequenceClone);
        //                }
        //            }
        //        }
        //    }

        //    foreach (var clonedSequence in binExpClonedSequences)
        //    {
        //        logger.Log(Smell.Duplication,
        //            _settings.Severity,
        //            $"{clonedSequence.Item1.Count}-item sequence duplicates sequence in {clonedSequence.Item2.First().Parent.GetIdentifier()}",
        //            clonedSequence.Item1.First().Parent.GetIdentifier());

        //        logger.Log(Smell.Duplication,
        //            _settings.Severity,
        //            $"{clonedSequence.Item2.Count}-item sequence duplicates sequence in {clonedSequence.Item1.First().Parent.GetIdentifier()}",
        //            clonedSequence.Item2.First().Parent.GetIdentifier());

        //        //Console.WriteLine($"Detected duplicated {clonedSequence.Item1.Count}-item sequences:");
        //        //Console.WriteLine("- " + clonedSequence.Item1.First().Parent.GetIdentifier());
        //        //Console.WriteLine("- " + clonedSequence.Item2.First().Parent.GetIdentifier());

        //        //foreach (var dupe in clonedSequence.Item1)
        //        //{
        //        //    Console.WriteLine("  " + dupe.Raw);
        //        //}
        //        //Console.WriteLine();
        //    }
        //    //Console.WriteLine("Found " + binExpClonedSequences.Count);
        //}

        public int GetNoOfDifferentValues(List<Node> first, List<Node> second)
        {
            int noOfDifferences = 0;
            Node firstNode = first.FirstOrDefault(x => x.GetType() != typeof(Comment));
            Node secondNode = second.FirstOrDefault(x => x.GetType() != typeof(Comment));

            int i = 0;
            int j = 0;
            while (i < first.Count && j < second.Count)
            {
                firstNode = first.ElementAt(i);
                if (firstNode.GetType() == typeof(Comment))
                {
                    i++;
                    continue;
                }

                secondNode = second.ElementAt(j);
                if (secondNode.GetType() == typeof(Comment))
                {
                    j++;
                    continue;
                }

                noOfDifferences += GetNoOfDifferentValues(firstNode, secondNode);

                i++;
                j++;
            }

            return noOfDifferences;
        }
        private int GetNoOfDifferentValues(Node first, Node second)
        {
            if (first.GetStrictHashCode() == second.GetStrictHashCode())
                return 0;

            if (first is BinaryExpression && second is BinaryExpression)
            {
                //Since we know the GetStrictHashCode() is not equal, the values must be different
                return 1;
            }
            else if (first is Block firstChildBlock && second is Block secondChildBlock)
            {
                return GetNoOfDifferentValues(firstChildBlock, secondChildBlock);
            }
            else
            {
                throw new Exception(); //These should cover all block types at this point
            }
        }

        public int GetNoOfDifferentValues(Block first, Block second)
        {
            int noOfDifferences = 0;

            Node firstChild = first.Children.FirstOrDefault(x => x.GetType() != typeof(Comment));
            Node secondChild = second.Children.FirstOrDefault(x => x.GetType() != typeof(Comment));
            while (firstChild != null && secondChild != null)
            {
                noOfDifferences = GetNoOfDifferentValues(firstChild, secondChild);

                firstChild = firstChild.NextNonCommentSibling;
                secondChild = secondChild.NextNonCommentSibling;
            }

            return noOfDifferences;
        }
    }
}
