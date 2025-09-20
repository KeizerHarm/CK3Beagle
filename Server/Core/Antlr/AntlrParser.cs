using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using System.IO;

namespace CK3Analyser.Core.Antlr
{
    public class AntlrParser : ICk3Parser
    {
        public ScriptFile ParseFile(string path, Context context, DeclarationType expectedDeclarationType)
        {
            var relativePath = Path.GetRelativePath(context.Path, path);

            var input = File.ReadAllText(path);
            return ParseText(input, relativePath, context, expectedDeclarationType);
        }

        public ScriptFile ParseText(string input, string relativePath, Context context, DeclarationType expectedDeclarationType)
        {
            var str = new AntlrInputStream(input);
            var lexer = new CK3Lexer(str);
            var tokens = new CommonTokenStream(lexer);
            var parser = new CK3Parser(tokens);
            //var listener_lexer = new ErrorListener<int>();
            //var listener_parser = new ErrorListener<IToken>();
            lexer.RemoveErrorListeners();
            parser.RemoveErrorListeners();
            //lexer.AddErrorListener(listener_lexer);
            //parser.AddErrorListener(listener_parser);
            var tree = parser.file();
            var walker = new ParseTreeWalker();
            var listener = new DomainListener(input, context, relativePath, expectedDeclarationType);
            walker.Walk(listener, tree);
            var parsedFile = listener.file;

            //var visitor = new DomainVisitor(input, context, relativePath, expectedDeclarationType);
            //var parsedFile = visitor.VisitFile(tree);

            return parsedFile;
        }
    }
}
