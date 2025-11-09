using CK3Analyser.Analysis;
using CK3Analyser.Analysis.Detectors;
using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources;
using CK3Analyser.Core.Resources.DetectorSettings;

namespace CK3Analyser.Analysing.Detectors
{
    public class MagicNumberTests : BaseDetectorTest
    {
        [Fact]
        public void DetectsMagicNumber()
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("MagicNumber/MagicNumber");
            var visitor = GetDetector(logger, testcase.Context, severity: Severity.Critical, statementsToConsider: [ "add_gold" ]);

            //act
            visitor.Visit(testcase);

            //assert
            Assert.Single(logger.LogEntries, x => x.Smell == Smell.MagicNumber);
        }


        private static AnalysisVisitor GetDetector(Logger logger,
            Context context,
            Severity severity = Severity.Critical,
            HashSet<string>? statementsToConsider = null)
        {
            var visitor = new AnalysisVisitor();
            statementsToConsider ??= [];
            var settings = new MagicNumberSettings
            {
                Severity = severity,
                StatementKeysToConsider = statementsToConsider
            };

            var detector = new MagicNumberDetector(logger, context, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
