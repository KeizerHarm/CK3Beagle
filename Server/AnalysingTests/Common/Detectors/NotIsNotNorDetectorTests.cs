using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Resources;
using CK3BeagleServer.Core.Resources.DetectorSettings;

namespace CK3BeagleServer.Analysing.Common.Detectors
{
    public class NotIsNotNorDetectorTests : BaseDetectorTest
    {
        [Fact]
        public void DetectsNotIsNotNor()
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("NotIsNotNor/NotIsNotNor");
            var visitor = GetDetector(logger, testcase.Context, severity: Severity.Critical);

            //act
            visitor.Visit(testcase);

            //assert
            Assert.Single(logger.LogEntries);
            Assert.Single(logger.LogEntries, x => x.Severity == Severity.Critical && x.Smell == Smell.NotIsNotNor);
        }


        private static AnalysisVisitor GetDetector(Logger logger,
            Context context,
            Severity severity = Severity.Critical)
        {
            var visitor = new AnalysisVisitor();
            var settings = new NotIsNotNorSettings
            {
                Severity = severity
            };

            var detector = new NotIsNotNorDetector(logger, context, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
