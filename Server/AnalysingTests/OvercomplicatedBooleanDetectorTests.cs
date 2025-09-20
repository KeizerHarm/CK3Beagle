using CK3Analyser.Analysis;
using CK3Analyser.Analysis.Detectors;
using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;

namespace CK3Analyser.Analysing
{
    public class OvercomplicatedBooleanDetectorTests : BaseTest
    {
        [Theory]
        [InlineData("OvercomplicatedBoolean/Associativity_AndWithAnd", "AND")]
        [InlineData("OvercomplicatedBoolean/Associativity_OrWithOr", "OR")]
        public void DetectsAssociativity(string file, string msgKeyword)
        {
            //arrange
            var logger = new Logger();
            var testcase = GetTestCase(file, DeclarationType.ScriptedTrigger);
            var detector = GetDetector(logger, testcase.Context, severity_Associativity: Severity.Critical);

            //act
            detector.Visit(testcase);

            //assert
            Assert.Single(logger.LogEntries);
            Assert.Single(logger.LogEntries, x => x.Severity == Severity.Critical && x.Smell == Smell.OvercomplicatedBoolean_Associativity && x.Message.Contains(msgKeyword));
        }

        [Fact]
        public void DetectsIdempotency()
        {
            //arrange
            var logger = new Logger();
            var testcase = GetTestCase("OvercomplicatedBoolean/Idempotency", DeclarationType.ScriptedTrigger);
            var detector = GetDetector(logger, testcase.Context, severity_Idempotency: Severity.Critical);

            //act
            detector.Visit(testcase);

            //assert
            Assert.Single(logger.LogEntries);
            Assert.Single(logger.LogEntries, x => x.Severity == Severity.Critical && x.Smell == Smell.OvercomplicatedBoolean_Idempotency);
        }

        [Fact]
        public void DetectsComplementation()
        {
            //arrange
            var logger = new Logger();
            var testcase = GetTestCase("OvercomplicatedBoolean/Complementation", DeclarationType.ScriptedTrigger);
            var detector = GetDetector(logger, testcase.Context, severity_Complementation: Severity.Critical);

            //act
            detector.Visit(testcase);

            //assert
            Assert.Single(logger.LogEntries);
            Assert.Single(logger.LogEntries, x => x.Severity == Severity.Critical && x.Smell == Smell.OvercomplicatedBoolean_Complementation);
        }

        [Fact]
        public void DetectsNotIsNotNor()
        {
            //arrange
            var logger = new Logger();
            var testcase = GetTestCase("OvercomplicatedBoolean/NotIsNotNor", DeclarationType.ScriptedTrigger);
            var detector = GetDetector(logger, testcase.Context, severity_NotIsNotNor: Severity.Critical);

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
            var testcase = GetTestCase("OvercomplicatedBoolean/DoubleNegation", DeclarationType.ScriptedTrigger);
            var detector = GetDetector(logger, testcase.Context, severity_DoubleNegation: Severity.Critical, severity_NotIsNotNor: Severity.Debug);
            
            //act
            detector.Visit(testcase);

            //assert
            Assert.Equal(13, logger.LogEntries.Count);
            Assert.Equal(13, logger.LogEntries.Where(x => x.Severity == Severity.Critical && x.Smell == Smell.OvercomplicatedBoolean_DoubleNegation).Count());
        }

        [Fact]
        public void DetectsDoubleNegation_NoCounterexamples()
        {
            //arrange
            var logger = new Logger();
            var testcase = GetTestCase("OvercomplicatedBoolean/DoubleNegation_Counterexamples", DeclarationType.ScriptedTrigger);
            var detector = GetDetector(logger, testcase.Context, severity_DoubleNegation: Severity.Critical, severity_NotIsNotNor: Severity.Debug);
            
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
            var testcase = GetTestCase("OvercomplicatedBoolean/Absorption", DeclarationType.ScriptedTrigger);
            var detector = GetDetector(logger, testcase.Context, severity_Absorption: Severity.Critical, severity_Associativity: Severity.Debug, severity_Distributivity: Severity.Debug);
            
            //act
            detector.Visit(testcase);

            //assert
            Assert.Equal(2, logger.LogEntries.Where(x => x.Severity > Severity.Debug).Count());
            Assert.Equal(2, logger.LogEntries.Where(x => x.Severity == Severity.Critical && x.Smell == Smell.OvercomplicatedBoolean_Absorption).Count());
        }

        [Fact]
        public void DetectsDistributivity()
        {
            //arrange
            var logger = new Logger();
            var testcase = GetTestCase("OvercomplicatedBoolean/Distributivity", DeclarationType.ScriptedTrigger);
            var detector = GetDetector(logger, testcase.Context, severity_Distributivity: Severity.Critical, severity_Associativity: Severity.Debug);
            
            //act
            detector.Visit(testcase);

            //assert
            Assert.Equal(2, logger.LogEntries.Where(x => x.Severity > Severity.Debug).Count());
            Assert.Equal(2, logger.LogEntries.Where(x => x.Severity == Severity.Critical && x.Smell == Smell.OvercomplicatedBoolean_Distributivity).Count());
        }

        private static AnalysisVisitor GetDetector(Logger logger,
            Context context,
            Severity severity_DoubleNegation = Severity.Warning,
            Severity severity_Associativity = Severity.Warning,
            Severity severity_Distributivity = Severity.Warning,
            Severity severity_Idempotency = Severity.Warning,
            Severity severity_Complementation = Severity.Warning,
            Severity severity_NotIsNotNor = Severity.Warning,
            Severity severity_Absorption = Severity.Warning)
        {
            var visitor = new AnalysisVisitor();
            var settings = new OvercomplicatedBooleanDetector.Settings
            {
                Severity_Absorption = severity_Absorption,
                Severity_Associativity = severity_Associativity,
                Severity_Complementation = severity_Complementation,
                Severity_Distributivity = severity_Distributivity,
                Severity_DoubleNegation = severity_DoubleNegation,
                Severity_Idempotency = severity_Idempotency,
                Severity_NotIsNotNor = severity_NotIsNotNor
            };

            var detector = new OvercomplicatedBooleanDetector(logger, context, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
