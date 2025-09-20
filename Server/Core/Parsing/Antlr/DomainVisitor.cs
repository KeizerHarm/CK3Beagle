using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using System.Linq;

namespace CK3Analyser.Core.Parsing.Antlr
{
    public class DomainVisitor : CK3BaseVisitor<Node>
    {
        private readonly string rawFile;
        private readonly Context domainContext;
        private readonly string relativePath;
        private readonly DeclarationType expectedDeclarationType;

        public DomainVisitor(string rawFile, Context context, string relativePath, DeclarationType expectedDeclarationType)
        {
            this.rawFile = rawFile;
            domainContext = context;
            this.relativePath = relativePath;
            this.expectedDeclarationType = expectedDeclarationType;
        }

        private string GetRawContents(ParserRuleContext context)
        {
            if (context == null)
                return "";

            int startIndex = context.Start.StartIndex;
            int endIndex = context?.Stop?.StopIndex ?? startIndex;
            return rawFile.Substring(startIndex, endIndex - startIndex);
        }

        public override ScriptFile VisitFile([NotNull] CK3Parser.FileContext context)
        {
            if (string.IsNullOrWhiteSpace(rawFile))
            {
                var file = new ScriptFile(domainContext, relativePath, expectedDeclarationType, rawFile);
                file.Raw = "";
            }

            return (ScriptFile)context.script().Accept(this);
        }

        public override ScriptFile VisitScript([NotNull] CK3Parser.ScriptContext context)
        {
            var file = new ScriptFile(domainContext, relativePath, expectedDeclarationType, rawFile);
            file.Raw = GetRawContents(context);

            HandleNamedBlocks(context, file);
            return file;
        }

        private static void HandleNamedBlocks(CK3Parser.ScriptContext context, ScriptFile file)
        {
            if (context.children == null)
                return;

            var namedBlocks = context.children.OfType<CK3Parser.NamedBlockContext>();
            foreach (var namedBlock in namedBlocks)
            {
                var key = namedBlock.token().GetText();
                var declarationType = file.ExpectedDeclarationType;

                var declaration = new Declaration(key, declarationType);
                declaration.Raw = namedBlock.ToString();
                file.AddDeclaration(declaration);
            }
        }
    }
}
