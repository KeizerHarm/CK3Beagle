using CK3BeagleServer.Core.Comparing;
using CK3BeagleServer.Core.Comparing.Building;
using CK3BeagleServer.Core.Comparing.Domain;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Generated;
using CK3BeagleServer.Core.Parsing.Antlr;
using CK3BeagleServer.Core.Parsing.SemanticPass;
using CK3BeagleServer.Core.Resources;
using CK3BeagleServer.Core.Resources.Storage;

namespace CK3BeagleServer.Analysing.Diff.Detectors
{
    public class BaseDiffDetectorTests
    {
        public static Delta GetTestCase(string caseName, DeclarationType? expectedDeclarationType = null)
        {
            var oldFileString = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Diff/Detectors/Testcases", caseName, "old.txt"));
            var newFileString = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Diff/Detectors/Testcases", caseName, "new.txt"));

            GlobalResources.AddEffects(["add_gold", "xxx", "yyy", "zzz", "else_if", "if", "trigger_event"]);
            GlobalResources.AddTriggers(["has_gold", "or", "and", "nand", "nor", "not", "aaa", "bbb", "ccc", "ddd", "has_primary_title", "is_adult", "T4N_story_content_enabled_for_jito_trigger"]);
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
            oldContext.AddFile(vanillaParsed);
            new SemanticPassHandler().ExecuteSemanticPass(oldContext);

            var modParsed = new ScriptFile(newContext, "", expDeclarationType, newFileString);
            parser.ParseFile(modParsed);
            newContext.AddFile(modParsed);
            new SemanticPassHandler().ExecuteSemanticPass(newContext);

            var delta = new FileComparisonBuilder().BuildFileComparison(vanillaParsed, modParsed);

            return delta;
        }
    }
}
