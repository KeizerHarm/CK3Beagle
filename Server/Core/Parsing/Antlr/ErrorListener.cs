// Template generated code from Antlr4Templates v6.0
namespace CK3Analyser.Core.Parsing.Antlr
{
    using Antlr4.Runtime;
    using System.IO;

    public class ErrorListener<S> : ConsoleErrorListener<S>
	{
		public bool had_error;

		public override void SyntaxError(TextWriter output, IRecognizer recognizer, S offendingSymbol, int line,
			int col, string msg, RecognitionException e)
		{
			had_error = true;
			base.SyntaxError(output, recognizer, offendingSymbol, line, col, msg, e);
		}
	}
}
