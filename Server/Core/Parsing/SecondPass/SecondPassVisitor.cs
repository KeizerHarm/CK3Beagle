using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources;
using System.Collections.Generic;

namespace CK3Analyser.Core.Parsing.SecondPass
{
    public class SecondPassVisitor : BaseDomainVisitor
    {
        private static readonly HashSet<string> statementsThatMayContainStatements = 
        [
            //Effects
            "if", "else_if", "else", "while", 
            "hidden_effect", "hidden_effect_new_object",
            "send_interface_message", "send_interface_toast", "send_interface_popup",
            "random", "custom_tooltip", "custom_description_no_bullet", "custom_description",

            //Triggers
            "all_false", "calc_true_if", "or", "and", "nor", "nand",
            "trigger_if", "trigger_else_if", "trigger_else"
        ];

        private static NodeType DeduceNodeType(Node node, string key)
        {
            key = key.ToLowerInvariant();
            if (node.Parent != null && node.Parent is NamedBlock parentBlock)
            {
                var parentKey = parentBlock.Key.ToLowerInvariant();
                if (parentBlock.NodeType == NodeType.Statement)
                {
                    if (!(statementsThatMayContainStatements.Contains(parentKey)
                        || parentKey.StartsWith("ordered")
                        || parentKey.StartsWith("random") 
                        || parentKey.StartsWith("every") 
                        || parentKey.StartsWith("any")
                        ))
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
