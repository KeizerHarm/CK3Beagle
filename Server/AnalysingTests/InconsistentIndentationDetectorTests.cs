using CK3Analyser.Analysis;
using CK3Analyser.Analysis.Detectors;
using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Antlr;
using CK3Analyser.Core.Domain;

namespace AnalysingTests
{
    public class InconsistentIndentationDetectorTests
    {
        //public static IEnumerable<object[]> ParserTypesUnderTest =>
        //[
        //    ["antlr"],
        //   // new object[]{ "fast" }
        //];

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
            logger.LogEntries.Single();
            logger.LogEntries.Single(x => x.Severity == Severity.Debug && x.Message.Contains(expectedDetectedType.ToString()));
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
            var logFunc = logger.Log;
            var visitor = new AnalysisVisitor();
            var detector = new InconsistentIndentationDetector(logFunc, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
