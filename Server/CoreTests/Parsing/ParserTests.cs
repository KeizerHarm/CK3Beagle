using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Generated;
using CK3Analyser.Core.Resources;
using CK3Analyser.Core.Resources.Storage;

namespace CK3Analyser.Core.Parsing
{
    public class ParserTests : BaseParserTest
    {
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
            var actualScriptFile = new ScriptFile(context, relativePath, expectedDeclarationType, stringToParse);


            //act
            parser.ParseFile(actualScriptFile);

            //assert
            AssertNodesEqual(expectedScriptFile, actualScriptFile);
        }

        [Theory]
        [MemberData(nameof(ParserTypesUnderTest))]
        public void SingleSimpleBlock(string parserType)
        {
            //arrange
            var stringToParse = GetTestCaseContents("SimpleBlock");
            GlobalResources.SymbolTable = new SymbolTable();
            GlobalResources.StringTable = new StringTable();

            var relativePath = "";
            var context = new Context("", ContextType.Old);
            var expectedDeclarationType = DeclarationType.Debug;
            var parser = GetParser(parserType);

            var expectedScriptFile = new ScriptFile(context, relativePath, expectedDeclarationType, stringToParse);
            var expectedDecl = new Declaration("aaa", expectedDeclarationType)
            {
                Start = new Position(0, 0, 0),
                End = new Position(0, stringToParse.Length, stringToParse.Length)
            };
            var expectedBinaryExpression = new BinaryExpression("b", Scoper.Equal, "c")
            {
                Start = new Position(0, 8, 8),
                End = new Position(0, 13, 13)
            };
            expectedDecl.AddChild(expectedBinaryExpression);

            expectedScriptFile.AddChild(expectedDecl);
            expectedScriptFile.RegisterDeclaration(expectedDecl);

            var actualScriptFile = new ScriptFile(context, relativePath, expectedDeclarationType, stringToParse);

            //act
            parser.ParseFile(actualScriptFile);

            //assert
            AssertNodesEqual(expectedScriptFile, actualScriptFile);
        }


        [Theory]
        [MemberData(nameof(ParserTypesUnderTest))]
        public void CommentedBlock(string parserType)
        {
            //arrange
            var stringToParse = GetTestCaseContents("BlockWithComment");

            var relativePath = "";
            var context = new Context("", ContextType.Old);
            var expectedDeclarationType = DeclarationType.Debug;
            var parser = GetParser(parserType);

            var expectedScriptFile = new ScriptFile(context, relativePath, expectedDeclarationType, stringToParse);
            var expectedDecl = new Declaration("aaa", expectedDeclarationType)
            {
                Start = new Position(0, 0, 0),
                End = new Position(0, 15, 15)
            };
            var expectedBinaryExpression = new BinaryExpression("b", Scoper.Equal, "c")
            {
                Start = new Position(0, 8, 8),
                End = new Position(0, 13, 13)
            };
            expectedDecl.AddChild(expectedBinaryExpression);

            expectedScriptFile.AddChild(expectedDecl);
            expectedScriptFile.RegisterDeclaration(expectedDecl);
            var expectedComment = new Comment()
            {
                Start = new Position(0, 16, 16),
                End = new Position(0, stringToParse.Length, stringToParse.Length)
            };
            expectedScriptFile.AddChild(expectedComment);

            var actualScriptFile = new ScriptFile(context, relativePath, expectedDeclarationType, stringToParse);

            //act
            parser.ParseFile(actualScriptFile);

            //assert
            AssertNodesEqual(expectedScriptFile, actualScriptFile);
        }


        [Theory]
        [MemberData(nameof(ParserTypesUnderTest))]
        public void MultilineBlock(string parserType)
        {
            //arrange
            var stringToParse = GetTestCaseContents("MultilineBlock");

            var relativePath = "";
            var context = new Context("", ContextType.Old);
            var expectedDeclarationType = DeclarationType.Debug;
            var parser = GetParser(parserType);

            var expectedScriptFile = new ScriptFile(context, relativePath, expectedDeclarationType, stringToParse);
            var expectedDecl = new Declaration("test_block", expectedDeclarationType)
            {
                Start = new Position(0, 0, 0),
                End = new Position(3, 1, 56)
            };
            var expectedBinExp1 = new BinaryExpression("param1", Scoper.Equal, "val1")
            {
                Start = new Position(1, 4, 20),
                End = new Position(1, 17, 33)
            };
            expectedDecl.AddChild(expectedBinExp1);
            var expectedBinExp2 = new BinaryExpression("param2", Scoper.NotEqual, "val2")
            {
                Start = new Position(2, 4, 39),
                End = new Position(2, 18, 53)
            };
            expectedDecl.AddChild(expectedBinExp2);
            expectedScriptFile.AddChild(expectedDecl);
            expectedScriptFile.RegisterDeclaration(expectedDecl);

            var actualScriptFile = new ScriptFile(context, relativePath, expectedDeclarationType, stringToParse);

            //act
            parser.ParseFile(actualScriptFile);

            //assert
            AssertNodesEqual(expectedScriptFile, actualScriptFile);
        }

        [Theory]
        [MemberData(nameof(ParserTypesUnderTest))]
        public void FullyDecoratedBlock(string parserType)
        {
            //arrange
            var stringToParse = GetTestCaseContents("ScriptedEffect");

            var relativePath = "";
            var context = new Context("", ContextType.Old);
            GlobalResources.SymbolTable = new SymbolTable();
            GlobalResources.StringTable = new StringTable();
            var expectedDeclarationType = DeclarationType.ScriptedEffect;
            var parser = GetParser(parserType);

            var expectedScriptFile = new ScriptFile(context, relativePath, expectedDeclarationType, stringToParse);
            var expectedDecl = new Declaration("laamp_base_3041_contract_scheme_prep_effect", expectedDeclarationType)
            {
                Start = new Position(0, 0, 0),
                End = new Position(13, 1, 393)
            };
            var binExp1 = new BinaryExpression("save_scope_as", Scoper.Equal, "scheme")
            {
                Start = new Position(1, 1, 50),
                End = new Position(1, 23, 72)
            };
            expectedDecl.AddChild(binExp1);
            var namedBlock1 = new NamedBlock("scope:scheme.task_contract", Scoper.ConditionalEqual)
            {
                Start = new Position(2, 1, 75),
                End = new Position(2, 64, 138)
            };

            var binExp2 = new BinaryExpression("save_scope_as", Scoper.Equal, "task_contract")
            {
                Start = new Position(2, 33, 107),
                End = new Position(2, 62, 136)
            };
            namedBlock1.AddChild(binExp2);
            expectedDecl.AddChild(namedBlock1);

            var namedBlock2 = new NamedBlock("save_scope_value_as")
            {
                Start = new Position(3, 1, 141),
                End = new Position(6, 2, 243)
            };
            var binExp3 = new BinaryExpression("name", Scoper.Equal, "follow_up_event")
            {
                Start = new Position(4, 2, 168),
                End = new Position(4, 24, 190)
            };
            var binExp4 = new BinaryExpression("value", Scoper.Equal, "event_id:scheme_critical_moments.2641")
            {
                Start = new Position(5, 2, 194),
                End = new Position(5, 47, 239)
            };
            namedBlock2.AddChild(binExp3);
            namedBlock2.AddChild(binExp4);
            expectedDecl.AddChild(namedBlock2);

            var namedBlock3 = new NamedBlock("if")
            {
                Start = new Position(7, 1, 246),
                End = new Position(12, 2, 390)
            };
            var namedBlock4 = new NamedBlock("limit")
            {
                Start = new Position(8, 2, 256),
                End = new Position(10, 3, 319)
            };
            var namedBlock5 = new NamedBlock("NOT")
            {
                Start = new Position(9, 3, 270),
                End = new Position(9, 47, 314)
            };
            var binExp5 = new BinaryExpression("exists", Scoper.Equal, "scope:suppress_next_event")
            {
                Start = new Position(9, 11, 278),
                End = new Position(9, 45, 312)
            };
            namedBlock5.AddChild(binExp5);
            namedBlock4.AddChild(namedBlock5);
            namedBlock3.AddChild(namedBlock4);

            var namedBlock6 = new NamedBlock("scheme_owner")
            {
                Start = new Position(11, 2, 323),
                End = new Position(11, 65, 386)
            };
            var binExp6 = new BinaryExpression("trigger_event", Scoper.Equal, "scheme_critical_moments.0002")
            {
                Start = new Position(11, 19, 340),
                End = new Position(11, 63, 384)
            };
            namedBlock6.AddChild(binExp6);
            namedBlock3.AddChild(namedBlock6);
            expectedDecl.AddChild(namedBlock3);

            expectedScriptFile.AddChild(expectedDecl);
            expectedScriptFile.RegisterDeclaration(expectedDecl);

            var actualScriptFile = new ScriptFile(context, relativePath, expectedDeclarationType, stringToParse);

            //act
            parser.ParseFile(actualScriptFile);

            //assert
            AssertNodesEqual(expectedScriptFile, actualScriptFile);
        }

    }
}
