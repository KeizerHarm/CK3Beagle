using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Generated;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Core.Comparing.Domain
{
    public class ShallowNode
    {
        public DeclarationType DeclarationType;
        public string Key;
        public Type OriginalType;
        public string StringRepresentation;
        public List<ShallowNode> Children = [];
        public List<ShallowNode> ChildrenFlattened
        {
            get
            {
                return Children.SelectMany(x => x.ChildrenFlattened).Union(Children).ToList();
            }
        }
        public int GetSize()
        {
            return 1 + Children.Sum(x => x.GetSize());
        }

        public ShallowNode(Node node)
        {
            OriginalType = node.GetType();
            if (node is BinaryExpression binExp)
            {
                Key = binExp.Key;
                StringRepresentation = binExp.StringRepresentation;
            }
            else if (node is Declaration declaration)
            {
                Key = declaration.Key;
                DeclarationType = declaration.DeclarationType;
                StringRepresentation = declaration.Key + " " + declaration.Scoper.ScoperToString();
                Children = declaration.Children.Select(x => new ShallowNode(x)).ToList();
            }
            else if (node is NamedBlock namedBlock)
            {
                Key = namedBlock.Key;
                StringRepresentation = namedBlock.Key + " " + namedBlock.Scoper.ScoperToString();
                Children = namedBlock.Children.Select(x => new ShallowNode(x)).ToList();
            }
            else if (node is Block block)
            {
                Children = block.Children.Select(x => new ShallowNode(x)).ToList();
            }
            else if (node is AnonymousToken anonymousToken)
            {
                StringRepresentation = anonymousToken.Value;
            }
            else if (node is Comment comment)
            {
                StringRepresentation = comment.StringRepresentation;
            }
            else
            {
                throw new Exception("Test");
            }
        }
    }
}
