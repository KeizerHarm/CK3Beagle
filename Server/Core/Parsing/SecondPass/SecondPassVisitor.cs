using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources;

namespace CK3Analyser.Core.Parsing.SecondPass
{
    public class SecondPassVisitor : BaseDomainVisitor
    {
        private static NodeType DeduceNodeType(Node node, string key)
        {
            if (node.Parent != null && node.Parent is NamedBlock parentBlock)
            {
                var parentKey = parentBlock.Key;
                if (parentBlock.NodeType == NodeType.Statement)
                {
                    if (!(parentKey.StartsWith("random") || parentKey.StartsWith("every") || parentKey.StartsWith("any")))
                    {
                        return NodeType.Other;
                    }
                }
            }

            if (key.Contains(':'))
                return NodeType.Link;

            if (GlobalResources.EFFECTKEYS.Contains(key))
                return NodeType.Statement;
            if (GlobalResources.TRIGGERKEYS.Contains(key))
                return NodeType.Statement;
            if (GlobalResources.EVENTTARGETS.Contains(key))
                return NodeType.Link;


            return NodeType.Other;
        }


        public override void Visit(BinaryExpression binaryExpression)
        {
            binaryExpression.NodeType = DeduceNodeType(binaryExpression, binaryExpression.Key);
            base.Visit(binaryExpression);
        }

        public override void Visit(NamedBlock namedBlock)
        {
            namedBlock.NodeType = DeduceNodeType(namedBlock, namedBlock.Key);
            base.Visit(namedBlock);
        }
    }
}
