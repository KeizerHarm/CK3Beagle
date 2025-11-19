using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CK3Analyser.Core.Domain.Entities;

namespace CK3Analyser.Core.Parsing.Antlr
{
    public class AntlrParser : ICk3Parser
    {
        private bool isInitialised = false;

        private CK3Lexer lexer;
        private CommonTokenStream commonTokenStream;
        private ParseTreeWalker walker;

        public AntlrParser()
        {

        }

        public void ParseFile(ScriptFile file)
        {
            CK3Parser parser = null;
            AntlrInputStream inputStream = new AntlrInputStream(file.StringRepresentation);

            if (!isInitialised)
            {
                lexer = new CK3Lexer(inputStream);
                commonTokenStream = new CommonTokenStream(lexer);
                walker = new ParseTreeWalker();

                isInitialised = true;
            }
            else
            {
                lexer.SetInputStream(inputStream);
                commonTokenStream.SetTokenSource(lexer);
            }
            lexer.RemoveErrorListeners();
            parser = new CK3Parser(commonTokenStream);
            parser.RemoveErrorListeners();

            //var listener_lexer = new ErrorListener<int>();
            //var listener_parser = new ErrorListener<IToken>();
            //lexer.AddErrorListener(listener_lexer);
            //parser.AddErrorListener(listener_parser);

            var tree = parser.file();
            var listener = new ParsingListener(file);
            walker.Walk(listener, tree);
        }
    }
}
