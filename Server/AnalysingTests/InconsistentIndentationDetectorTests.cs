using CK3Analyser.Analysis;
using CK3Analyser.Analysis.Detectors;
using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;

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
            var visitor = GetDetector(logger, expectedIndentationType: expectedDetectedType);

            ScriptFile testcase = GetTestCase(file);

            //act
            visitor.Visit(testcase);

            //assert
            Assert.Single(logger.LogEntries);
            Assert.Single(logger.LogEntries, x => x.Severity == Severity.Debug && x.Message.Contains(expectedDetectedType.ToString()));
        }

        [Fact]
        public void LogsUnexpectedType()
        {
            //arrange
            var logger = new Logger();
            var visitor = GetDetector(logger, expectedIndentationType: IndentationType.FourSpaces, severity_UnexpectedType: Severity.Critical);

            ScriptFile testcase = GetTestCase("InconsistentIndentation/ConsistentTabs");

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
        public void RepectsSettingDisregardBracketsInComments(string testcase, bool disregardBracketsInComments, bool shouldError)
        {
            //arrange
            var logger = new Logger();
            var visitor = GetDetector(logger, disregardCommentedBracket: disregardBracketsInComments);

            ScriptFile file = GetTestCase(testcase);

            //act
            visitor.Visit(file);

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
            var visitor = GetDetector(logger, severity_Inconsistency: Severity.Critical, expectedIndentationType: IndentationType.FourSpaces);

            ScriptFile testcase = GetTestCase("InconsistentIndentation/InconsistentTabs");

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
            var detector = new InconsistentIndentationDetector(logger, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
