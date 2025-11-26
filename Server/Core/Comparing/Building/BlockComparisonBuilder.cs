using CK3Analyser.Core.Comparing.Domain;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace CK3Analyser.Core.Comparing.Building
{
    public class BlockComparisonBuilder
    {
        private ShallowNode Base;
        private Block Edit;
        private NodeMatcher matcher;
        public List<IEditOperation> EditScript = [];

        public void BuildComparison(Block @base, Block edit)
        {
            Base = new ShallowNode(@base); Edit = edit;

            FindMatchedNodes();
            GenerateEditScript();
        }

        private void GenerateEditScript()
        {
            UpdatePhase();
            //AlignPhase();
            InsertPhase();
            //MovePhase();
            //DeletePhase();
        }

        public void FindMatchedNodes()
        {
            matcher = new NodeMatcher();
            matcher.MatchAllNodes(Base, Edit);
        }

        private void UpdatePhase()
        {
            foreach ((ShallowNode baseNode, Node editNode) in matcher.MatchedNodes)
            {
                if (editNode is BinaryExpression editBinExp && !baseNode.StringRepresentation.Equals(editBinExp.StringRepresentation))
                {
                    EditScript.Add(new UpdateOperation(editNode, editBinExp.StringRepresentation));
                }
                else if (editNode is Comment editComment && !baseNode.StringRepresentation.Equals(editComment.RawWithoutHashtag))
                {
                    EditScript.Add(new UpdateOperation(editNode, editComment.RawWithoutHashtag));
                }
                else if (editNode is AnonymousToken editAnonToken && !baseNode.StringRepresentation.Equals(editAnonToken.Value))
                {
                    EditScript.Add(new UpdateOperation(editNode, editAnonToken.Value));
                }
                else if (editNode is NamedBlock editNamedBlock && !(baseNode.StringRepresentation == editNamedBlock.Key + " " + editNamedBlock.Scoper.ScoperToString()))
                {
                    EditScript.Add(new UpdateOperation(editNode, editNamedBlock.Key + " " + editNamedBlock.Scoper.ScoperToString()));
                }
            }
        }

        private void AlignPhase()
        {
            throw new NotImplementedException();
        }

        private void InsertPhase()
        {
            void ifUnmatchedAdd(Node editNode)
            {
                if (!matcher.EditNodeIsMatched(editNode)
                    && matcher.EditNodeIsMatched(editNode.Parent))
                {
                    var matchedParent = matcher.GetBaseMatchForEditNode(editNode.Parent);
                    EditScript.Add(new InsertOperation(editNode, editNode.Parent, 0));
                    var clone = new ShallowNode(editNode);
                    matchedParent.Children.Add(clone);
                    matcher.AddMatch(clone, editNode);
                }
            }

            new PostOrderWalker(ifUnmatchedAdd).Walk(Edit);
        }

        private void MovePhase()
        {
            throw new NotImplementedException();
        }

        private void DeletePhase()
        {
            //void deleteIfUnmatched(Node node)
            //{
            //    if (!matcher.MatchedNodes.ContainsKey(node.GetTrueHash()))
            //    {
            //        EditScript.Add(new DeleteOperation(node));
            //    }
            //}

            //new PostOrderWalker(deleteIfUnmatched).Walk(Edit);
        }

    }
}
