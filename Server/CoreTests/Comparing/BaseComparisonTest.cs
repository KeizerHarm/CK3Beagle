using CK3Analyser.Core.Comparing.Domain;
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
            oldContext.AddFile(vanillaParsed);
            new SemanticPassHandler().ExecuteSemanticPass(oldContext);

            var modParsed = new ScriptFile(newContext, "", expDeclarationType, newFileString);
            parser.ParseFile(modParsed);
            newContext.AddFile(modParsed);
            new SemanticPassHandler().ExecuteSemanticPass(newContext);

            return (vanillaParsed, modParsed);
        }


        public static Delta GetDelta(DeltaKind kind, params Delta[] children) =>
            new() { Kind = kind, Children = children.Length != 0 ? [.. children] : null };

        public static void AssertDeltasEqual(Delta expected, Delta actual)
        {
            Assert.Equal(expected.Kind, actual.Kind);
            if (expected.Children == null)
            {
                Assert.Null(actual.Children);
            }
            else
            {
                Assert.NotNull(actual.Children);
                Assert.Equal(expected.Children.Count, actual.Children.Count);
                for (int i = 0; i < expected.Children.Count; i++)
                {
                    AssertDeltasEqual(expected.Children[i], actual.Children[i]);
                }
            }
        }
    }
}
