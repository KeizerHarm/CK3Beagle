using CK3Analyser.Core.Parsing.Antlr;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Parsing.Fast;

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
            var stringToParse = "aaa = { b = c }";

            var relativePath = "";
            var context = new Context("", ContextType.Old);
            var expectedDeclarationType = DeclarationType.Debug;
            var parser = GetParser(parserType);

            var expectedScriptFile = new ScriptFile(context, relativePath, expectedDeclarationType, stringToParse);
            var expectedDecl = new Declaration("aaa", expectedDeclarationType)
            {
                Raw = stringToParse,
                StartLine = 0,
                StartIndex = 0,
                EndLine = 0,
                EndIndex = stringToParse.Length
            };
            var expectedBinaryExpression = new BinaryExpression("b", "=", "c")
            {
                Raw = "b = c",
                StartLine = 0,
                StartIndex = 8,
                EndLine = 0,
                EndIndex = 13
            };
            expectedDecl.AddChild(expectedBinaryExpression);

            expectedScriptFile.AddDeclaration(expectedDecl);

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
            var stringToParse = "aaa = { b = c } #Teest!";

            var relativePath = "";
            var context = new Context("", ContextType.Old);
            var expectedDeclarationType = DeclarationType.Debug;
            var parser = GetParser(parserType);

            var expectedScriptFile = new ScriptFile(context, relativePath, expectedDeclarationType, stringToParse);
            var expectedDecl = new Declaration("aaa", expectedDeclarationType)
            {
                Raw = "aaa = { b = c }",
                StartLine = 0,
                StartIndex = 0,
                EndLine = 0,
                EndIndex = 15
            };
            var expectedBinaryExpression = new BinaryExpression("b", "=", "c")
            {
                Raw = "b = c",
                StartLine = 0,
                StartIndex = 8,
                EndLine = 0,
                EndIndex = 13
            };
            expectedDecl.AddChild(expectedBinaryExpression);

            expectedScriptFile.AddDeclaration(expectedDecl);
            var expectedComment = new Comment()
            {
                Raw = "#Teest!",
                RawWithoutHashtag = "Teest!",
                StartLine = 0,
                StartIndex = 16,
                EndLine = 0,
                EndIndex = stringToParse.Length
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
            var stringToParse = 
@"test_block = {
    param1 = val1
    param2 != val2
}";

            var relativePath = "";
            var context = new Context("", ContextType.Old);
            var expectedDeclarationType = DeclarationType.Debug;
            var parser = GetParser(parserType);

            var expectedScriptFile = new ScriptFile(context, relativePath, expectedDeclarationType, stringToParse);
            var expectedDecl = new Declaration("test_block", expectedDeclarationType)
            {
                Raw = stringToParse,
                StartLine = 0,
                StartIndex = 0,
                EndLine = 3,
                EndIndex = 1
            };
            var expectedBinExp1 = new BinaryExpression("param1", "=", "val1")
            {
                Raw = "param1 = val1",
                StartLine = 1,
                StartIndex = 4,
                EndLine = 1,
                EndIndex = 17
            };
            expectedDecl.AddChild(expectedBinExp1);
            var expectedBinExp2 = new BinaryExpression("param2", "!=", "val2")
            {
                Raw = "param2 != val2",
                StartLine = 2,
                StartIndex = 4,
                EndLine = 2,
                EndIndex = 18
            };
            expectedDecl.AddChild(expectedBinExp2);
            expectedScriptFile.AddDeclaration(expectedDecl);

            var actualScriptFile = new ScriptFile(context, relativePath, expectedDeclarationType, stringToParse);

            //act
            parser.ParseFile(actualScriptFile);

            //assert
            AssertNodesEqual(expectedScriptFile, actualScriptFile);
        }

        [Theory]
        [MemberData(nameof(ParserTypesUnderTest))]
        public void ScriptedTrigger(string parserType)
        {
            //arrange
            var stringToParse = GetTestCaseContents("ScriptedEffect");

            var relativePath = "";
            var context = new Context("", ContextType.Old);
            var expectedDeclarationType = DeclarationType.ScriptedEffect;
            var parser = GetParser(parserType);

            var expectedScriptFile = new ScriptFile(context, relativePath, expectedDeclarationType, stringToParse);
            var expectedDecl = new Declaration("laamp_base_3041_contract_scheme_prep_effect", expectedDeclarationType)
            {
                Raw = stringToParse,
                StartLine = 0,
                StartIndex = 0,
                EndLine = 13,
                EndIndex = 1
            };
            var binExp1 = new BinaryExpression("save_scope_as", "=", "scheme")
            {
                Raw = "save_scope_as = scheme",
                StartLine = 1,
                StartIndex = 1,
                EndLine = 1,
                EndIndex = 23
            };
            expectedDecl.AddChild(binExp1);
            var namedBlock1 = new NamedBlock("scope:scheme.task_contract", "?=")
            {
                Raw = "scope:scheme.task_contract ?= { save_scope_as = task_contract }",
                StartLine = 2,
                StartIndex = 1,
                EndLine = 2,
                EndIndex = 64
            };

            var binExp2 = new BinaryExpression("save_scope_as", "=", "task_contract")
            {
                Raw = "save_scope_as = task_contract",
                StartLine = 2,
                StartIndex = 33,
                EndLine = 2,
                EndIndex = 62
            };
            namedBlock1.AddChild(binExp2);
            expectedDecl.AddChild(namedBlock1);

            var namedBlock2 = new NamedBlock("save_scope_value_as")
            {
                Raw = @"save_scope_value_as = {
		name = follow_up_event
		value = event_id:scheme_critical_moments.2641
	}",
                StartLine = 3,
                StartIndex = 1,
                EndLine = 6,
                EndIndex = 2
            };
            var binExp3 = new BinaryExpression("name", "=", "follow_up_event")
            {
                Raw = "name = follow_up_event",
                StartLine = 4,
                StartIndex = 2,
                EndLine = 4,
                EndIndex = 24
            };
            var binExp4 = new BinaryExpression("value", "=", "event_id:scheme_critical_moments.2641")
            {
                Raw = "value = event_id:scheme_critical_moments.2641",
                StartLine = 5,
                StartIndex = 2,
                EndLine = 5,
                EndIndex = 47
            };
            namedBlock2.AddChild(binExp3);
            namedBlock2.AddChild(binExp4);
            expectedDecl.AddChild(namedBlock2);

            var namedBlock3 = new NamedBlock("if")
            {
                Raw = @"if = {
		limit = {
			NOT = { exists = scope:suppress_next_event }
		}
		scheme_owner = { trigger_event = scheme_critical_moments.0002 }
	}",
                StartLine = 7,
                StartIndex = 1,
                EndLine = 12,
                EndIndex = 2
            };
            var namedBlock4 = new NamedBlock("limit")
            {
                Raw = @"limit = {
			NOT = { exists = scope:suppress_next_event }
		}",
                StartLine = 8,
                StartIndex = 2,
                EndLine = 10,
                EndIndex = 3
            };
            var namedBlock5 = new NamedBlock("NOT")
            {
                Raw = "NOT = { exists = scope:suppress_next_event }",
                StartLine = 9,
                StartIndex = 3,
                EndLine = 9,
                EndIndex = 47
            };
            var binExp5 = new BinaryExpression("exists", "=", "scope:suppress_next_event")
            {
                Raw = "exists = scope:suppress_next_event",
                StartLine = 9,
                StartIndex = 11,
                EndLine = 9,
                EndIndex = 45
            };
            namedBlock5.AddChild(binExp5);
            namedBlock4.AddChild(namedBlock5);
            namedBlock3.AddChild(namedBlock4);

            var namedBlock6 = new NamedBlock("scheme_owner")
            {
                Raw = "scheme_owner = { trigger_event = scheme_critical_moments.0002 }",
                StartLine = 11,
                StartIndex = 2,
                EndLine = 11,
                EndIndex = 65
            };
            var binExp6 = new BinaryExpression("trigger_event", "=", "scheme_critical_moments.0002")
            {
                Raw = "trigger_event = scheme_critical_moments.0002",
                StartLine = 11,
                StartIndex = 19,
                EndLine = 11,
                EndIndex = 63
            };
            namedBlock6.AddChild(binExp6);
            namedBlock3.AddChild(namedBlock6);
            expectedDecl.AddChild(namedBlock3);

            expectedScriptFile.AddDeclaration(expectedDecl);

            var actualScriptFile = new ScriptFile(context, relativePath, expectedDeclarationType, stringToParse);

            //act
            parser.ParseFile(actualScriptFile);

            //assert
            AssertNodesEqual(expectedScriptFile, actualScriptFile);
        }

    }
}
