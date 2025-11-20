using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Parsing.Antlr;
using CK3Analyser.Core.Parsing.SemanticPass;
using CK3Analyser.Core.Resources;
using CK3Analyser.Core.Generated;

namespace CK3Analyser.Analysing.Comparing
{
    public class BaseComparisonTest
    {
        public static (ScriptFile, ScriptFile) GetTestCase(string caseName, DeclarationType? expectedDeclarationType = null)
        {
            var oldFileString = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Comparing/Testcases", caseName, "old.txt"));
            var newFileString = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Comparing/Testcases", caseName, "new.txt"));

            GlobalResources.AddEffects(["add_gold"]);
            GlobalResources.AddTriggers(["has_gold"]);
            GlobalResources.AddEventTargets(["father"]);

            var oldContext = new Context("", ContextType.Old);
            var newContext = new Context("", ContextType.New);

            var expDeclarationType = expectedDeclarationType ?? DeclarationType.Debug;
            var parser = new AntlrParser();
            var oldParsed = new ScriptFile(oldContext, "", expDeclarationType, oldFileString);
            parser.ParseFile(oldParsed);

            var newParsed = new ScriptFile(newContext, "", expDeclarationType, newFileString);
            parser.ParseFile(newParsed);

            oldParsed.Accept(new SecondPassVisitor());
            newParsed.Accept(new SecondPassVisitor());

            return (oldParsed, newParsed);
        }
    }
}
