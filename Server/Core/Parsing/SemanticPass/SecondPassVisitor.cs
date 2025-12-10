using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Generated;
using CK3BeagleServer.Core.Resources;
using System.Collections.Generic;

namespace CK3BeagleServer.Core.Parsing.SemanticPass
{
    public class SecondPassVisitor : BaseDomainVisitor
    {
        private static readonly HashSet<string> effectsThatCanContainEffects = 
        [
            "if", "else_if", "else", "while", 
            "hidden_effect", "hidden_effect_new_object",
            "send_interface_message", "send_interface_toast", "send_interface_popup",
            "random", "custom_tooltip", "custom_description_no_bullet", "custom_description"
        ];

        private static readonly HashSet<string> triggersThatCanContainTriggers = [
            "all_false", "calc_true_if", "or", "and", "nor", "nand", "not",
            "trigger_if", "trigger_else_if", "trigger_else"
        ];

        private static readonly HashSet<string> keysThatAreProbablyTriggers = [
            "limit", "trigger", "potential", "is_shown", "is_valid", "is_available"
        ];

        private static NodeType DeduceNodeType(Node node, string key, bool isBinExp)
        {
            key = key.ToLowerInvariant().Split(':', '.')[0];

            if (key == "T4N_story_content_enabled_for_jito_trigger")
            {

            }
            var parentKey = "";
            if (node.Parent != null && node.Parent is NamedBlock parentBlock
                && node.Parent is not Declaration)
            {
                parentKey = parentBlock.Key;
                if (GlobalResources.EFFECTKEYS.Contains(parentKey)
                    && (!(effectsThatCanContainEffects.Contains(parentKey)
                        || parentKey.StartsWith("ordered")
                        || parentKey.StartsWith("random")
                        || parentKey.StartsWith("every")
                        )))
                {
                    return NodeType.NonStatement;
                }
                if (GlobalResources.TRIGGERKEYS.Contains(parentKey)
                    && (!(triggersThatCanContainTriggers.Contains(parentKey)
                        || parentKey.StartsWith("any")
                        )))
                {
                    return NodeType.NonStatement;
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

                if (node.Parent != null)
                {
                    if (node.Parent.SymbolType.GetContextType() == BlockContext.Trigger
                        || node.Parent.NodeType == NodeType.Trigger
                        || keysThatAreProbablyTriggers.Contains(parentKey))
                        return NodeType.Trigger;
                    if (node.Parent.SymbolType.GetContextType() == BlockContext.Effect
                        || node.Parent.NodeType == NodeType.Effect)
                        return NodeType.Effect;
                }
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
