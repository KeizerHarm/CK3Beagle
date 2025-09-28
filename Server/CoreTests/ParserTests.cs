using CK3Analyser.Core.Parsing.Antlr;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Parsing.Fast;

namespace CK3Analyser.Core
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
            var expectedDeclarationType = DeclarationType.Debug;
            var parser = GetParser(parserType);

            var expectedScriptFile = new ScriptFile(context, relativePath, expectedDeclarationType, stringToParse);

            //act
            var parsed = parser.ParseText(stringToParse, relativePath, context, expectedDeclarationType);

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
            var expectedDeclarationType = DeclarationType.Debug;
            var parser = GetParser(parserType);

            var expectedScriptFile = new ScriptFile(context, relativePath, expectedDeclarationType, stringToParse);
            var expectedDecl = new Declaration("aaa", expectedDeclarationType)
            {
                Raw = stringToParse
            };
            var expectedBinaryExpression = new BinaryExpression("b", "=", "c")
            {
                Raw = "b = c"
            };
            expectedDecl.AddChild(expectedBinaryExpression);

            expectedScriptFile.AddDeclaration(expectedDecl);

            //act
            var parsed = parser.ParseText(stringToParse, relativePath, context, expectedDeclarationType);

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
            var expectedDeclarationType = DeclarationType.Debug;
            var parser = GetParser(parserType);

            var expectedScriptFile = new ScriptFile(context, relativePath, expectedDeclarationType, stringToParse);
            var expectedDecl = new Declaration("aaa", expectedDeclarationType)
            {
                Raw = "aaa = { b = c }"
            };
            var expectedBinaryExpression = new BinaryExpression("b", "=", "c")
            {
                Raw = "b = c"
            };
            expectedDecl.AddChild(expectedBinaryExpression);

            expectedScriptFile.AddDeclaration(expectedDecl);
            var expectedComment = new Comment()
            {
                Raw = "#Teest!",
                RawWithoutHashtag = "Teest!"
            };
            expectedScriptFile.AddChild(expectedComment);

            //act
            var parsed = parser.ParseText(stringToParse, relativePath, context, expectedDeclarationType);

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

            if (expected.GetType().IsAssignableFrom(typeof(Block)))
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

        private static ICk3Parser GetParser(string parserType)
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
    }
}
