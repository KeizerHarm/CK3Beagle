using CK3Analyser.Analysis;
using CK3Analyser.Analysis.Detectors;
using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources;
using CK3Analyser.Core.Resources.DetectorSettings;

namespace CK3Analyser.Analysing.Detectors
{
    public class InconsistentIndentationDetectorTests : BaseDetectorTest
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
            var visitor = GetDetector(logger, testcase.Context, expectedIndentationTypes: [expectedDetectedType]);

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
            var visitor = GetDetector(logger, testcase.Context, expectedIndentationTypes: [IndentationType.FourSpaces], UnexpectedType_severity: Severity.Critical);

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
        [InlineData("InconsistentIndentation/ConsistentTabs", false, false)]
        [InlineData("InconsistentIndentation/ConsistentTabs", true, true)]
        [InlineData("InconsistentIndentation/ConsistentTabs_SpacedWithBracket", true, false)]
        [InlineData("InconsistentIndentation/ConsistentTabs_SpacedWithBracket", false, true)]
        public void RepectsSettingDisregardBracketsInComments(string file, bool accountBracketsInComments, bool shouldError)
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase(file);
            var visitor = GetDetector(logger, testcase.Context, allowBracketsInComments: accountBracketsInComments);

            //act
            visitor.Visit(testcase);

            //assert
            if (shouldError)
            {
                Assert.Single(logger.LogEntries, x => x.Smell == Smell.InconsistentIndentation_Inconsistency);
            }
            else
            {
                Assert.DoesNotContain(logger.LogEntries, x => x.Smell == Smell.InconsistentIndentation_Inconsistency);
            }
        }

        [Fact]
        public void DetectsInconsistency()
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("InconsistentIndentation/InconsistentTabs");
            var visitor = GetDetector(logger, testcase.Context, Inconsistency_severity: Severity.Critical, expectedIndentationTypes: [IndentationType.FourSpaces]);

            //act
            visitor.Visit(testcase);

            //assert
            Assert.Single(logger.LogEntries, x => x.Severity == Severity.Critical);
            Assert.Single(logger.LogEntries, x =>
                    x.Severity == Severity.Critical
                 && x.Smell == Smell.InconsistentIndentation_Inconsistency);
        }

        private static AnalysisVisitor GetDetector(
            Logger logger,
            Context context,
            Severity Inconsistency_severity = Severity.Warning,
            Severity UnexpectedType_severity = Severity.Warning, 
            bool allowBracketsInComments = false, 
            HashSet<IndentationType>? expectedIndentationTypes = null)
        {
            expectedIndentationTypes ??= [IndentationType.Tab];

            var settings = new InconsistentIndentationSettings
            {
                AbberatingLines_Severity = Inconsistency_severity,
                UnexpectedType_Severity = UnexpectedType_severity,
                AccountCommentedBrackets = allowBracketsInComments,
                AllowedIndentationTypes = expectedIndentationTypes
            };

            var visitor = new AnalysisVisitor();
            var detector = new InconsistentIndentationDetector(logger, context, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
