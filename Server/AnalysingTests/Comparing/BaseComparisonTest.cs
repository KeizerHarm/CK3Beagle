using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Parsing.Antlr;
using CK3Analyser.Core.Parsing.SecondPass;
using CK3Analyser.Core.Resources;

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
            var oldParsed = parser.ParseText(oldFileString, "", oldContext, expDeclarationType);
            var newParsed = parser.ParseText(oldFileString, "", newContext, expDeclarationType);

            oldParsed.Accept(new SecondPassVisitor());
            newParsed.Accept(new SecondPassVisitor());

            return (oldParsed, newParsed);
        }
    }
}
