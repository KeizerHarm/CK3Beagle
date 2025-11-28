using CK3Analyser.Core.Comparing.Building;
using CK3Analyser.Core.Comparing.Domain;
using CK3Analyser.Core.Comparing.PreviousAttempts.Domain;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using System;
using System.Collections.Generic;

namespace CK3Analyser.Core.Comparing.PreviousAttempts.Building
{
    public class BlockComparisonBuilder
    {
        private ShadowNode Source;
        private Block Edit;
        private NodeMatcherOrig matcher;
        public List<IEditOperation> EditScript = [];

        public void BuildComparison(Block source, Block edit)
        {
            Source = new ShadowNode(source); Edit = edit;

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
            matcher = new NodeMatcherOrig();
            matcher.MatchAllNodes(Source, Edit);
        }

        private void UpdatePhase()
        {
            foreach ((ShadowNode sourceNode, Node editNode) in matcher.MatchedNodes)
            {
                if (editNode is BinaryExpression editBinExp && !sourceNode.StringRepresentation.Equals(editBinExp.StringRepresentation))
                {
                    EditScript.Add(new UpdateOperation(editNode, editBinExp.StringRepresentation));
                }
                else if (editNode is Comment editComment && !sourceNode.StringRepresentation.Equals(editComment.StringRepresentation))
                {
                    EditScript.Add(new UpdateOperation(editNode, editComment.StringRepresentation));
                }
                else if (editNode is AnonymousToken editAnonToken && !sourceNode.StringRepresentation.Equals(editAnonToken.Value))
                {
                    EditScript.Add(new UpdateOperation(editNode, editAnonToken.Value));
                }
                else if (editNode is NamedBlock editNamedBlock && !(sourceNode.StringRepresentation == editNamedBlock.Key + " " + editNamedBlock.Scoper.ScoperToString()))
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
                    var matchedParent = matcher.GetSourceMatchForEditNode(editNode.Parent);
                    EditScript.Add(new InsertOperation(editNode, editNode.Parent, 0));
                    var clone = new ShadowNode(editNode);
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
            void ifUnmatchedAdd(ShadowNode sourceNode)
            {
                if (!matcher.SourceNodeIsMatched(sourceNode))
                {
                    EditScript.Add(new DeleteOperation(sourceNode));
                }
            }

            new PostOrderShallowWalker(ifUnmatchedAdd).Walk(Source);
        }

    }
}
