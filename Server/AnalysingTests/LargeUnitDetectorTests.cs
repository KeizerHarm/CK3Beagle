using CK3Analyser.Analysis;
using CK3Analyser.Analysis.Detectors;
using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;

namespace CK3Analyser.Analysing
{
    public class LargeUnitDetectorTests : BaseTest
    {
        [Theory]
        [InlineData("LargeUnit/LargeFile_SmallUnits", 5, 0)]
        [InlineData("LargeUnit/LargeUnits", 5, 2)]
        [InlineData("LargeUnit/LargeFile_SmallUnits", 15, 0)]
        [InlineData("LargeUnit/LargeUnits", 15, 1)]
        [InlineData("LargeUnit/LargeFile_SmallUnits", 25, 0)]
        [InlineData("LargeUnit/LargeUnits", 25, 0)]
        public void DetectsLargeFile(string file, int maxSize, int numberOfExpectedErrors)
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase(file);
            var visitor = GetDetector(logger, testcase.Context, severity: Severity.Critical, maxSize: maxSize);


            //act
            visitor.Visit(testcase);

            //assert
            Assert.Equal(numberOfExpectedErrors, logger.LogEntries.Where(x => x.Smell == Smell.LargeUnit).Count());
        }

        private static AnalysisVisitor GetDetector(
            Logger logger,
            Context context,
            Severity severity = Severity.Warning,
            int maxSize = 40)
        {
            var settings = new LargeUnitDetector.Settings
            {
                Severity = severity,
                MaxSize = maxSize
            };

            var visitor = new AnalysisVisitor();
            var detector = new LargeUnitDetector(logger, context, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
