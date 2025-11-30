using CK3BeagleServer.Core.Comparing.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CK3BeagleServer.Core.Comparing.Building
{
    public class FileComparisonBuilder
    {
        private ScriptFile SourceFile;
        private ScriptFile EditFile;
        public HashSet<string> AddedDeclarations;
        public HashSet<string> ChangedDeclarations;
        public HashSet<string> RemovedDeclarations;
        public HashSet<string> UntouchedDeclarations;

        public Delta BuildFileComparison(ScriptFile source, ScriptFile edit)
        {
            SourceFile = source;
            EditFile = edit;

            if (source.GetTrueHash() == edit.GetTrueHash())
            {
                return Delta.Unchanged(edit);
            }

            (AddedDeclarations, RemovedDeclarations, ChangedDeclarations, UntouchedDeclarations)
                = ComparisonHelpers.SimpleListComparison(SourceFile.Declarations.ToDictionary(k => k.Key), EditFile.Declarations.ToDictionary(k => k.Key),
                    (first, second) => first.GetTrueHash() == second.GetTrueHash());

            var initialMatches = ComparisonHelpers.GetIdenticalPairs(source.Children, edit.Children, node =>
            {
                if (node is Declaration decl)
                    return decl.Key;

                return node.GetTrueHash().ToString();
            }).ToList();

            var initialDelta = GetDelta(SourceFile, EditFile, initialMatches);

            return initialDelta;
        }

        private Delta GetDelta(Node source, Node edit, IEnumerable<(Node, Node)> matchedChildren = null)
        {
            if (source.GetTrueHash() == edit.GetTrueHash())
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

                // If we are in a Declaration block, the matching still needs to happen
                if (source is Declaration _)
                {
                    var matcher = new NodeMatcher();
                    matcher.MatchAllNodes(sourceBlock, editBlock);
                    matchedChildren = matcher.MatchedNodes.Select(x => (x.Key, x.Value));
                }
                //We are in a sub-block; just need to filter by matches within this block
                var relevantMatchedChildren = matchedChildren.Where(
                        x => sourceBlock.Children.Contains(x.Item1) && editBlock.Children.Contains(x.Item2)).ToList();

                //if (relevantMatchedChildren.Count() == 0)
                //{
                //    return Delta.Changed(edit, new ShadowNode(source));
                //}

                //If less than 1/3rd of children are matched, treat whole block as changed
                //if (relevantMatchedChildren.Count() < Math.Max(noOfSourceChildren, noOfEditChildren) / 3f)
                //{
                //    return Delta.Changed(edit, new ShadowNode(source));
                //}

                var delta = BlockPropertiesAreSame(sourceBlock, editBlock)
                    ? Delta.ChangeInChildren(edit)
                    : Delta.Changed(edit, new ShadowNode(source));
                HandleNodeChildren(sourceBlock, editBlock, matchedChildren, relevantMatchedChildren, delta);

                return delta;
            }
            return Delta.Changed(edit, new ShadowNode(source));
        }

        private void HandleNodeChildren(Block sourceBlock, Block editBlock, IEnumerable<(Node, Node)> matchedChildren, List<(Node, Node)> relevantMatchedChildren, Delta delta)
        {
            int noOfSourceChildren = sourceBlock.Children.Count;
            int noOfEditChildren = editBlock.Children.Count;
            HandleCaseOfIdenticalKeys(sourceBlock, editBlock, relevantMatchedChildren);
            int sourceIndex = 0;
            int editIndex = 0;
            delta.Children = [];

            var matchedChildrenDict = relevantMatchedChildren.ToDictionary(k => k.Item1, v => v.Item2);
            var matchedChildrenDictReversed = relevantMatchedChildren.ToDictionary(k => k.Item2, v => v.Item1);
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

                //If current pair is match, register this as unchanged pair
                if (matchedChildrenDict.TryGetValue(sourceChild, out var sourceChildPair) && sourceChildPair == editChild)
                {
                    //Recurse to get difference
                    var childDelta = GetDelta(sourceChild, editChild, matchedChildren);
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

                //Experimental fallback - if neither child is part of a match, but they share a key, treat them as matches anyway

                //If neither child is part of match, declare and continue iterating with both.
                delta.AddChild(Delta.Deleted(new ShadowNode(sourceChild)));
                editIndex++;
                delta.AddChild(Delta.Added(sourceChild));
                sourceIndex++;
            }

            if (delta.Children.All(x => x.Kind == DeltaKind.Unchanged))
                delta.Children = null;
        }
        //If both sourceBlock and editBlock's children's keys are equal and in the same order, treat them all as matched.
        private void HandleCaseOfIdenticalKeys(Block sourceBlock, Block editBlock, List<(Node, Node)> matchedChildren)
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
                    sourceKeys.Add(namedBlock.Key);
                }
                if (editChild is BinaryExpression binExp)
                {
                    sourceKeys.Add(binExp.Key);
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
                            continue;
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
                            continue;
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
