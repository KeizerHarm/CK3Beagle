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

        private static readonly HashSet<string> keysThatAreProbablyTriggers = [
            "limit", "trigger", "potential", "is_shown", "is_valid"
            ];

        private static NodeType DeduceNodeType(Node node, string key, bool isBinExp)
        {
            key = key.ToLowerInvariant().Split(':', '.')[0];
            var parentNodeType = node.Parent?.NodeType;

            if (node.Parent != null && node.Parent is NamedBlock parentBlock)
            {
                var parentKey = parentBlock.Key.ToLowerInvariant();
                if (parentBlock.NodeType != NodeType.NonStatement)
                {
                    if (!(statementsThatMayContainStatements.Contains(parentKey)
                        || parentKey.StartsWith("ordered")
                        || parentKey.StartsWith("random") 
                        || parentKey.StartsWith("every") 
                        || parentKey.StartsWith("any")
                        ))
                    {
                        return NodeType.NonStatement;
                    }
                }
                else {
                    if (keysThatAreProbablyTriggers.Contains(parentKey))
                        parentNodeType = NodeType.Trigger;
                }
            }

            if (GlobalResources.EFFECTKEYS.Contains(key))
                return NodeType.Effect;
            if (GlobalResources.TRIGGERKEYS.Contains(key))
                return NodeType.Trigger;

            //Links may be either
            if (GlobalResources.EVENTTARGETS.Contains(key))
            {
                //Binary expressions where the keys are links, are always triggers
                if (isBinExp)
                    return NodeType.Trigger;

                //Otherwise default to parent type
                if (node.Parent != null)
                    return node.Parent.NodeType;
            }

            return NodeType.NonStatement;
        }


        public override void Visit(BinaryExpression binaryExpression)
        {
            binaryExpression.NodeType = DeduceNodeType(binaryExpression, binaryExpression.Key, true);
            base.Visit(binaryExpression);
        }

        public override void Visit(NamedBlock namedBlock)
        {
            namedBlock.NodeType = DeduceNodeType(namedBlock, namedBlock.Key, false);
            base.Visit(namedBlock);
        }
    }
}
