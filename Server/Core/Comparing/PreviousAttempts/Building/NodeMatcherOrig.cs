using CK3Analyser.Core.Comparing.Building;
using CK3Analyser.Core.Comparing.Domain;
using CK3Analyser.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Core.Comparing.PreviousAttempts.Building
{
    public class NodeMatcherOrig
    {
        public readonly Dictionary<ShadowNode, Node> MatchedNodes = [];

        public void AddMatch(ShadowNode source, Node edit)
        {
            MatchedNodes.Add(source, edit);
        }
        public bool SourceNodeIsMatched(ShadowNode sourceNode) => MatchedNodes.ContainsKey(sourceNode);
        public Node GetEditMatchForSourceNode(ShadowNode sourceNode)
        {
            var success = MatchedNodes.TryGetValue(sourceNode, out var match);
            return success ? match : null;
        }
        public bool EditNodeIsMatched(Node editNode) => MatchedNodes.Any(x => x.Value == editNode);
        public ShadowNode GetSourceMatchForEditNode(Node editNode)
        {
            return MatchedNodes.FirstOrDefault(x => x.Value == editNode).Key;
        }

        internal void MatchAllNodes(ShadowNode source, Block edit)
        {
            var sourceLeaves = GetLeaves(source);
            var editLeaves = GetLeaves(edit);

            var matchedLeaves = new List<Tuple<ShadowNode, Node, float>>();

            foreach (var sourceLeaf in sourceLeaves)
            {
                foreach (var editLeaf in editLeaves)
                {
                    if (LeafMatches(sourceLeaf, editLeaf, out float similarity))
                    {
                        matchedLeaves.Add(new Tuple<ShadowNode, Node, float>(sourceLeaf, editLeaf, similarity));
                    }
                }
            }

            var orderedMatchedLeaves = matchedLeaves.OrderByDescending(x => x.Item3);

            foreach (var matchedPair in orderedMatchedLeaves)
            {
                if (!SourceNodeIsMatched(matchedPair.Item1))
                {
                    AddMatch(matchedPair.Item1, matchedPair.Item2);
                }
            }

            Action<ShadowNode> addIfMatch = (sourceNode) =>
            {
                Action<Node> addIfMatch = (editNode) =>
                {
                    if (!SourceNodeIsMatched(sourceNode))
                    {
                        if (Matches(sourceNode, editNode))
                        {
                            AddMatch(sourceNode, editNode);
                        }
                    }
                };
                new PostOrderWalker(addIfMatch).Walk(edit);
            };
            new PostOrderShallowWalker(addIfMatch).Walk(source);
        }

        private static List<Node> GetLeaves(Node node)
        {
            var leaves = new List<Node>();

            void addIfLeaf(Node node)
            {
                if (node is not Block _)
                {
                    leaves.Add(node);
                }
            }

            new PostOrderWalker(addIfLeaf).Walk(node);
            return leaves;
        }

        private static List<ShadowNode> GetLeaves(ShadowNode node)
        {
            var leaves = new List<ShadowNode>();

            void addIfLeaf(ShadowNode node)
            {
                if (!node.OriginalType.IsAssignableTo(typeof(Block)))
                {
                    leaves.Add(node);
                }
            }

            new PostOrderShallowWalker(addIfLeaf).Walk(node);
            return leaves;
        }

        public bool Matches(ShadowNode sourceNode, Node editNode)
        {
            if (sourceNode.OriginalType != editNode.GetType())
                return false;

            if (editNode is Block editBlock)
                return BlockMatches(sourceNode, editBlock);

            return LeafMatches(sourceNode, editNode, out float _);
        }

        public bool LeafMatches(ShadowNode sourceNode, Node editNode, out float similarity)
        {
            similarity = 0;
            if (sourceNode.OriginalType != editNode.GetType())
                return false;

            if (editNode is Comment editComment)
            {
                return StringMatches(sourceNode.StringRepresentation, editComment.StringRepresentation, out similarity);
            }
            if (editNode is AnonymousToken editToken)
            {
                return StringMatches(sourceNode.StringRepresentation, editToken.StringRepresentation, out similarity);
            }
            if (editNode is BinaryExpression editBinExp)
            {
                //Consider bin exps equal if keys are equal
                //Test to see if it produces sane results
                return sourceNode.Key == editBinExp.Key
                    && StringMatches(sourceNode.StringRepresentation, editBinExp.StringRepresentation, out similarity);
            }
            throw new ArgumentException("Node type not recognised!");
        }

        public bool BlockMatches(ShadowNode sourceBlock, Block editBlock)
        {
            if (sourceBlock.OriginalType != editBlock.GetType())
                return false;

            if (editBlock is Declaration editDeclaration)
            {
                return sourceBlock.Key == editDeclaration.Key
                    && sourceBlock.DeclarationType == editDeclaration.DeclarationType;
            }
            if (editBlock is AnonymousBlock editAnonBlock)
            {
                return ChildrenMatch(sourceBlock, editAnonBlock);
            }
            if (editBlock is NamedBlock editNamedBlock)
            {
                return sourceBlock.Key == editNamedBlock.Key
                    && ChildrenMatch(sourceBlock, editNamedBlock);
            }
            if (editBlock is ScriptFile editFile)
            {
                return true;
            }
            throw new ArgumentException("Node type not recognised!");
        }

        private bool ChildrenMatch(ShadowNode sourceBlock, Block editBlock)
        {
            int commonNodes = GetSharedNodes(sourceBlock, editBlock);
            var maxNoOfNodes = Math.Max(sourceBlock.Children.Count, editBlock.Children.Count);

            var maxSize = Math.Max(sourceBlock.GetSize(), editBlock.GetSize());
            if (maxSize <= 4)
            {
                return (float)commonNodes / maxNoOfNodes >= 0.4;
            }
            return (float)commonNodes / maxNoOfNodes >= 0.6;
        }

        private int GetSharedNodes(ShadowNode sourceBlock, Block editBlock)
        {
            var commonNodes = 0;
            var sourceChildren = sourceBlock.ChildrenFlattened.ToArray();
            var editChildren = editBlock.ChildrenFlattened.ToArray();
            foreach (var child in sourceBlock.ChildrenFlattened)
            {
                if (SourceNodeIsMatched(child) && editChildren.Contains(GetEditMatchForSourceNode(child)))
                {
                    commonNodes++;
                }
            }

            return commonNodes;
        }

        public static bool StringMatches(string sourceString, string editString, out float similarity)
        {
            if (sourceString == editString)
                similarity = 1;
            else
                similarity = ComparisonHelpers.CalcBigramSimilarity(sourceString, editString);

            return similarity >= 0.6;
        }
    }
}
