using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Resources;
using CK3BeagleServer.Core.Resources.DetectorSettings;

namespace CK3BeagleServer.Analysing.Common.Detectors
{
    public class MagicNumberTests : BaseDetectorTest
    {
        [Fact]
        public void DetectsMagicNumber()
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("MagicNumber/MagicNumber");
            var visitor = GetDetector(logger, testcase.Context, severity: Severity.Critical, keysToConsider: [ "add_gold" ]);

            //act
            visitor.Visit(testcase);

            //assert
            Assert.Single(logger.LogEntries, x => x.Smell == Smell.MagicNumber);
        }

        private static AnalysisVisitor GetDetector(Logger logger,
            Context context,
            Severity severity = Severity.Critical,
            HashSet<string>? keysToConsider = null)
        {
            var visitor = new AnalysisVisitor();
            keysToConsider ??= [];
            var settings = new MagicNumberSettings
            {
                Severity = severity,
                KeysToConsider = keysToConsider
            };

            var detector = new MagicNumberDetector(logger, context, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
