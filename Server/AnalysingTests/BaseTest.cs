using CK3Analyser.Core.Antlr;
using CK3Analyser.Core.Domain;

namespace CK3Analyser.Analysing
{
    public class BaseTest
    {
        public static ScriptFile GetTestCase(string caseName, EntityType? expectedEntityType = null)
        {
            var stringToParse = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Testcases", caseName + ".txt"));

            var context = new Context("", ContextType.Old);
            var expEntityType = expectedEntityType ?? EntityType.Root;
            var parser = new AntlrParser();
            var parsed = parser.ParseText(stringToParse, "", context, expEntityType);
            return parsed;
        }
    }
}
