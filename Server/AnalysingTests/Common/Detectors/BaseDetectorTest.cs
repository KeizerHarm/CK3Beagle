using CK3Analyser.Core.Parsing.Antlr;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources;
using CK3Analyser.Core.Parsing.SemanticPass;
using CK3Analyser.Core.Generated;
using CK3Analyser.Core.Resources.Storage;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace CK3Analyser.Analysing.Common.Detectors
{
    public class BaseDetectorTest
    {
        public static ScriptFile GetTestCase(string caseName, DeclarationType? expectedDeclarationType = null)
        {
            var stringToParse = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Common/Detectors/Testcases", caseName + ".txt"));

            GlobalResources.AddEffects(["add_gold", "xxx", "yyy", "zzz"]);
            GlobalResources.AddTriggers(["has_gold", "or", "and", "nand", "nor", "not", "aaa", "bbb", "ccc", "ddd"]);
            GlobalResources.AddEventTargets(["father", "link1", "link2"]);
            GlobalResources.Lock();
            GlobalResources.SymbolTable = new SymbolTable();
            GlobalResources.StringTable = new StringTable();

            var context = new Context("", ContextType.Vanilla);
            var expDeclarationType = expectedDeclarationType ?? DeclarationType.Debug;
            var parser = new AntlrParser();

            var parsedScriptFile = new ScriptFile(context, "", expDeclarationType, stringToParse);
            parser.ParseFile(parsedScriptFile);
            context.AddFile(parsedScriptFile);
            new SemanticPassHandler().ExecuteSemanticPass(context);
            return parsedScriptFile;
        }
    }
}
