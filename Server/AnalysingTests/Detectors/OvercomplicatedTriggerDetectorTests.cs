using CK3Analyser.Analysis;
using CK3Analyser.Analysis.Detectors;
using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Resources;
using CK3Analyser.Core.Resources.DetectorSettings;

namespace CK3Analyser.Analysing.Detectors
{
    public class OvercomplicatedTriggerDetectorTests : BaseDetectorTest
    {
        [Theory]
        [InlineData("OvercomplicatedTrigger/Associativity_AndWithAnd", "AND")]
        [InlineData("OvercomplicatedTrigger/Associativity_OrWithOr", "OR")]
        public void DetectsAssociativity(string file, string msgKeyword)
        {
            //arrange
            var logger = new Logger();
            var testcase = GetTestCase(file, DeclarationType.ScriptedTrigger);
            var detector = GetDetector(logger, testcase.Context, Associativity_severity: Severity.Critical);

            //act
            detector.Visit(testcase);

            //assert
            Assert.Single(logger.LogEntries);
            Assert.Single(logger.LogEntries, x => x.Severity == Severity.Critical && x.Smell == Smell.OvercomplicatedTrigger_Associativity && x.Message.Contains(msgKeyword));
        }

        [Fact]
        public void DetectsIdempotency()
        {
            //arrange
            var logger = new Logger();
            var testcase = GetTestCase("OvercomplicatedTrigger/Idempotency", DeclarationType.ScriptedTrigger);
            var detector = GetDetector(logger, testcase.Context, Idempotency_severity: Severity.Critical);

            //act
            detector.Visit(testcase);

            //assert
            Assert.Single(logger.LogEntries);
            Assert.Single(logger.LogEntries, x => x.Severity == Severity.Critical && x.Smell == Smell.OvercomplicatedTrigger_Idempotency);
        }

        [Fact]
        public void DetectsComplementation()
        {
            //arrange
            var logger = new Logger();
            var testcase = GetTestCase("OvercomplicatedTrigger/Complementation", DeclarationType.ScriptedTrigger);
            var detector = GetDetector(logger, testcase.Context, Complementation_severity: Severity.Critical);

            //act
            detector.Visit(testcase);

            //assert
            Assert.Single(logger.LogEntries);
            Assert.Single(logger.LogEntries, x => x.Severity == Severity.Critical && x.Smell == Smell.OvercomplicatedTrigger_Complementation);
        }

        [Fact]
        public void DetectsNotIsNotNor()
        {
            //arrange
            var logger = new Logger();
            var testcase = GetTestCase("OvercomplicatedTrigger/NotIsNotNor", DeclarationType.ScriptedTrigger);
            var detector = GetDetector(logger, testcase.Context, NotIsNotNor_severity: Severity.Critical);

            //act
            detector.Visit(testcase);

            //assert
            Assert.Single(logger.LogEntries);
            Assert.Single(logger.LogEntries, x => x.Severity == Severity.Critical && x.Smell == Smell.NotIsNotNor);
        }

        [Fact]
        public void DetectsDoubleNegation()
        {
            //arrange
            var logger = new Logger();
            var testcase = GetTestCase("OvercomplicatedTrigger/DoubleNegation", DeclarationType.ScriptedTrigger);
            var detector = GetDetector(logger, testcase.Context, DoubleNegation_severity: Severity.Critical, NotIsNotNor_severity: Severity.Debug);
            
            //act
            detector.Visit(testcase);

            //assert
            Assert.Equal(13, logger.LogEntries.Count);
            Assert.Equal(13, logger.LogEntries.Where(x => x.Severity == Severity.Critical && x.Smell == Smell.OvercomplicatedTrigger_DoubleNegation).Count());
        }

        [Fact]
        public void DetectsDoubleNegation_NoCounterexamples()
        {
            //arrange
            var logger = new Logger();
            var testcase = GetTestCase("OvercomplicatedTrigger/DoubleNegation_Counterexamples", DeclarationType.ScriptedTrigger);
            var detector = GetDetector(logger, testcase.Context, DoubleNegation_severity: Severity.Critical, NotIsNotNor_severity: Severity.Debug);
            
            //act
            detector.Visit(testcase);

            //assert
            Assert.DoesNotContain(logger.LogEntries, x => x.Severity > Severity.Debug);
        }

        [Fact]
        public void DetectsAbsorption()
        {
            //arrange
            var logger = new Logger();
            var testcase = GetTestCase("OvercomplicatedTrigger/Absorption", DeclarationType.ScriptedTrigger);
            var detector = GetDetector(logger, testcase.Context, 
                Absorption_severity: Severity.Critical, 
                Associativity_severity: Severity.Debug, 
                Distributivity_severity: Severity.Debug);
            
            //act
            detector.Visit(testcase);

            //assert
            Assert.Equal(2, logger.LogEntries.Where(x => x.Severity > Severity.Debug).Count());
            Assert.Equal(2, logger.LogEntries.Where(x => x.Severity == Severity.Critical && x.Smell == Smell.OvercomplicatedTrigger_Absorption).Count());
        }

        [Fact]
        public void DetectsDistributivity()
        {
            //arrange
            var logger = new Logger();
            var testcase = GetTestCase("OvercomplicatedTrigger/Distributivity", DeclarationType.ScriptedTrigger);
            var detector = GetDetector(logger, testcase.Context, 
                Distributivity_severity: Severity.Critical, 
                Associativity_severity: Severity.Debug,
                Absorption_severity: Severity.Debug);
            
            //act
            detector.Visit(testcase);

            //assert
            Assert.Equal(4, logger.LogEntries.Where(x => x.Severity > Severity.Debug).Count());
            Assert.Equal(4, logger.LogEntries.Where(x => x.Severity == Severity.Critical && x.Smell == Smell.OvercomplicatedTrigger_Distributivity).Count());
        }

        private static AnalysisVisitor GetDetector(Logger logger,
            Context context,
            Severity DoubleNegation_severity = Severity.Warning,
            Severity Associativity_severity = Severity.Warning,
            Severity Distributivity_severity = Severity.Warning,
            Severity Idempotency_severity = Severity.Warning,
            Severity Complementation_severity = Severity.Warning,
            Severity NotIsNotNor_severity = Severity.Warning,
            Severity Absorption_severity = Severity.Warning)
        {
            var visitor = new AnalysisVisitor();
            var settings = new OvercomplicatedTriggerSettings
            {
                Absorption_Severity = Absorption_severity,
                Associativity_Severity = Associativity_severity,
                Complementation_Severity = Complementation_severity,
                Distributivity_Severity = Distributivity_severity,
                DoubleNegation_Severity = DoubleNegation_severity,
                Idempotency_Severity = Idempotency_severity,
                NotIsNotNor_Severity = NotIsNotNor_severity
            };

            var detector = new OvercomplicatedTriggerDetector(logger, context, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
