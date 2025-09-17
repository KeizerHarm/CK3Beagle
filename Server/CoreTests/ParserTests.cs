using CK3Analyser.Core;
using CK3Analyser.Core.Antlr;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Fast;

namespace CK3Analyser.Parser
{
    public class ParserTests
    {
        public static IEnumerable<object[]> ParserTypesUnderTest =>
        [
            ["antlr"],
           // new object[]{ "fast" }
        ];


        [Theory]
        [MemberData(nameof(ParserTypesUnderTest))]
        public void EmptyFile(string parserType)
        {
            //arrange
            var stringToParse = "";

            var relativePath = "";
            var context = new Context("", ContextType.Old);
            var expectedEntityType = EntityType.Root;
            var parser = GetParser(parserType);

            var expectedScriptFile = new ScriptFile(context, relativePath, expectedEntityType, stringToParse);

            //act
            var parsed = parser.ParseText(stringToParse, relativePath, context, expectedEntityType);

            //assert
            AssertNodesEqual(expectedScriptFile, parsed);
        }

        [Theory]
        [MemberData(nameof(ParserTypesUnderTest))]
        public void SingleSimpleBlock(string parserType)
        {
            //arrange
            var stringToParse = "aaa = { b = c }";

            var relativePath = "";
            var context = new Context("", ContextType.Old);
            var expectedEntityType = EntityType.Root;
            var parser = GetParser(parserType);

            var expectedScriptFile = new ScriptFile(context, relativePath, expectedEntityType, stringToParse);
            var expectedDecl = new Declaration("aaa", expectedEntityType)
            {
                Raw = stringToParse
            };
            var expectedKeyValuePair = new Core.Domain.KeyValuePair("b", "=", "c")
            {
                Raw = "b = c"
            };
            expectedDecl.AddChild(expectedKeyValuePair);

            expectedScriptFile.AddDeclaration(expectedDecl);

            //act
            var parsed = parser.ParseText(stringToParse, relativePath, context, expectedEntityType);

            //assert
            AssertNodesEqual(expectedScriptFile, parsed);
        }


        [Theory]
        [MemberData(nameof(ParserTypesUnderTest))]
        public void CommentedBlock(string parserType)
        {
            //arrange
            var stringToParse = "aaa = { b = c } #Teest!";

            var relativePath = "";
            var context = new Context("", ContextType.Old);
            var expectedEntityType = EntityType.Root;
            var parser = GetParser(parserType);

            var expectedScriptFile = new ScriptFile(context, relativePath, expectedEntityType, stringToParse);
            var expectedDecl = new Declaration("aaa", expectedEntityType)
            {
                Raw = "aaa = { b = c }"
            };
            var expectedKeyValuePair = new Core.Domain.KeyValuePair("b", "=", "c")
            {
                Raw = "b = c"
            };
            expectedDecl.AddChild(expectedKeyValuePair);

            expectedScriptFile.AddDeclaration(expectedDecl);
            var expectedComment = new Comment()
            {
                Raw = "#Teest!",
                RawWithoutHashtag = "Teest!"
            };
            expectedScriptFile.AddChild(expectedComment);

            //act
            var parsed = parser.ParseText(stringToParse, relativePath, context, expectedEntityType);

            //assert
            AssertNodesEqual(expectedScriptFile, parsed);
        }


        private void AssertNodesEqual(Node expected, Node actual)
        {
            Assert.Equal(expected.GetType(), actual.GetType());
            Assert.Equal(expected.Raw, actual.Raw);
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

            if (expected.GetType().IsAssignableTo(typeof(Core.Domain.KeyValuePair)))
            {
                AssertKeyValuePairsEqual((Core.Domain.KeyValuePair)expected, (Core.Domain.KeyValuePair)actual);
            }
        }

        private void AssertKeyValuePairsEqual(Core.Domain.KeyValuePair expected, Core.Domain.KeyValuePair actual)
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
            while (expectedChild != null)
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
            Assert.Equal(expected.ExpectedEntityType, actual.ExpectedEntityType);
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
            Assert.Equal(expected.EntityType, actual.EntityType);
        }





        private static ICk3Parser GetParser(string parserType)
        {
            switch (parserType)
            {
                case "antlr":
                    return new AntlrParser();
                case "fast":
                    return new FastParser();
                default:
                    return null;
            }
        }
    }
}
