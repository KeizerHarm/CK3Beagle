using CK3Analyser.Analysis;
using CK3Analyser.Analysis.Detectors;
using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;

namespace CK3Analyser.Analysing
{
    public class LargeFileDetectorTests : BaseTest
    {
        [Theory]
        [InlineData("LargeFile/LargeFile_1Line", 40, false)]
        [InlineData("LargeFile/LargeFile_40Lines", 40, false)]
        [InlineData("LargeFile/LargeFile_41Lines", 40, true)]
        [InlineData("LargeFile/LargeFile_10000Lines", 40, true)]
        [InlineData("LargeFile/LargeFile_1Line", 41, false)]
        [InlineData("LargeFile/LargeFile_40Lines", 41, false)]
        [InlineData("LargeFile/LargeFile_41Lines", 41, false)]
        [InlineData("LargeFile/LargeFile_10000Lines", 41, true)]
        public void DetectsLargeFile(string file, int maxSize, bool shouldError)
        {
            //arrange
            var logger = new Logger();
            var visitor = GetDetector(logger, severity: Severity.Critical, maxSize: maxSize);

            ScriptFile testcase = GetTestCase(file);

            //act
            visitor.Visit(testcase);

            //assert
            if (shouldError)
            {
                Assert.Single(logger.LogEntries);
                Assert.Single(logger.LogEntries, x => x.Smell == Smell.LargeFile);
            }
            else
            {
                Assert.Empty(logger.LogEntries);
            }
        }

        private static AnalysisVisitor GetDetector(
            Logger logger,
            Severity severity = Severity.Warning,
            int maxSize = 40)
        {
            var settings = new LargeFileDetector.Settings
            {
                Severity = severity,
                MaxSize = maxSize
            };

            var visitor = new AnalysisVisitor();
            var detector = new LargeFileDetector(logger, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
