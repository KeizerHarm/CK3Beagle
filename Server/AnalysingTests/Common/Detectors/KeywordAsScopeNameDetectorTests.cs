using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Resources;
using CK3BeagleServer.Core.Generated;
using CK3BeagleServer.Core.Resources.DetectorSettings;

namespace CK3BeagleServer.Analysing.Common.Detectors
{
    public class KeywordAsScopeNameDetectorTests : BaseDetectorTest
    {
        [Fact]
        public void DetectsRootPrevThis()
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("KeywordAsScopeName/RootPrevThis", DeclarationType.ScriptedEffect);
            var visitor = GetDetector(logger, testcase.Context, severity_rootPrevThis: Severity.Critical);

            //act
            visitor.Visit(testcase);

            //assert
            Assert.Equal(3, logger.LogEntries.Count(x => x.Smell == Smell.KeywordAsScopeName_RootPrevThis));
        }

        [Fact]
        public void DetectsScopeLink()
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("KeywordAsScopeName/ScopeLink", DeclarationType.ScriptedEffect);
            var visitor = GetDetector(logger, testcase.Context, severity_rootPrevThis: Severity.Critical);

            //act
            visitor.Visit(testcase);

            //assert
            Assert.Equal(4, logger.LogEntries.Count(x => x.Smell == Smell.KeywordAsScopeName_ScopeLink));
        }

        private static AnalysisVisitor GetDetector(Logger logger,
            Context context,
            Severity severity_rootPrevThis = Severity.Critical,
            Severity severity_scopeLink = Severity.Critical,
            HashSet<string>? keysToConsider = null)
        {
            var visitor = new AnalysisVisitor();
            var settings = new KeywordAsScopeNameSettings
            {
                RootPrevThis_Severity = severity_rootPrevThis,
                ScopeLink_Severity = severity_scopeLink
            };

            var detector = new KeywordAsScopeNameDetector(logger, context, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
