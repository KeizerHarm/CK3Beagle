using CK3Analyser.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Core.Comparing.Building
{
    public class NodeMatcher
    {
        public readonly Dictionary<Node, Node> MatchedNodes = [];
        public readonly Dictionary<Node, Node> MatchedNodesInv = [];

        public void AddMatch(Node source, Node edit)
        {
            MatchedNodes.Add(source, edit);
            MatchedNodesInv.Add(edit, source);
        }
        public bool SourceNodeIsMatched(Node sourceNode) => MatchedNodes.ContainsKey(sourceNode);
        public Node GetEditMatchForSourceNode(Node sourceNode)
        {
            var success = MatchedNodes.TryGetValue(sourceNode, out var match);
            return success ? match : null;
        }
        public bool EditNodeIsMatched(Node editNode) => MatchedNodesInv.ContainsKey(editNode);
        public Node GetSourceMatchForEditNode(Node editNode)
        {
            var success = MatchedNodesInv.TryGetValue(editNode, out var match);
            return success ? match : null;
        }

        internal void MatchAllNodes(Block source, Block edit)
        {
            var sourceLeaves = GetLeaves(source);
            var editLeaves = GetLeaves(edit);

            var matchedLeaves = new List<Tuple<Node, Node, float>>();

            foreach (var sourceLeaf in sourceLeaves)
            {
                foreach (var editLeaf in editLeaves)
                {
                    if (LeafMatches(sourceLeaf, editLeaf, out float similarity))
                    {
                        matchedLeaves.Add(new Tuple<Node, Node, float>(sourceLeaf, editLeaf, similarity));
                    }
                }
            }

            var orderedMatchedLeaves = matchedLeaves.OrderByDescending(x => x.Item3);

            foreach (var matchedPair in orderedMatchedLeaves)
            {
                if (!SourceNodeIsMatched(matchedPair.Item1) && !EditNodeIsMatched(matchedPair.Item2))
                {
                    AddMatch(matchedPair.Item1, matchedPair.Item2);
                }
            }

            Action<Node> addIfMatch = (sourceNode) =>
            {
                Action<Node> addIfMatch = (editNode) =>
                {
                    if (!SourceNodeIsMatched(sourceNode) && !EditNodeIsMatched(editNode))
                    {
                        if (Matches(sourceNode, editNode))
                        {
                            AddMatch(sourceNode, editNode);
                        }
                    }
                };
                new PostOrderWalker(addIfMatch).Walk(edit);
            };
            new PostOrderWalker(addIfMatch).Walk(source);
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

        public bool Matches(Node sourceNode, Node editNode)
        {
            if (sourceNode.GetType() != editNode.GetType())
                return false;

            if (editNode is Block editBlock)
                return BlockMatches(sourceNode as Block, editBlock);

            return LeafMatches(sourceNode, editNode, out float _);
        }

        public bool LeafMatches(Node sourceNode, Node editNode, out float similarity)
        {
            similarity = 0;
            if (sourceNode.GetType() != editNode.GetType())
                return false;

            if (editNode is Comment editComment)
            {
                return StringMatches(((Comment)sourceNode).RawWithoutHashtag, editComment.RawWithoutHashtag, out similarity);
            }
            if (editNode is AnonymousToken editToken)
            {
                return StringMatches(sourceNode.StringRepresentation, editToken.StringRepresentation, out similarity);
            }
            if (editNode is BinaryExpression editBinExp)
            {
                //Consider bin exps equal if keys are equal
                //Test to see if it produces sane results
                return ((BinaryExpression)sourceNode).Key == editBinExp.Key;
            }
            throw new ArgumentException("Node type not recognised!");
        }

        public bool BlockMatches(Block sourceBlock, Block editBlock)
        {
            if (sourceBlock.GetType() != editBlock.GetType())
                return false;

            if (editBlock is Declaration editDeclaration)
            {
                return ((Declaration)sourceBlock).Key == editDeclaration.Key
                    && ((Declaration)sourceBlock).DeclarationType == editDeclaration.DeclarationType;
            }
            if (editBlock is AnonymousBlock editAnonBlock)
            {
                return ChildrenMatch(sourceBlock, editAnonBlock);
            }
            if (editBlock is NamedBlock editNamedBlock)
            {
                return ((NamedBlock)sourceBlock).Key == editNamedBlock.Key
                    && ChildrenMatch(sourceBlock, editNamedBlock);
            }
            if (editBlock is ScriptFile editFile)
            {
                return true;
            }
            throw new ArgumentException("Node type not recognised!");
        }

        private bool ChildrenMatch(Block sourceBlock, Block editBlock)
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

        private int GetSharedNodes(Block sourceBlock, Block editBlock)
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
