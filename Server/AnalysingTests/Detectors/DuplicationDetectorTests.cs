using CK3Analyser.Analysis;
using CK3Analyser.Analysis.Detectors;
using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources;
using CK3Analyser.Core.Resources.DetectorSettings;
using CK3Analyser.Core.Generated;

namespace CK3Analyser.Analysing.Detectors
{
    public class DuplicationDetectorTests : BaseDetectorTest
    {
        [Fact]
        public void LargeDuplicatesSwallowSmallDuplicates()
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("Duplication/LargeDupesWithSmallDupes", DeclarationType.ScriptedEffect);
            var visitor = GetDetector(logger, testcase.Context,
                severity: Severity.Critical,
                minSize: 1
            );

            //act
            visitor.Visit(testcase);
            visitor.Finish();

            //assert
            Assert.Equal(2, logger.LogEntries.Where(x => x.Smell == Smell.Duplication).Count());
        }

        [Fact]
        public void DuplicatesIgnoresComments()
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("Duplication/DupesWithComments", DeclarationType.ScriptedEffect);
            var visitor = GetDetector(logger, testcase.Context,
                severity: Severity.Critical,
                minSize: 1
            );

            //act
            visitor.Visit(testcase);
            visitor.Finish();

            //assert
            Assert.Equal(2, logger.LogEntries.Where(x => x.Smell == Smell.Duplication).Count());
        }

        private static AnalysisVisitor GetDetector(
            Logger logger,
            Context context,
            Severity severity = Severity.Warning,
            int minSize = 40
            )
        {
            var settings = new DuplicationSettings
            {
                Severity = severity,
                MinSize = minSize
            };

            var visitor = new AnalysisVisitor();
            var detector = new DuplicationDetector(logger, context, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
