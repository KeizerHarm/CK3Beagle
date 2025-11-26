using CK3Analyser.Core.Comparing.Domain;
using CK3Analyser.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Core.Comparing.Building
{
    public class NodeMatcher
    {
        public readonly Dictionary<ShallowNode, Node> MatchedNodes = [];

        public void AddMatch(ShallowNode @base, Node edit)
        {
            MatchedNodes.Add(@base, edit);
        }
        public bool BaseNodeIsMatched(ShallowNode baseNode) => MatchedNodes.ContainsKey(baseNode);
        public Node GetEditMatchForBaseNode(ShallowNode baseNode)
        {
            var success = MatchedNodes.TryGetValue(baseNode, out var match);
            return success ? match : null;
        }
        public bool EditNodeIsMatched(Node editNode) => MatchedNodes.Any(x => x.Value == editNode);
        public ShallowNode GetBaseMatchForEditNode(Node editNode)
        {
            return MatchedNodes.FirstOrDefault(x => x.Value == editNode).Key;
        }

        internal void MatchAllNodes(ShallowNode @base, Block edit)
        {
            var baseLeaves = GetLeaves(@base);
            var editLeaves = GetLeaves(edit);

            var matchedLeaves = new List<Tuple<ShallowNode, Node, float>>();

            foreach (var baseLeaf in baseLeaves)
            {
                foreach (var editLeaf in editLeaves)
                {
                    if (LeafMatches(baseLeaf, editLeaf, out float similarity))
                    {
                        matchedLeaves.Add(new Tuple<ShallowNode, Node, float>(baseLeaf, editLeaf, similarity));
                    }
                }
            }

            var orderedMatchedLeaves = matchedLeaves.OrderByDescending(x => x.Item3);

            foreach (var matchedPair in orderedMatchedLeaves)
            {
                if (!BaseNodeIsMatched(matchedPair.Item1))
                {
                    AddMatch(matchedPair.Item1, matchedPair.Item2);
                }
            }

            Action<ShallowNode> addIfMatch = (baseNode) =>
            {
                Action<Node> addIfMatch = (editNode) =>
                {
                    if (!BaseNodeIsMatched(baseNode))
                    {
                        if (Matches(baseNode, editNode))
                        {
                            AddMatch(baseNode, editNode);
                        }
                    }
                };
                new PostOrderWalker(addIfMatch).Walk(edit);
            };
            new PostOrderShallowWalker(addIfMatch).Walk(@base);
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

        private static List<ShallowNode> GetLeaves(ShallowNode node)
        {
            var leaves = new List<ShallowNode>();

            void addIfLeaf(ShallowNode node)
            {
                if (!node.OriginalType.IsAssignableTo(typeof(Block)))
                {
                    leaves.Add(node);
                }
            }

            new PostOrderShallowWalker(addIfLeaf).Walk(node);
            return leaves;
        }

        public bool Matches(ShallowNode baseNode, Node editNode)
        {
            if (baseNode.OriginalType != editNode.GetType())
                return false;

            if (editNode is Block editBlock)
                return BlockMatches(baseNode, editBlock);

            return LeafMatches(baseNode, editNode, out float _);
        }

        public bool LeafMatches(ShallowNode baseNode, Node editNode, out float similarity)
        {
            similarity = 0;
            if (baseNode.OriginalType != editNode.GetType())
                return false;

            if (editNode is Comment editComment)
            {
                return StringMatches(baseNode.StringRepresentation, editComment.RawWithoutHashtag, out similarity);
            }
            if (editNode is AnonymousToken editToken)
            {
                return StringMatches(baseNode.StringRepresentation, editToken.StringRepresentation, out similarity);
            }
            if (editNode is BinaryExpression editBinExp)
            {
                //Consider bin exps equal if keys are equal
                //Test to see if it produces sane results
                return baseNode.Key == editBinExp.Key;
            }
            throw new ArgumentException("Node type not recognised!");
        }

        public bool BlockMatches(ShallowNode baseBlock, Block editBlock)
        {
            if (baseBlock.OriginalType != editBlock.GetType())
                return false;

            if (editBlock is Declaration editDeclaration)
            {
                return baseBlock.Key == editDeclaration.Key
                    && baseBlock.DeclarationType == editDeclaration.DeclarationType;
            }
            if (editBlock is AnonymousBlock editAnonBlock)
            {
                return ChildrenMatch(baseBlock, editAnonBlock);
            }
            if (editBlock is NamedBlock editNamedBlock)
            {
                return baseBlock.Key == editNamedBlock.Key
                    && ChildrenMatch(baseBlock, editNamedBlock);
            }
            if (editBlock is ScriptFile editFile)
            {
                return true;
            }
            throw new ArgumentException("Node type not recognised!");
        }

        private bool ChildrenMatch(ShallowNode baseBlock, Block editBlock)
        {
            int commonNodes = GetSharedNodes(baseBlock, editBlock);
            var maxNoOfNodes = Math.Max(baseBlock.Children.Count, editBlock.Children.Count);

            var maxSize = Math.Max(baseBlock.GetSize(), editBlock.GetSize());
            if (maxSize <= 4)
            {
                return (float)commonNodes / maxNoOfNodes >= 0.4;
            }
            return (float)commonNodes / maxNoOfNodes >= 0.6;
        }

        private int GetSharedNodes(ShallowNode baseBlock, Block editBlock)
        {
            var commonNodes = 0;
            var baseChildren = baseBlock.ChildrenFlattened.ToArray();
            var editChildren = editBlock.ChildrenFlattened.ToArray();
            foreach (var child in baseBlock.ChildrenFlattened)
            {
                if (BaseNodeIsMatched(child) && editChildren.Contains(GetEditMatchForBaseNode(child)))
                {
                    commonNodes++;
                }
            }

            return commonNodes;
        }

        public static bool StringMatches(string baseString, string editString, out float similarity)
        {
            if (baseString == editString)
                similarity = 1;
            else
                similarity = CalcBigramSimilarity(baseString, editString);

            return similarity >= 0.6;
        }

        public static float CalcBigramSimilarity(string baseString, string editString)
        {
            if (baseString.Length <= 1 || editString.Length <= 1)
                return baseString == editString ? 1 : 0;

            var bigrams = new HashSet<string>();
            for (int i = 0; i < baseString.Length - 1; i++)
            {
                var substring = baseString.Substring(i, 2);
                bigrams.Add(substring);
            }

            int sharedBigramCount = 0;
            for (int i = 0; i < editString.Length - 1; i++)
            {
                var substring = editString.Substring(i, 2);
                if (!bigrams.Add(substring))
                {
                    sharedBigramCount++;
                }
            }

            var totalNoOfBigrams = baseString.Length - 1 + editString.Length - 1;

            return 2 * (float)sharedBigramCount / totalNoOfBigrams;
        }
    }
}
