using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Generated;
using CK3Analyser.Core.Parsing.Antlr;
using CK3Analyser.Core.Parsing.SemanticPass;
using CK3Analyser.Core.Resources;
using CK3Analyser.Core.Resources.Storage;

namespace CK3Analyser.Core.Comparing
{
    public class BaseComparisonTest
    {
        public static (ScriptFile, ScriptFile) GetTestCase(string caseName, DeclarationType? expectedDeclarationType = null)
        {
            var oldFileString = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Comparing/Testcases", caseName, "old.txt"));
            var newFileString = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Comparing/Testcases", caseName, "new.txt"));

            GlobalResources.AddEffects(["add_gold", "xxx", "yyy", "zzz"]);
            GlobalResources.AddTriggers(["has_gold", "or", "and", "nand", "nor", "not", "aaa", "bbb", "ccc", "ddd"]);
            GlobalResources.AddEventTargets(["father", "link1", "link2"]);
            GlobalResources.Lock();
            GlobalResources.SymbolTable = new SymbolTable();
            GlobalResources.StringTable = new StringTable();

            var oldContext = new Context("", ContextType.Vanilla);
            var newContext = new Context("", ContextType.Modded);

            var expDeclarationType = expectedDeclarationType ?? DeclarationType.Debug;
            var parser = new AntlrParser();
            var vanillaParsed = new ScriptFile(oldContext, "", expDeclarationType, oldFileString);
            parser.ParseFile(vanillaParsed);

            var modParsed = new ScriptFile(newContext, "", expDeclarationType, newFileString);
            parser.ParseFile(modParsed);

            vanillaParsed.Accept(new SecondPassVisitor());
            modParsed.Accept(new SecondPassVisitor());

            return (vanillaParsed, modParsed);
        }
    }
}
