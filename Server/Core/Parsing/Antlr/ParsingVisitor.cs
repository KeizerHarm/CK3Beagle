using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Core.Parsing.Antlr
{
    public class ParsingVisitor : CK3BaseListener
    {
        private readonly string rawFile;
        private readonly Context domainContext;
        private readonly string relativePath;
        private readonly DeclarationType expectedDeclarationType;
        public ScriptFile file;
        private Stack<Block> thisBlock;

        public ParsingVisitor(string rawFile, Context domainContext, string relativePath, DeclarationType expectedDeclarationType)
        {
            this.rawFile = rawFile;
            this.domainContext = domainContext;
            this.relativePath = relativePath;
            this.expectedDeclarationType = expectedDeclarationType;
            thisBlock = new Stack<Block>();
        }

        public override void EnterFile([NotNull] CK3Parser.FileContext context)
        {
            var file = new ScriptFile(domainContext, relativePath, expectedDeclarationType, rawFile);
            this.file = file;
            thisBlock.Push(file);
        }

        public override void ExitFile([NotNull] CK3Parser.FileContext context)
        {
            thisBlock.Pop();
        }

        public override void EnterNamedBlock([NotNull] CK3Parser.NamedBlockContext context)
        {
            var key = context.token().GetText();
            var raw = GetRawContents(context);
            if (thisBlock.Peek() == file)
            {
                var declarationType = file.ExpectedDeclarationType;
                if (file.ExpectedDeclarationType == DeclarationType.Event && thisBlock.Peek().Children.LastOrDefault()?.Raw == "scripted_trigger")
                {
                    declarationType = DeclarationType.ScriptedTrigger;
                }
                if (file.ExpectedDeclarationType == DeclarationType.Event && thisBlock.Peek().Children.LastOrDefault()?.Raw  == "scripted_effect")
                {
                    declarationType = DeclarationType.ScriptedEffect;
                }

                var declaration = new Declaration(key, declarationType);
                declaration.Raw = raw;
                file.AddDeclaration(declaration);
                thisBlock.Push(declaration);
            }
            else
            {
                var block = new NamedBlock(key)
                {
                    Raw = raw
                };
                thisBlock.Peek().AddChild(block);
                thisBlock.Push(block);
            }
        }

        public override void ExitNamedBlock([NotNull] CK3Parser.NamedBlockContext context)
        {
            thisBlock.Pop();
        }


        public override void EnterAnonymousBlock([NotNull] CK3Parser.AnonymousBlockContext context)
        {
            var raw = GetRawContents(context);
            var block = new AnonymousBlock()
            {
                Raw = raw
            };
            thisBlock.Peek().AddChild(block);
            thisBlock.Push(block);
        }
        public override void ExitAnonymousBlock([NotNull] CK3Parser.AnonymousBlockContext context)
        {
            thisBlock.Pop();
        }


        public override void EnterComment([NotNull] CK3Parser.CommentContext context)
        {
            var raw = GetRawContents(context);
            var rawWithoutHashtag = raw.Split('#')[1].Trim();
            var comment = new Comment()
            {
                Raw = raw,
                RawWithoutHashtag = rawWithoutHashtag
            };
            thisBlock.Peek().AddChild(comment);
        }

        public override void ExitComment([NotNull] CK3Parser.CommentContext context)
        {
            //No-op
        }


        public override void EnterBinaryExpression([NotNull] CK3Parser.BinaryExpressionContext context)
        {
            var raw = GetRawContents(context);
            var key = GetRawContents(context.token(0));
            var value = GetRawContents(context.token(1));
            var scoper = context.SCOPER().ToString();
            var binaryExpression = new BinaryExpression(key, scoper, value) { Raw = raw };
            thisBlock.Peek().AddChild(binaryExpression);
        }
        public override void ExitBinaryExpression([NotNull] CK3Parser.BinaryExpressionContext context)
        {
            //No-op
        }


        public override void EnterAnonymousToken([NotNull] CK3Parser.AnonymousTokenContext context)
        {
            var token = new AnonymousToken
            {
                Raw = GetRawContents(context),
                Value = GetRawContents(context.token())
            };
            thisBlock.Peek().AddChild(token);
        }

        public override void ExitAnonymousToken([NotNull] CK3Parser.AnonymousTokenContext context)
        {
            //No-op
        }

        private string GetRawContents(ParserRuleContext context)
        {
            if (context == null)
                return "";

            int startIndex = context.Start.StartIndex;
            int endIndex = startIndex;
            if(context?.Stop != null)
            {
                endIndex = context.Stop.StopIndex + 1;
            }
            return rawFile.Substring(startIndex, endIndex - startIndex);
        }
    }
}
