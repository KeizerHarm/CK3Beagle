using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Resources;
using CK3BeagleServer.Core.Resources.DetectorSettings;

namespace CK3BeagleServer.Analysing.Common.Detectors
{
    public class EntityKeyReuseDetectorTests : BaseDetectorTest
    {
        [Fact]
        public void DetectsSameTypeReuse()
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("EntityKeyReuse/SameType", Core.Generated.DeclarationType.Event);
            var visitor = GetDetector(logger, testcase.Context, severity_sameType: Severity.Critical);

            //act
            visitor.Finish();

            //assert
            Assert.Single(logger.LogEntries);
            Assert.Single(logger.LogEntries, x => x.Smell == Smell.EntityKeyReused_SameType);
        }

        [Fact]
        public void DetectsDifferentTypeReuse()
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("EntityKeyReuse/DifferentType", Core.Generated.DeclarationType.Event);
            var visitor = GetDetector(logger, testcase.Context, severity_differentType: Severity.Critical);

            //act
            visitor.Finish();

            //assert
            Assert.Single(logger.LogEntries);
            Assert.Single(logger.LogEntries, x => x.Smell == Smell.EntityKeyReused_DifferentType);
        }

        private static AnalysisVisitor GetDetector(Logger logger,
            Context context,
            Severity severity_sameType = Severity.Critical,
            Severity severity_differentType = Severity.Critical)
        {
            var visitor = new AnalysisVisitor();
            var settings = new EntityKeyReuseSettings
            {
                SameType_Severity = severity_sameType,
                DifferentType_Severity = severity_differentType
            };

            var detector = new EntityKeyReuseDetector(logger, context, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
