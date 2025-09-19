using CK3Analyser.Analysis;
using CK3Analyser.Analysis.Detectors;
using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Antlr;
using CK3Analyser.Core.Domain;

namespace AnalysingTests
{
    public class InconsistentIndentationDetectorTests
    {
        [Theory]
        [InlineData("ConsistentTabs", IndentationType.Tab)]
        [InlineData("ConsistentFourSpaces", IndentationType.FourSpaces)]
        [InlineData("ConsistentThreeSpaces", IndentationType.ThreeSpaces)]
        [InlineData("ConsistentTwoSpaces", IndentationType.TwoSpaces)]
        public void DetectsUsedType(string file, IndentationType expectedDetectedType)
        {
            //arrange
            var logger = new Logger();
            var settings = new InconsistentIndentationDetector.Settings
            {
                Severity_Inconsistency = Severity.Info,
                Severity_UnexpectedType = Severity.Warning,
                DisregardBracketsInComments = true,
                ExpectedIndentationType = expectedDetectedType
            };
            var visitor = GetDetector(settings, logger);

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
            var settings = new InconsistentIndentationDetector.Settings
            {
                Severity_Inconsistency = Severity.Info,
                Severity_UnexpectedType = Severity.Warning,
                DisregardBracketsInComments = true,
                ExpectedIndentationType = IndentationType.FourSpaces
            };
            var visitor = GetDetector(settings, logger);

            ScriptFile testcase = GetTestCase("ConsistentTabs");

            //act
            visitor.Visit(testcase);

            //assert
            Assert.Single(logger.LogEntries, x => x.Severity == Severity.Warning);
            Assert.Single(logger.LogEntries, x => 
                    x.Severity == Severity.Warning 
                 && x.Message.Contains(IndentationType.Tab.ToString())
                 && x.Smell == Smell.InconsistentIndentation_UnexpectedType);
        }

        [Theory]
        [InlineData("ConsistentTabs", true, false)]
        [InlineData("ConsistentTabs", false, true)]
        [InlineData("ConsistentTabs_SpacedWithBracket", false, false)]
        [InlineData("ConsistentTabs_SpacedWithBracket", true, true)]
        public void RepectsSettingDisregardBracketsInComments(string testcase, bool disregardBracketsInComments, bool shouldError)
        {
            //arrange
            var logger = new Logger();
            var settings = new InconsistentIndentationDetector.Settings
            {
                Severity_Inconsistency = Severity.Info,
                Severity_UnexpectedType = Severity.Warning,
                DisregardBracketsInComments = disregardBracketsInComments,
                ExpectedIndentationType = IndentationType.Tab
            };
            var visitor = GetDetector(settings, logger);

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
            var settings = new InconsistentIndentationDetector.Settings
            {
                Severity_Inconsistency = Severity.Info,
                Severity_UnexpectedType = Severity.Warning,
                DisregardBracketsInComments = true,
                ExpectedIndentationType = IndentationType.FourSpaces
            };
            var visitor = GetDetector(settings, logger);

            ScriptFile testcase = GetTestCase("InconsistentTabs");

            //act
            visitor.Visit(testcase);

            //assert
            Assert.Single(logger.LogEntries, x => x.Severity == Severity.Warning);
            Assert.Single(logger.LogEntries, x =>
                    x.Severity == Severity.Info
                 && x.Smell == Smell.InconclusiveIndentation_Inconsistency);
        }

        private ScriptFile GetTestCase(string caseName)
        {
            var stringToParse = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Testcases/InconsistentIndentation", caseName + ".txt"));

            var context = new Context("", ContextType.Old);
            var expectedEntityType = EntityType.Root;
            var parser = new AntlrParser();
            var parsed = parser.ParseText(stringToParse, "", context, expectedEntityType);
            return parsed;
        }

        private static AnalysisVisitor GetDetector(InconsistentIndentationDetector.Settings settings, Logger logger)
        {
            var visitor = new AnalysisVisitor();
            var detector = new InconsistentIndentationDetector(logger, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
