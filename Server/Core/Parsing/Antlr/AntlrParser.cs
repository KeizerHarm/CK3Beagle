using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CK3Analyser.Core.Domain.Entities;

namespace CK3Analyser.Core.Parsing.Antlr
{
    public class AntlrParser : ICk3Parser
    {
        public void ParseFile(ScriptFile file)
        {
            var str = new AntlrInputStream(file.Raw);
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
            var listener = new ParsingVisitor(file);
            walker.Walk(listener, tree);
        }
    }
}
