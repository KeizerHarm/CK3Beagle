using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using CK3BeagleServer.Core.Generated;
using CK3BeagleServer.Core.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using static CK3BeagleServer.Core.CK3Parser;
using CK3BeagleServer.Core.Domain;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices.Marshalling;

namespace CK3BeagleServer.Core.Parsing.Antlr
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
                block.Scoper = context.SCOPER().ToString().StringToScoper();
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

        private readonly Regex blankLineRegex = new Regex("^\\s*\\n", RegexOptions.Multiline | RegexOptions.Compiled);
        public override void ExitComment([NotNull] CommentContext context)
        {
            ApplyRange(context, thisNonBlock);

            //Comment blocks currently can include blank lines. Need to split them manually.
            if (blankLineRegex.IsMatch(thisNonBlock.StringRepresentation))
            {
                var lines = thisNonBlock.StringRepresentation.Split('\n');

                var currentComment = thisNonBlock;
                var absStartPosition = thisNonBlock.Start;
                int prevLineLength = 0;
                int totalLength = 0;
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        if (currentComment != null)
                        {
                            var endPosition = new Position(
                               (absStartPosition.Line + i - 1),
                               prevLineLength,
                               totalLength + absStartPosition.Offset
                               );
                            currentComment.End = endPosition;
                            currentComment = null;
                        }
                    }
                    else {
                        if (currentComment == null)
                        {
                            currentComment = new Comment();
                            thisBlock.Peek().AddChild(currentComment);
                            var startPosition = new Position(
                               (absStartPosition.Line + i),
                               line.IndexOf('#'),
                               totalLength + line.IndexOf('#') + absStartPosition.Offset
                               );
                            currentComment.Start = startPosition;
                        }
                    }

                    prevLineLength = line.Length;
                    totalLength += line.Length + 1;
                }
                if (currentComment != null)
                {
                    var line = absStartPosition.Line + lines.Length - 1;
                    var column = prevLineLength;
                    var offset = totalLength + 1 + absStartPosition.Offset;
                    if (line == file.StringRepresentation.Count(x => x == '\n'))
                    {
                        offset -= 2;
                    }

                    var finalEndPosition = new Position(line, column, offset);
                    currentComment.End = finalEndPosition;
                }
                

            }

            thisNonBlock = null;
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
            thisBinExp.Scoper = context.SCOPER().ToString().StringToScoper();
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

            node.Start = new Position(startLine, startColumn, startOffset);

            var finalToken = context.Stop ?? context.Start;
            var endLine = finalToken.Line - 1;
            var endColumn = finalToken.Column + finalToken.Text.Length;
            var endOffset = finalToken.StopIndex + 1;

            node.End = new Position(endLine, endColumn, endOffset);
        }
    }
}
