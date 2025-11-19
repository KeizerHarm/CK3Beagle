using CK3Analyser.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Analysis.Comparing.Building
{
    public class ComparisonBuilder
    {
        private Node Base;
        private Node Edit;
        private Dictionary<int, (Node, Node)> MatchedNodes = [];
        private List<EditOperation> EditScript = [];

        public void BuildComparison(Node @base, Node edit)
        {
            Base = @base; Edit = edit;

            FindMatchedNodes();
            GenerateEditScript();
        }

        private void GenerateEditScript()
        {
            UpdatePhase();
            AlignPhase();
            InsertPhase();
            MovePhase();
            DeletePhase();
        }

        private void UpdatePhase()
        {
            foreach ((Node baseNode, Node editNode) in MatchedNodes.Values)
            {
                if (baseNode is BinaryExpression baseBinExp && editNode is BinaryExpression editBinExp && !baseBinExp.Value.Equals(editBinExp.Value))
                {
                    EditScript.Add(new UpdateOperation
                    {
                        NewValue = editBinExp.Value,
                        UpdatedNode = editNode
                    });
                }
                else if (baseNode is Comment baseComment && editNode is Comment editComment && !baseComment.RawWithoutHashtag.Equals(editComment.RawWithoutHashtag))
                {
                    EditScript.Add(new UpdateOperation
                    {
                        NewValue = editComment.RawWithoutHashtag,
                        UpdatedNode = editNode
                    });
                }
                else if (baseNode is AnonymousToken baseAnonToken && editNode is AnonymousToken editAnonToken && !baseAnonToken.Value.Equals(editAnonToken.Value))
                {
                    EditScript.Add(new UpdateOperation
                    {
                        NewValue = editAnonToken.Value,
                        UpdatedNode = editNode
                    });
                }
            }
        }

        private void AlignPhase()
        {
            throw new NotImplementedException();
        }

        private void InsertPhase()
        {
            //var leaves = new List<Node>();

            //void addIfLeaf(Node node)
            //{
                
            //}

            //new PostOrderWalker(addIfLeaf).Walk(Edit);
        }

        private void MovePhase()
        {
            throw new NotImplementedException();
        }

        private void DeletePhase()
        {
            void deleteIfUnmatched(Node node)
            {
                if (!MatchedNodes.ContainsKey(node.GetHashCode()))
                {
                    EditScript.Add(new DeleteOperation
                    {
                        DeletedNode = node
                    });
                }
            }

            new PostOrderWalker(deleteIfUnmatched).Walk(Edit);
        }

        public void FindMatchedNodes()
        {
            var baseLeaves = GetLeaves(Base);
            var editLeaves = GetLeaves(Edit);

            var matchedLeaves = new List<Tuple<Node, Node, float>>();

            foreach (var baseLeaf in baseLeaves)
            {
                foreach (var editLeaf in editLeaves)
                {
                    if (LeafMatches(baseLeaf, editLeaf, out float similarity))
                    {
                        matchedLeaves.Add(new Tuple<Node, Node, float>(baseLeaf, editLeaf, similarity));
                    }
                }
            }

            var orderedMatchedLeaves = matchedLeaves.OrderByDescending(x => x.Item3);

            foreach (var matchedPair in orderedMatchedLeaves)
            {
                if (!MatchedNodes.ContainsKey(matchedPair.Item1.GetHashCode()))
                {
                    MatchedNodes.Add(matchedPair.Item1.GetHashCode(), (matchedPair.Item1, matchedPair.Item2));
                }
            }


            Action<Node> addIfMatch = (baseNode) =>
            {
                Action<Node> addIfMatch = (editNode) =>
                {
                    if (!MatchedNodes.ContainsKey(baseNode.GetHashCode()))
                    {
                        MatchedNodes.Add(baseNode.GetHashCode(), (baseNode, editNode));
                    }
                };
                new PostOrderWalker(addIfMatch).Walk(Edit);
            };
            new PostOrderWalker(addIfMatch).Walk(Base);
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

        public bool Matches(Node baseNode, Node editNode)
        {
            if (baseNode.GetType() != editNode.GetType())
                return false;

            if (baseNode is Block baseBlock && editNode is Block editBlock)
                return BlockMatches(baseBlock, editBlock);

            return LeafMatches(baseNode, editNode, out float _);
        }

        public bool LeafMatches(Node baseNode, Node editNode, out float similarity)
        {
            similarity = 0;
            if (baseNode.GetType() != editNode.GetType())
                return false;

            if (baseNode is Comment baseComment && editNode is Comment editComment)
            {
                return StringMatches(baseComment.RawWithoutHashtag, editComment.RawWithoutHashtag, out similarity);
            }
            if (baseNode is AnonymousToken baseToken && editNode is AnonymousToken editToken)
            {
                return StringMatches(baseToken.StringRepresentation, editToken.StringRepresentation, out similarity);
            }
            if (baseNode is BinaryExpression baseBinExp && editNode is BinaryExpression editBinExp)
            {
                return baseBinExp.Key == editBinExp.Key
                    && StringMatches(baseBinExp.Value, editBinExp.Value, out similarity);
            }
            throw new ArgumentException("Node type not recognised!");
        }

        public bool BlockMatches(Block baseBlock, Block editBlock)
        {
            if (baseBlock.GetType() != editBlock.GetType())
                return false;

            if (baseBlock is AnonymousBlock baseAnonBlock && editBlock is AnonymousBlock editAnonBlock)
            {
                return ChildrenMatch(baseAnonBlock, editAnonBlock);
            }
            if (baseBlock is NamedBlock baseNamedBlock && editBlock is NamedBlock editNamedBlock)
            {
                return baseNamedBlock.Key == editNamedBlock.Key
                    && ChildrenMatch(baseNamedBlock, editNamedBlock);
            }
            if (baseBlock is Declaration baseDeclaration && editBlock is Declaration editDeclaration)
            {
                return baseDeclaration.Key == editDeclaration.Key
                    && baseDeclaration.DeclarationType == editDeclaration.DeclarationType;
            }
            if (baseBlock is ScriptFile baseFile && editBlock is ScriptFile editFile)
            {
                return baseFile.RelativePath == editFile.RelativePath;
            }
            throw new ArgumentException("Node type not recognised!");
        }

        private bool ChildrenMatch(Block baseBlock, Block editBlock)
        {
            var commonNodes = 0;
            foreach (var child in baseBlock.Children)
            {
                if (MatchedNodes.TryGetValue(child.GetHashCode(), out (Node, Node) pair) && pair.Item2.Parent == editBlock)
                {
                    commonNodes++;
                }
            }
            var maxNoOfNodes = Math.Max(baseBlock.Children.Count, editBlock.Children.Count);

            var maxSize = Math.Max(baseBlock.GetSize(), editBlock.GetSize());
            if (maxSize <= 4)
            {
                return (float)commonNodes / maxNoOfNodes >= 0.4;
            }
            return (float)commonNodes / maxNoOfNodes >= 0.6;
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
