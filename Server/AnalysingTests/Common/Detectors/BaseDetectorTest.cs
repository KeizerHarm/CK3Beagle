using CK3BeagleServer.Core.Parsing.Antlr;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Resources;
using CK3BeagleServer.Core.Parsing.SemanticPass;
using CK3BeagleServer.Core.Generated;
using CK3BeagleServer.Core.Resources.Storage;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace CK3BeagleServer.Analysing.Common.Detectors
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
