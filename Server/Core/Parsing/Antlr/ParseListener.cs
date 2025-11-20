using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using CK3Analyser.Core.Generated;
using CK3Analyser.Core.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static CK3Analyser.Core.CK3Parser;

namespace CK3Analyser.Core.Parsing.Antlr
{
    public class ParseListener : CK3BaseListener
    {
        public ScriptFile file;
        private readonly Stack<Block> thisBlock;
        private Node thisNonBlock;

        public ParseListener(ScriptFile file)
        {
            thisBlock = new Stack<Block>();
            this.file = file;
            thisBlock.Push(file);
        }

        public override void EnterNamedBlock([NotNull] NamedBlockContext context)
        {
            var key = context.Start.Text;
            if (thisBlock.Peek() == file)
            {
                var declarationType = file.ExpectedDeclarationType;
                if (file.ExpectedDeclarationType == DeclarationType.Event && thisBlock.Peek().Children.LastOrDefault()?.StringRepresentation == "scripted_trigger")
                {
                    declarationType = DeclarationType.ScriptedTrigger;
                }
                if (file.ExpectedDeclarationType == DeclarationType.Event && thisBlock.Peek().Children.LastOrDefault()?.StringRepresentation == "scripted_effect")
                {
                    declarationType = DeclarationType.ScriptedEffect;
                }

                var declaration = new Declaration(key, declarationType);
                thisBlock.Peek().AddChild(declaration);
                thisBlock.Push(declaration);
            }
            else
            {
                var block = new NamedBlock(key);
                thisBlock.Peek().AddChild(block);
                thisBlock.Push(block);
            }
        }

        public override void ExitNamedBlock([NotNull] NamedBlockContext context)
        {
            var thisNamedBlock = thisBlock.Peek();
            ApplyRange(context, thisNamedBlock);
            if (thisNamedBlock is Declaration decl)
            {
                decl.Key = GetTokenText(context.identifier);
                file.RegisterDeclaration(decl);
            }
            else if (thisNamedBlock is NamedBlock block)
            {
                block.Scoper = context.SCOPER().ToString();
                block.Key = GetTokenText(context.identifier);
            }
            thisBlock.Pop();
        }

        public override void EnterAnonymousBlock([NotNull] AnonymousBlockContext context)
        {
            var block = new AnonymousBlock();
            thisBlock.Peek().AddChild(block);
            thisBlock.Push(block);
        }
        public override void ExitAnonymousBlock([NotNull] AnonymousBlockContext context)
        {
            ApplyRange(context, thisBlock.Peek());
            thisBlock.Pop();
        }

        public override void EnterComment([NotNull] CommentContext context)
        {
            var comment = new Comment();
            thisNonBlock = comment;
            thisBlock.Peek().AddChild(comment);
        }

        public override void ExitComment([NotNull] CommentContext context)
        {
            ApplyRange(context, thisNonBlock);
            thisNonBlock = null;
            //No-op
        }

        private string GetTokenText(TokenChainContext context)
        {
            if (context?.Start == null)
                return "";

            if (context.Start == context.Stop)
                return context.Start.Text;

            return string.Join("", context.Start.Text, context.GetText(), context.Stop?.Text);
        }


        public override void EnterBinaryExpression([NotNull] BinaryExpressionContext context)
        {
            var binaryExpression = new BinaryExpression();
            thisBlock.Peek().AddChild(binaryExpression);
            thisNonBlock = binaryExpression;
        }
        public override void ExitBinaryExpression([NotNull] BinaryExpressionContext context)
        {
            var thisBinExp = (BinaryExpression)thisNonBlock;
            ApplyRange(context, thisBinExp);
            thisBinExp.Scoper = context.SCOPER().ToString();
            thisBinExp.Value = GetTokenText(context.value);
            thisBinExp.Key = GetTokenText(context.key);
            thisNonBlock = null;
        }


        public override void EnterAnonymousToken([NotNull] AnonymousTokenContext context)
        {
            var token = new AnonymousToken();
            thisNonBlock = token;
            ApplyRange(context, token);
            thisBlock.Peek().AddChild(token);
        }

        public override void ExitAnonymousToken([NotNull] AnonymousTokenContext context)
        {
            ((AnonymousToken)thisNonBlock).Value = GetTokenText(context.identifier);
            ApplyRange(context, thisNonBlock);
            thisNonBlock = null;
            //No-op
        }

        //private string GetTokenText(TokenContext context)
        //{
        //    if (context?.Start == null)
        //        return "";
        //    return context.GetText();
        //}

        private void ApplyRange(ParserRuleContext context, Node node)
        {
            var startLine = context.Start.Line - 1;
            var startColumn = context.Start.Column;
            var startOffset = context.Start.StartIndex;

            node.Start = new Position
            {
                Line = startLine,
                Column = startColumn,
                Offset = startOffset
            };

            var finalToken = context.Stop ?? context.Start;
            var endLine = finalToken.Line - 1;
            var endColumn = finalToken.Column + finalToken.Text.Length;
            var endOffset = finalToken.StopIndex + 1;

            node.End = new Position
            {
                Line = endLine,
                Column = endColumn,
                Offset = endOffset
            };
        }
    }
}
