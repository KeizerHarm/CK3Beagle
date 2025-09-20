using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Analysis.Detectors
{
    public class DuplicationDetector : BaseDetector
    {
        public DuplicationDetector(ILogger logger, Context context) : base(logger, context)
        {
        }

        private readonly Dictionary<string, List<NamedBlock>> namedBlocksByKey = [];
        private readonly Dictionary<string, List<BinaryExpression>> binaryExpressionsByKey = [];

        public override void AnalyseBinaryExpression(BinaryExpression binaryExpression)
        {
            if (!binaryExpressionsByKey.TryGetValue(binaryExpression.Key, out var list))
            {
                list = new List<BinaryExpression>();
                binaryExpressionsByKey.Add(binaryExpression.Key, list);
            }
            list.Add(binaryExpression);
        }

        public override void AnalyseNamedBlock(NamedBlock namedBlock)
        {
            if (!namedBlocksByKey.TryGetValue(namedBlock.Key, out var list))
            {
                list = new List<NamedBlock>();
                namedBlocksByKey.Add(namedBlock.Key, list);
            }
            list.Add(namedBlock);
        }

        public override void Finish()
        {
            FindBinaryExpressionClones();
        }

        

        private void FindBinaryExpressionClones()
        {
            var binaryExpressionsByKeyFiltered = binaryExpressionsByKey.Where(x => x.Value.Count > 1);

            var binExpClones = new Dictionary<int, List<BinaryExpression>>();
            foreach (var list in binaryExpressionsByKeyFiltered)
            {
                foreach (var binExp in list.Value)
                {
                    AddToDictList(binExpClones, binExp, binExp.GetHashCode());
                }
            }

            var binExpClonesFiltered = binExpClones.Where(x => x.Value.Count > 1);

            var binExpClonedSequences = new List<Tuple<List<BinaryExpression>, List<BinaryExpression>>>();

            //var blackList = new HashSet<int>();
            var knownClones = new Dictionary<int, HashSet<int>>();

            foreach (var list in binExpClonesFiltered)
            {
                foreach (var binExp1 in list.Value)
                {
                    //if (blackList.Contains(binExp1.GetHashCode()))
                    //    continue;

                    foreach (var binExp2 in list.Value)
                    {
                        

                        //if (blackList.Contains(binExp2.GetHashCode()))
                        //    continue;

                        if (binExp1 == binExp2)
                            continue;

                        if (knownClones.GetValueOrDefault(binExp1.GetHashCode())?.Contains(binExp2.GetHashCode()) ?? false)
                            continue;

                        if (knownClones.GetValueOrDefault(binExp2.GetHashCode())?.Contains(binExp1.GetHashCode()) ?? false)
                            continue;

                        if (binExp1.NextSibling == null || binExp2.NextSibling == null)
                            continue;

                        if (binExp1.NextSibling.GetType() != typeof(BinaryExpression) || binExp2.NextSibling.GetType() != typeof(BinaryExpression))
                            continue;

                        BinaryExpression nextSib1 = (BinaryExpression)binExp1.NextSibling, nextSib2 = (BinaryExpression)binExp2.NextSibling;

                        if (nextSib1.GetHashCode() != nextSib2.GetHashCode())
                            continue;

                        var sequence1 = new List<BinaryExpression>() { binExp1 };
                        var sequence2 = new List<BinaryExpression>() { binExp2 };
                        AddToDictHashSet(knownClones, binExp1.GetHashCode(), binExp1.GetHashCode());
                        //blackList.Add(binExp2.GetHashCode());

                        while (nextSib1 != null && nextSib2 != null)
                        {
                            if (nextSib1.GetHashCode() == nextSib2.GetHashCode())
                            {
                                sequence1.Add(nextSib1);
                                sequence2.Add(nextSib2);
                                AddToDictHashSet(knownClones, nextSib1.GetHashCode(), nextSib2.GetHashCode());
                                //blackList.Add(nextSib2.GetHashCode());
                            }

                            if (nextSib1.NextSibling == null || nextSib2.NextSibling == null)
                                break;

                            if (nextSib1.NextSibling.GetType() != typeof(BinaryExpression) || nextSib2.NextSibling.GetType() != typeof(BinaryExpression))
                                break;

                            nextSib1 = (BinaryExpression)nextSib1.NextSibling;
                            nextSib2 = (BinaryExpression)nextSib2.NextSibling;
                        }

                        if (sequence1.Count >= 5)
                        {
                            var sequenceClone = new Tuple<List<BinaryExpression>, List<BinaryExpression>>(sequence1, sequence2);
                            binExpClonedSequences.Add(sequenceClone);
                        }
                    }
                }
            }

            foreach (var clonedSequence in binExpClonedSequences)
            {
                Console.WriteLine($"Detected duplicated {clonedSequence.Item1.Count}-item sequences:");
                Console.WriteLine("- " + clonedSequence.Item1.First().Parent.GetIdentifier());
                Console.WriteLine("- " + clonedSequence.Item2.First().Parent.GetIdentifier());

                foreach (var dupe in clonedSequence.Item1)
                {
                    Console.WriteLine("  " + dupe.Raw);
                }
                Console.WriteLine();
            }
        }

        private static void AddToDictList<KEY, VALUE>(Dictionary<KEY, List<VALUE>> dict, VALUE obj, KEY key)
        {
            if (!dict.TryGetValue(key, out var list))
            {
                list = new List<VALUE>();
                dict.Add(key, list);
            }
            list.Add(obj);
        }

        private static void AddToDictHashSet<KEY, VALUE>(Dictionary<KEY, HashSet<VALUE>> dict, VALUE obj, KEY key)
        {
            if (!dict.TryGetValue(key, out var list))
            {
                list = new HashSet<VALUE>();
                dict.Add(key, list);
            }
            list.Add(obj);
        }
    }
}
