using CK3Analyser.Analysing.Logging;
using CK3Analyser.Core.Comparing.Domain;
using CK3Analyser.Core.Resources;
using CK3Analyser.Core.Resources.DetectorSettings;

namespace CK3Analyser.Analysing.Diff.Detectors
{
    public class UnencapsulatedAdditionDetectorTests : BaseDiffDetectorTests
    {
        [Fact]
        public void DetectsSimpleFiveStatementInsert()
        {
            //arrange
            var logger = new Logger();
            Delta testcase = GetTestCase("UnencapsulatedAddition/Simple");
            var visitor = GetDetector(logger, severity: Severity.Critical, threshold: 5);

            //act
            visitor.VisitAny(testcase);

            //assert
            Assert.Single(logger.LogEntries, x => x.Smell == Smell.UnencapsulatedAddition);
        }
        [Fact]
        public void DetectsBlockInsert()
        {
            //arrange
            var logger = new Logger();
            Delta testcase = GetTestCase("UnencapsulatedAddition/Block");
            var visitor = GetDetector(logger, severity: Severity.Critical, threshold: 5);

            //act
            visitor.VisitAny(testcase);

            //assert
            Assert.Single(logger.LogEntries, x => x.Smell == Smell.UnencapsulatedAddition);
        }

        private static AnalysisDeltaVisitor GetDetector(Logger logger,
            Severity severity = Severity.Critical,
            int threshold = 5)
        {
            var visitor = new AnalysisDeltaVisitor();
            var settings = new UnencapsulatedAdditionSettings
            {
                Severity = severity,
                Threshold = threshold
            };

            var detector = new UnencapsulatedAdditionDetector(logger, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
