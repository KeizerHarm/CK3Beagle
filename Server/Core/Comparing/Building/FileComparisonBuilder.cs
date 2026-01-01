using CK3BeagleServer.Core.Comparing.Domain;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace CK3BeagleServer.Core.Comparing.Building
{
    public class FileComparisonBuilder
    {
        private ScriptFile SourceFile;
        private ScriptFile EditFile;
        //public HashSet<string> AddedDeclarations;
        //public HashSet<string> ChangedDeclarations;
        //public HashSet<string> RemovedDeclarations;
        //public HashSet<string> UntouchedDeclarations;
        private readonly MixedComparer _mixedComparer = new();
        private readonly KeyBasedNodeComparer _keyComparer = new();

        public Delta BuildFileComparison(ScriptFile source, ScriptFile edit)
        {
            SourceFile = source;
            EditFile = edit;

            if (source.GetTrueHash() == edit.GetTrueHash())
            {
                return Delta.Unchanged(edit);
            }

            //(AddedDeclarations, RemovedDeclarations, ChangedDeclarations, UntouchedDeclarations)
            //    = ComparisonHelpers.SimpleListComparison(SourceFile.Declarations.ToDictionary(k => k.Key), EditFile.Declarations.ToDictionary(k => k.Key),
            //        (first, second) => first.GetTrueHash() == second.GetTrueHash());

            var initialDelta = GetDelta(SourceFile, EditFile);

            return initialDelta;
        }

        private Delta GetDelta(Node source, Node edit)
        {
            if (source.GetTrueHash() == edit.GetTrueHash() && source.ParentSymbolType == edit.ParentSymbolType)
            {
                return null;
            }

            if (source is Block sourceBlock && edit is Block editBlock)
            {
                int noOfSourceChildren = sourceBlock.Children.Count;
                int noOfEditChildren = editBlock.Children.Count;
                if (noOfSourceChildren == 0 && noOfEditChildren == 0)
                {
                    return Delta.Changed(edit, new ShadowNode(source));
                }

                var delta = BlockPropertiesAreSame(sourceBlock, editBlock)
                    ? Delta.ChangeInChildren(edit)
                    : Delta.Changed(edit, new ShadowNode(source));
                HandleNodeChildren(sourceBlock, editBlock, delta);

                return delta;
            }
            return Delta.Changed(edit, new ShadowNode(source));
        }

        private void HandleNodeChildren(Block sourceBlock, Block editBlock, Delta delta)
        {
            var sourceChildren = sourceBlock.Children;
            var editChildren = editBlock.Children;
            List<(Node, Node)> combinedMatches = GetMatches(sourceChildren, editChildren);

            int noOfSourceChildren = sourceChildren.Count;
            int noOfEditChildren = editChildren.Count;

            int sourceIndex = 0;
            int editIndex = 0;
            delta.Children = [];

            var matchedChildrenDict = combinedMatches.ToDictionary(k => k.Item1, v => v.Item2);
            var matchedChildrenDictReversed = combinedMatches.ToDictionary(k => k.Item2, v => v.Item1);
            while (sourceIndex < noOfSourceChildren || editIndex < noOfEditChildren)
            {
                var sourceChild =
                    sourceIndex < noOfSourceChildren
                    ? sourceBlock.Children[sourceIndex]
                    : null;
                var editChild =
                    editIndex < noOfEditChildren
                    ? editBlock.Children[editIndex]
                    : null;

                //If either child does not exist, then iterator went past max length and the rest of the nodes on the other are added/deleted
                if (sourceChild == null)
                {
                    delta.AddChild(Delta.Added(editChild));
                    editIndex++;
                    continue;
                }
                if (editChild == null)
                {
                    delta.AddChild(Delta.Deleted(new ShadowNode(sourceChild)));
                    sourceIndex++;
                    continue;
                }

                //If current pair is match, register this as changed pair
                if (matchedChildrenDict.TryGetValue(sourceChild, out var sourceChildPair) && sourceChildPair == editChild)
                {
                    //Recurse to get difference
                    var childDelta = GetDelta(sourceChild, editChild);
                    if (childDelta != null)
                        delta.AddChild(childDelta);

                    sourceIndex++;
                    editIndex++;
                    continue;
                }
                //If source child is match but edit child not the pair, continue iterating with editChild
                if (matchedChildrenDict.ContainsKey(sourceChild))
                {
                    delta.AddChild(Delta.Added(editChild));
                    editIndex++;
                    continue;
                }
                //If edit child is part of match but source child not the pair, continue iterating with sourceChild
                if (matchedChildrenDictReversed.ContainsKey(editChild))
                {
                    delta.AddChild(Delta.Deleted(new ShadowNode(sourceChild)));
                    sourceIndex++;
                    continue;
                }

                //If neither child is part of match, declare and continue iterating with both.
                delta.AddChild(Delta.Deleted(new ShadowNode(sourceChild)));
                editIndex++;
                delta.AddChild(Delta.Added(editChild));
                sourceIndex++;
            }

            if (delta.Children.All(x => x.Kind == DeltaKind.Unchanged))
                delta.Children = null;
        }

        private List<(Node, Node)> GetMatches(List<Node> sourceChildren, List<Node> editChildren)
        {
            List<(Node, Node)> combinedMatches = null;

            var trueHashBasedLcs = LcsCalculator.ComputeLcs(
                CollectionsMarshal.AsSpan(sourceChildren),
                CollectionsMarshal.AsSpan(editChildren),
                _mixedComparer);

            var sourceKeys = sourceChildren.OfType<NamedBlock>().Select(x => x.Key).ToList();
            sourceKeys.AddRange(sourceChildren.OfType<BinaryExpression>().Select(x => x.Key));

            var editKeys = editChildren.OfType<NamedBlock>().Select(x => x.Key).ToList();
            editKeys.AddRange(editChildren.OfType<BinaryExpression>().Select(x => x.Key));

            if (sourceKeys.SequenceEqual(editKeys))
            {
                combinedMatches = AddKeyMatches(sourceChildren, editChildren, trueHashBasedLcs);
            }
            else
            {
                combinedMatches = LcsToMatches(sourceChildren, editChildren, trueHashBasedLcs, _mixedComparer);
            }
            return combinedMatches;
        }

        private List<(Node, Node)> LcsToMatches(List<Node> sourceChildren, List<Node> editChildren, IEnumerable<Node> trueHashBasedLcs, IEqualityComparer<Node> hashComparer)
        {
            List<(Node, Node)> matches = [];
            int sourceSublistStartIndex = 0;
            int editSublistStartIndex = 0;
            foreach (var matchedNode in trueHashBasedLcs)
            {
                var sourceListMatchPos = FindNextInstance(sourceChildren, matchedNode, sourceSublistStartIndex, hashComparer);
                var editListMatchPos = FindNextInstance(editChildren, matchedNode, editSublistStartIndex, hashComparer);

                matches.Add((sourceChildren[sourceListMatchPos], editChildren[editListMatchPos]));
                sourceSublistStartIndex = sourceListMatchPos + 1;
                editSublistStartIndex = editListMatchPos + 1;
            }
            return matches;
        }

        private List<(Node, Node)> AddKeyMatches(List<Node> sourceChildren, List<Node> editChildren, IEnumerable<Node> trueHashBasedLcs)
        {
            List<(Node, Node)> matches = [];
            int sourceSublistStartIndex = 0;
            int editSublistStartIndex = 0;
            foreach (var matchedNode in trueHashBasedLcs)
            {
                var sourceListMatchPos = FindNextInstance(sourceChildren, matchedNode, sourceSublistStartIndex, _mixedComparer);
                var editListMatchPos = FindNextInstance(editChildren, matchedNode, editSublistStartIndex, _mixedComparer);

                var sourceSublistLength = sourceListMatchPos - sourceSublistStartIndex + 1;
                var editSublistLength = editListMatchPos - editSublistStartIndex + 1;

                if (!(sourceSublistLength > 1 && editSublistLength > 1))
                {
                    matches.Add((sourceChildren[sourceListMatchPos], editChildren[editListMatchPos]));
                    sourceSublistStartIndex = sourceListMatchPos + 1;
                    editSublistStartIndex = editListMatchPos + 1;
                    continue;
                }

                AppendKeyMatchesInRange(sourceChildren, editChildren, matches, sourceSublistStartIndex, editSublistStartIndex, sourceListMatchPos - 1, editListMatchPos - 1);
                matches.Add((sourceChildren[sourceListMatchPos], editChildren[editListMatchPos]));
                sourceSublistStartIndex = sourceListMatchPos + 1;
                editSublistStartIndex = editListMatchPos + 1;
            }

            AppendKeyMatchesInRange(sourceChildren, editChildren, matches, sourceSublistStartIndex, editSublistStartIndex, sourceChildren.Count - 1, editChildren.Count - 1);
            return matches;
        }

        private void AppendKeyMatchesInRange(List<Node> sourceChildren, List<Node> editChildren, List<(Node, Node)> combinedMatches, int sourceSublistStartIndex, int editSublistStartIndex, int sourceLastIndexToConsider, int editLastIndexToConsider)
        {
            var sourceSublistLength = sourceLastIndexToConsider - sourceSublistStartIndex + 1;
            var editSublistLength = editLastIndexToConsider - editSublistStartIndex + 1;
            var sourceSublist = sourceChildren.GetRange(sourceSublistStartIndex, sourceSublistLength);
            var editSublist = editChildren.GetRange(editSublistStartIndex, editSublistLength);

            if (LcsCalculator.ComputeLcsLength(
                CollectionsMarshal.AsSpan(sourceSublist),
                CollectionsMarshal.AsSpan(editSublist),
                _keyComparer) == 0)
            {
                return;
            }

            var keyBasedLcs = LcsCalculator.ComputeLcs(
                CollectionsMarshal.AsSpan(sourceSublist),
                CollectionsMarshal.AsSpan(editSublist), _keyComparer);

            var sourceIndex = sourceSublistStartIndex;
            var editIndex = sourceSublistStartIndex;

            while (sourceIndex <= sourceLastIndexToConsider && editIndex <= editLastIndexToConsider)
            {
                var sourceNode = sourceChildren[sourceIndex];
                var editNode = editChildren[editIndex];
                if (_keyComparer.Equals(sourceNode, editNode))
                {
                    combinedMatches.Add((sourceNode, editNode));
                    sourceIndex++;
                    editIndex++;
                    continue;
                }
                if (keyBasedLcs.Any(x => _keyComparer.Equals(sourceNode)))
                {
                    editIndex++;
                    continue;
                }
                if (keyBasedLcs.Any(x => _keyComparer.Equals(editNode)))
                {
                    sourceIndex++;
                    continue;
                }
                sourceIndex++;
                editIndex++;
                continue;
            }
        }

        private int FindNextInstance(List<Node> list, Node matchedNode, int startIndex, IEqualityComparer<Node> comparer)
        {
            for (int i = startIndex; i < list.Count; i++)
            {
                if (comparer.Equals(list[i], matchedNode))
                    return i;
            }
            return -1;
        }

        private void MatchTotallyIdenticalChildren(Block sourceBlock, Block editBlock, List<(Node, Node)> matchedChildren)
        {
            foreach (var sourceChild in sourceBlock.Children)
            {
                if (matchedChildren.Any(x => x.Item1 == sourceChild))
                    continue;

                foreach (var editChild in editBlock.Children)
                {
                    if (matchedChildren.Any(x => x.Item2 == editChild))
                        continue;

                    if (sourceChild.GetTrueHash() == editChild.GetTrueHash())
                    {
                        matchedChildren.Add((sourceChild, editChild));
                        break;
                    }
                }
            }
        }

        //If both sourceBlock and editBlock's children's keys are equal and in the same order, treat them all as matched.
        private void MatchOnIdenticalKeys(Block sourceBlock, Block editBlock, List<(Node, Node)> matchedChildren)
        {
            List<string> sourceKeys = [];
            foreach (var sourceChild in sourceBlock.Children)
            {
                if (sourceChild is NamedBlock namedBlock)
                {
                    sourceKeys.Add(namedBlock.Key);
                }
                if (sourceChild is BinaryExpression binExp)
                {
                    sourceKeys.Add(binExp.Key);
                }
            }
            List<string> editKeys = [];
            foreach (var editChild in editBlock.Children)
            {
                if (editChild is NamedBlock namedBlock)
                {
                    editKeys.Add(namedBlock.Key);
                }
                if (editChild is BinaryExpression binExp)
                {
                    editKeys.Add(binExp.Key);
                }
            }

            if (sourceKeys.SequenceEqual(editKeys))
            {
                foreach (var sourceChild in sourceBlock.Children.OfType<BinaryExpression>())
                {
                    if (matchedChildren.Any(x => x.Item1 == sourceChild))
                        continue;

                    foreach (var editChild in editBlock.Children.OfType<BinaryExpression>())
                    {
                        if (matchedChildren.Any(x => x.Item2 == editChild))
                            continue;

                        if (sourceChild.Key == editChild.Key)
                        {
                            matchedChildren.Add((sourceChild, editChild));
                            break;
                        }
                    }
                }
                foreach (var sourceChild in sourceBlock.Children.OfType<NamedBlock>())
                {
                    if (matchedChildren.Any(x => x.Item1 == sourceChild))
                        continue;

                    foreach (var editChild in editBlock.Children.OfType<NamedBlock>())
                    {
                        if (matchedChildren.Any(x => x.Item2 == editChild))
                            continue;

                        if (sourceChild.Key == editChild.Key)
                        {
                            matchedChildren.Add((sourceChild, editChild));
                            break;
                        }
                    }
                }
            }
        }

        private bool BlockPropertiesAreSame(Block sourceBlock, Block editBlock)
        {
            if (sourceBlock is Declaration sourceDecl && editBlock is Declaration editDecl) { 
                return sourceDecl.Key == editDecl.Key 
                    && sourceDecl.DeclarationType == editDecl.DeclarationType;
            }
            if (sourceBlock is NamedBlock sourceNamedBlock && editBlock is NamedBlock editNamedBlock)
            {
                return sourceNamedBlock.Key == editNamedBlock.Key
                    && sourceNamedBlock.Scoper == editNamedBlock.Scoper;
            }
            //Anonymous blocks have no other distinguishing factors, so they are always equal
            return true;
        }
    }
}
