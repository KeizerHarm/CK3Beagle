using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core.Comparing.Domain;
using CK3BeagleServer.Core.Resources;
using CK3BeagleServer.Core.Generated;
using CK3BeagleServer.Core.Resources.DetectorSettings;

namespace CK3BeagleServer.Analysing.Diff.Detectors
{
    public class UnencapsulatedAdditionDetectorTests : BaseDiffDetectorTests
    {
        [Fact]
        public void DetectsSimpleFiveStatementInsert()
        {
            //arrange
            var logger = new Logger();
            Delta testcase = GetTestCase("UnencapsulatedAddition/Simple", DeclarationType.ScriptedTrigger);
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
            Delta testcase = GetTestCase("UnencapsulatedAddition/Block", DeclarationType.ScriptedTrigger);
            var visitor = GetDetector(logger, severity: Severity.Critical, threshold: 5);

            //act
            visitor.VisitAny(testcase);

            //assert
            Assert.Single(logger.LogEntries, x => x.Smell == Smell.UnencapsulatedAddition);
        }

        [Fact]
        public void ExtendedTest_CharacterInteraction()
        {
            //arrange
            var logger = new Logger();
            Delta testcase = GetTestCase("UnencapsulatedAddition/ExtendedTest_CharacterInteraction", DeclarationType.CharacterInteraction);
            var visitor = GetDetector(logger, severity: Severity.Critical, threshold: 5);

            //act
            visitor.VisitAny(testcase);

            //assert
            Assert.Empty(logger.LogEntries);
        }

        //[Fact]
        //public void ExtendedTest_NewScriptedEffect()
        //{
        //    //arrange
        //    var logger = new Logger();
        //    Delta testcase = GetTestCase("UnencapsulatedAddition/NewScriptedEffect", DeclarationType.CharacterInteraction);
        //    var visitor = GetDetector(logger, severity: Severity.Critical, threshold: 3);

        //    //act
        //    visitor.VisitAny(testcase);

        //    //assert
        //    Assert.Empty(logger.LogEntries);
        //}

        [Fact]
        public void ExtendedTest_Event()
        {
            //arrange
            var logger = new Logger();
            Delta testcase = GetTestCase("UnencapsulatedAddition/ExtendedTest_Event", DeclarationType.Event);
            var visitor = GetDetector(logger, severity: Severity.Critical, threshold: 5);

            //act
            visitor.VisitAny(testcase);

            //assert
            Assert.Equal(6, logger.LogEntries.Count(x => x.Smell == Smell.UnencapsulatedAddition));
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
