using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Resources;
using CK3BeagleServer.Core.Resources.DetectorSettings;

namespace CK3BeagleServer.Analysing.Common.Detectors
{
    public class MisuseOfThisDetectorTests : BaseDetectorTest
    {
        [Fact]
        public void DetectsMisuseOfThis()
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("MisuseOfThis/MisuseOfThis");
            var visitor = GetDetector(logger, testcase.Context, severity: Severity.Critical);

            //act
            visitor.Visit(testcase);

            //assert
            Assert.Single(logger.LogEntries);
            Assert.Single(logger.LogEntries, x => x.Severity == Severity.Critical && x.Smell == Smell.MisuseOfThis);
        }

        private static AnalysisVisitor GetDetector(Logger logger,
            Context context,
            Severity severity = Severity.Critical)
        {
            var visitor = new AnalysisVisitor();
            var settings = new MisuseOfThisSettings
            {
                Severity = severity
            };

            var detector = new MisuseOfThisDetector(logger, context, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
