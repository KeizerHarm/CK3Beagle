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
            var stringToParse = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Detectors/Testcases", caseName + ".txt"));

            GlobalResources.AddEffects(["add_gold"]);
            GlobalResources.AddTriggers(["has_gold", "or", "and", "aaa", "bbb", "ccc", "ddd"]);
            GlobalResources.AddEventTargets(["father"]);
            GlobalResources.Lock();

            var context = new Context("", ContextType.Old);
            var expDeclarationType = expectedDeclarationType ?? DeclarationType.Debug;
            var parser = new AntlrParser();

            var parsedScriptFile = new ScriptFile(context, "", expDeclarationType, stringToParse);
            parser.ParseFile(parsedScriptFile);
            parsedScriptFile.Accept(new SecondPassVisitor());
            return parsedScriptFile;
        }
    }
}
