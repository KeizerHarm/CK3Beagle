using CK3Analyser.Analysis;
using CK3Analyser.Analysis.Detectors;
using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;

namespace CK3Analyser.Analysing
{
    public class InconsistentIndentationDetectorTests : BaseTest
    {
        [Theory]
        [InlineData("InconsistentIndentation/ConsistentTabs", IndentationType.Tab)]
        [InlineData("InconsistentIndentation/ConsistentFourSpaces", IndentationType.FourSpaces)]
        [InlineData("InconsistentIndentation/ConsistentThreeSpaces", IndentationType.ThreeSpaces)]
        [InlineData("InconsistentIndentation/ConsistentTwoSpaces", IndentationType.TwoSpaces)]
        public void DetectsUsedType(string file, IndentationType expectedDetectedType)
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase(file);
            var visitor = GetDetector(logger, testcase.Context, expectedIndentationType: expectedDetectedType);


            //act
            visitor.Visit(testcase);

            //assert
            Assert.Single(logger.LogEntries);
            Assert.Single(logger.LogEntries, x => x.Severity == Severity.Debug && x.Message.Contains(expectedDetectedType.ToString()));
        }

        [Fact]
        public void DetectsUnexpectedType()
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("InconsistentIndentation/ConsistentTabs");
            var visitor = GetDetector(logger, testcase.Context, expectedIndentationType: IndentationType.FourSpaces, severity_UnexpectedType: Severity.Critical);


            //act
            visitor.Visit(testcase);

            //assert
            Assert.Single(logger.LogEntries, x => x.Severity == Severity.Critical);
            Assert.Single(logger.LogEntries, x => 
                    x.Severity == Severity.Critical
                 && x.Message.Contains(IndentationType.Tab.ToString())
                 && x.Smell == Smell.InconsistentIndentation_UnexpectedType);
        }

        [Theory]
        [InlineData("InconsistentIndentation/ConsistentTabs", true, false)]
        [InlineData("InconsistentIndentation/ConsistentTabs", false, true)]
        [InlineData("InconsistentIndentation/ConsistentTabs_SpacedWithBracket", false, false)]
        [InlineData("InconsistentIndentation/ConsistentTabs_SpacedWithBracket", true, true)]
        public void RepectsSettingDisregardBracketsInComments(string file, bool disregardBracketsInComments, bool shouldError)
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase(file);
            var visitor = GetDetector(logger, testcase.Context, disregardCommentedBracket: disregardBracketsInComments);


            //act
            visitor.Visit(testcase);

            //assert
            if (shouldError)
            {
                Assert.Single(logger.LogEntries, x => x.Smell == Smell.InconclusiveIndentation_Inconsistency);
            }
            else
            {
                Assert.DoesNotContain(logger.LogEntries, x => x.Smell == Smell.InconclusiveIndentation_Inconsistency);
            }
        }

        [Fact]
        public void DetectsInconsistency()
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("InconsistentIndentation/InconsistentTabs");
            var visitor = GetDetector(logger, testcase.Context, severity_Inconsistency: Severity.Critical, expectedIndentationType: IndentationType.FourSpaces);


            //act
            visitor.Visit(testcase);

            //assert
            Assert.Single(logger.LogEntries, x => x.Severity == Severity.Critical);
            Assert.Single(logger.LogEntries, x =>
                    x.Severity == Severity.Critical
                 && x.Smell == Smell.InconclusiveIndentation_Inconsistency);
        }

        private static AnalysisVisitor GetDetector(
            Logger logger,
            Context context,
            Severity severity_Inconsistency = Severity.Warning,
            Severity severity_UnexpectedType = Severity.Warning, 
            bool disregardCommentedBracket = true, 
            IndentationType expectedIndentationType = IndentationType.Tab)
        {
            var settings = new InconsistentIndentationDetector.Settings
            {
                Severity_Inconsistency = severity_Inconsistency,
                Severity_UnexpectedType = severity_UnexpectedType,
                DisregardBracketsInComments = disregardCommentedBracket,
                ExpectedIndentationType = expectedIndentationType
            };

            var visitor = new AnalysisVisitor();
            var detector = new InconsistentIndentationDetector(logger, context, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
