using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using CK3Analyser.Core.Generated;
using CK3Analyser.Core.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using static CK3Analyser.Core.CK3Parser;

namespace CK3Analyser.Core.Parsing.Antlr
{
    public class ParsingListener : CK3BaseListener
    {
        public ScriptFile file;
        private readonly Stack<Block> thisBlock;

        public ParsingListener(ScriptFile file)
        {
            thisBlock = new Stack<Block>();
            this.file = file;
            thisBlock.Push(file);
        }

        public override void EnterNamedBlock([NotNull] NamedBlockContext context)
        {
            var key = context.identifier.GetText();
            if (thisBlock.Peek() == file)
            {
                var declarationType = file.ExpectedDeclarationType;
                if (file.ExpectedDeclarationType == DeclarationType.Event && thisBlock.Peek().Children.LastOrDefault()?.StringRepresentation == "scripted_trigger")
                {
                    declarationType = DeclarationType.ScriptedTrigger;
                }
                if (file.ExpectedDeclarationType == DeclarationType.Event && thisBlock.Peek().Children.LastOrDefault()?.StringRepresentation  == "scripted_effect")
                {
                    declarationType = DeclarationType.ScriptedEffect;
                }

                var declaration = new Declaration(key, declarationType);
                ApplyRange(context, declaration);
                file.RegisterDeclaration(declaration);
                thisBlock.Push(declaration);
            }
            else
            {
                var scoper = context.SCOPER().ToString();
                var block = new NamedBlock(key, scoper);
                ApplyRange(context, block);
                thisBlock.Peek().AddChild(block);
                thisBlock.Push(block);
            }
        }

        public override void ExitNamedBlock([NotNull] NamedBlockContext context)
        {
            thisBlock.Pop();
        }


        public override void EnterAnonymousBlock([NotNull] AnonymousBlockContext context)
        {
            var block = new AnonymousBlock();
            ApplyRange(context, block);
            thisBlock.Peek().AddChild(block);
            thisBlock.Push(block);
        }
        public override void ExitAnonymousBlock([NotNull] AnonymousBlockContext context)
        {
            thisBlock.Pop();
        }


        public override void EnterComment([NotNull] CommentContext context)
        {
            var comment = new Comment();
            ApplyRange(context, comment);
            thisBlock.Peek().AddChild(comment);
        }

        public override void ExitComment([NotNull] CommentContext context)
        {
            //No-op
        }


        public override void EnterBinaryExpression([NotNull] BinaryExpressionContext context)
        {
            var key = context.key.GetText();
            var value = context.value.GetText();
            var scoper = context.SCOPER().ToString();
            var binaryExpression = new BinaryExpression(key, scoper, value);
            ApplyRange(context, binaryExpression);
            thisBlock.Peek().AddChild(binaryExpression);
        }
        public override void ExitBinaryExpression([NotNull] BinaryExpressionContext context)
        {
            //No-op
        }


        public override void EnterAnonymousToken([NotNull] AnonymousTokenContext context)
        {
            var token = new AnonymousToken
            {
                Value = context.identifier.GetText()
            };
            ApplyRange(context, token);
            thisBlock.Peek().AddChild(token);
        }

        public override void ExitAnonymousToken([NotNull] AnonymousTokenContext context)
        {
            //No-op
        }

        //private string GetTokenText(TokenChainContext context)
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
