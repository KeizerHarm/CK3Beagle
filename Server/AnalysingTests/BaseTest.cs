using CK3Analyser.Core.Antlr;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;

namespace CK3Analyser.Analysing
{
    public class BaseTest
    {
        public static ScriptFile GetTestCase(string caseName, DeclarationType? expectedDeclarationType = null)
        {
            var stringToParse = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Testcases", caseName + ".txt"));

            var context = new Context("", ContextType.Old);
            var expDeclarationType = expectedDeclarationType ?? DeclarationType.Debug;
            var parser = new AntlrParser();
            var parsed = parser.ParseText(stringToParse, "", context, expDeclarationType);
            return parsed;
        }
    }
}
