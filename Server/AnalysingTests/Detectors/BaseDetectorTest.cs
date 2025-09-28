using CK3Analyser.Core.Parsing.Antlr;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources;
using CK3Analyser.Core.Parsing.SecondPass;

namespace CK3Analyser.Analysing.Detectors
{
    public class BaseDetectorTest
    {
        public static ScriptFile GetTestCase(string caseName, DeclarationType? expectedDeclarationType = null)
        {
            var stringToParse = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Testcases", caseName + ".txt"));

            GlobalResources.AddEffects(["add_gold"]);
            GlobalResources.AddTriggers(["has_gold"]);
            GlobalResources.AddEventTargets(["father"]);

            var context = new Context("", ContextType.Old);
            var expDeclarationType = expectedDeclarationType ?? DeclarationType.Debug;
            var parser = new AntlrParser();
            var parsed = parser.ParseText(stringToParse, "", context, expDeclarationType);
            parsed.Accept(new SecondPassVisitor());
            return parsed;
        }
    }
}
