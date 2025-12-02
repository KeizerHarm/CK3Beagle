using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Resources;
using CK3BeagleServer.Core.Resources.DetectorSettings;

namespace CK3BeagleServer.Analysing.Common.Detectors
{
    public class LinkAsParameterTests : BaseDetectorTest
    {
        [Fact]
        public void DetectsLinkAsParameter()
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("LinkAsParameter/LinkAsParameter", Core.Generated.DeclarationType.ScriptedEffect);
            var visitor = GetDetector(logger, testcase.Context, severity: Severity.Critical);

            //act
            visitor.Visit(testcase);

            //assert
            Assert.Equal(2, logger.LogEntries.Count(x => x.Smell == Smell.LinkAsParameter));
        }

        private static AnalysisVisitor GetDetector(Logger logger,
            Context context,
            Severity severity = Severity.Critical)
        {
            var visitor = new AnalysisVisitor();
            var settings = new LinkAsParameterSettings
            {
                Severity = severity
            };

            var detector = new LinkAsParameterDetector(logger, context, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
