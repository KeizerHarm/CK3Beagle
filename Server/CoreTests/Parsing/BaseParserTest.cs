using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Parsing.Antlr;
using CK3BeagleServer.Core.Parsing.Fast;
using CK3BeagleServer.Core.Parsing.SemanticPass;
using CK3BeagleServer.Core.Resources;
using CK3BeagleServer.Core.Resources.Storage;
using CK3BeagleServer.Core.Generated;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace CK3BeagleServer.Core.Parsing
{
    public class BaseParserTest
    {
        public static IEnumerable<object[]> ParserTypesUnderTest =>
        [
            ["antlr"],
           // new object[]{ "fast" }
        ];


        protected void AssertNodesEqual(Node expected, Node actual)
        {
            Assert.Equal(expected.GetType(), actual.GetType());
            Assert.Equal(expected.StringRepresentation, actual.StringRepresentation);
            Assert.Equal(expected.Start, actual.Start);
            Assert.Equal(expected.End, actual.End);
            if (actual.NextSibling != null)
            {
                Assert.Equal(actual.NextSibling.PrevSibling, actual);
            }
            if (actual.PrevSibling != null)
            {
                Assert.Equal(actual.PrevSibling.NextSibling, actual);
            }

            if (expected.GetType().IsAssignableTo(typeof(Block)))
            {
                AssertBlocksEqual((Block)expected, (Block)actual);
            }

            if (expected.GetType().IsAssignableTo(typeof(Comment)))
            {
                AssertCommentsEqual((Comment)expected, (Comment)actual);
            }

            if (expected.GetType().IsAssignableTo(typeof(BinaryExpression)))
            {
                AssertBinaryExpressionsEqual((BinaryExpression)expected, (BinaryExpression)actual);
            }
        }

        private void AssertBinaryExpressionsEqual(BinaryExpression expected, BinaryExpression actual)
        {
            Assert.Equal(expected.Key, actual.Key);
            Assert.Equal(expected.Scoper, actual.Scoper);
            Assert.Equal(expected.Value, actual.Value);
        }

        private void AssertCommentsEqual(Comment expected, Comment actual)
        {
            Assert.Equal(expected.RawWithoutHashtag, actual.RawWithoutHashtag);
        }

        private void AssertBlocksEqual(Block expected, Block actual)
        {
            Assert.Equal(expected.Children.Count, actual.Children.Count);

            if (expected.GetType().IsAssignableTo(typeof(NamedBlock)))
            {
                AssertNamedBlocksEqual((NamedBlock)expected, (NamedBlock)actual);
            }

            if (expected.GetType().IsAssignableTo(typeof(ScriptFile)))
            {
                AssertScriptFilesEqual((ScriptFile)expected, (ScriptFile)actual);
            }

            var expectedChild = expected.Children.FirstOrDefault();
            var actualChild = actual.Children.FirstOrDefault();
            while (expectedChild != null && actualChild != null)
            {
                Assert.NotNull(actual);
                AssertNodesEqual(expectedChild, actualChild);
                expectedChild = expectedChild.NextSibling;
                actualChild = actualChild.NextSibling;
            }
        }
        private void AssertScriptFilesEqual(ScriptFile expected, ScriptFile actual)
        {
            Assert.Equal(expected.RelativePath, actual.RelativePath);
            Assert.Equal(expected.ExpectedDeclarationType, actual.ExpectedDeclarationType);
            Assert.Equal(expected.Context, actual.Context);
            Assert.Equal(expected.Declarations.Count, actual.Declarations.Count);
        }


        private void AssertNamedBlocksEqual(NamedBlock expected, NamedBlock actual)
        {
            Assert.Equal(expected.Key, actual.Key);
            if (expected.GetType().IsAssignableTo(typeof(Declaration)))
            {
                AssertDeclarationsEqual((Declaration)expected, (Declaration)actual);
            }
        }
        private void AssertDeclarationsEqual(Declaration expected, Declaration actual)
        {
            Assert.Equal(expected.DeclarationType, actual.DeclarationType);
        }

        protected static ICk3Parser GetParser(string parserType)
        {
            switch (parserType)
            {
                case "antlr":
                    return new AntlrParser();
                case "fast":
                    return new FastParser();
                default:
                    throw new ArgumentException();
            }
        }

        public static string GetTestCaseContents(string caseName)
        {
            return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Parsing/Testcases", caseName + ".txt"));
        }



        public static ScriptFile GetTestCase(string caseName, string? parserType = null, DeclarationType? expectedDeclarationType = null, string? stringToParse = null,
            IEnumerable<string>? effects = null, IEnumerable<string>? triggers = null, IEnumerable<string>? eventTargets = null)
        {
            stringToParse ??= File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Parsing/Testcases", caseName + ".txt"));

            effects ??= ["add_gold"];
            triggers ??= ["has_gold"];
            eventTargets ??= ["father"];

            GlobalResources.SymbolTable = new SymbolTable();
            GlobalResources.StringTable = new StringTable();
            GlobalResources.AddEffects(effects);
            GlobalResources.AddTriggers(triggers);
            GlobalResources.AddEventTargets(eventTargets);
            GlobalResources.Lock();

            var context = new Context("", ContextType.Vanilla);
            var expDeclarationType = expectedDeclarationType ?? DeclarationType.Debug;
            ICk3Parser parser;
            if (parserType != null)
                parser = GetParser(parserType);
            else
            {
                parser = new AntlrParser();
            }

            var parsedScriptFile = new ScriptFile(context, "", expDeclarationType, stringToParse);
            parser.ParseFile(parsedScriptFile);
            context.AddFile(parsedScriptFile);
            new SemanticPassHandler().ExecuteSemanticPass(context);
            return parsedScriptFile;
        }
    }
}
