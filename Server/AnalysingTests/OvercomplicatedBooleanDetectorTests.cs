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
        public void DetectsAssociativity(string testcase, string msgKeyword)
        {
            //arrange
            var logger = new Logger();
            var detector = GetDetector(logger, severity_Associativity: Severity.Critical);
            var file = GetTestCase(testcase, EntityType.ScriptedTrigger);

            //act
            detector.Visit(file);

            //assert
            Assert.Single(logger.LogEntries);
            Assert.Single(logger.LogEntries, x => x.Severity == Severity.Critical && x.Smell == Smell.OvercomplicatedBoolean_Associativity && x.Message.Contains(msgKeyword));
        }

        [Fact]
        public void DetectsIdempotency()
        {
            //arrange
            var logger = new Logger();
            var detector = GetDetector(logger, severity_Idempotency: Severity.Critical);
            var file = GetTestCase("OvercomplicatedBoolean/Idempotency", EntityType.ScriptedTrigger);

            //act
            detector.Visit(file);

            //assert
            Assert.Single(logger.LogEntries);
            Assert.Single(logger.LogEntries, x => x.Severity == Severity.Critical && x.Smell == Smell.OvercomplicatedBoolean_Idempotency);
        }

        [Fact]
        public void DetectsComplementation()
        {
            //arrange
            var logger = new Logger();
            var detector = GetDetector(logger, severity_Complementation: Severity.Critical);
            var file = GetTestCase("OvercomplicatedBoolean/Complementation", EntityType.ScriptedTrigger);

            //act
            detector.Visit(file);

            //assert
            Assert.Single(logger.LogEntries);
            Assert.Single(logger.LogEntries, x => x.Severity == Severity.Critical && x.Smell == Smell.OvercomplicatedBoolean_Complementation);
        }

        [Fact]
        public void DetectsNotIsNotNor()
        {
            //arrange
            var logger = new Logger();
            var detector = GetDetector(logger, severity_NotIsNotNor: Severity.Critical);
            var file = GetTestCase("OvercomplicatedBoolean/NotIsNotNor", EntityType.ScriptedTrigger);

            //act
            detector.Visit(file);

            //assert
            Assert.Single(logger.LogEntries);
            Assert.Single(logger.LogEntries, x => x.Severity == Severity.Critical && x.Smell == Smell.NotIsNotNor);
        }

        [Fact]
        public void DetectsDoubleNegation()
        {
            //arrange
            var logger = new Logger();
            var detector = GetDetector(logger, severity_DoubleNegation: Severity.Critical, severity_NotIsNotNor: Severity.Debug);
            var file = GetTestCase("OvercomplicatedBoolean/DoubleNegation", EntityType.ScriptedTrigger);

            //act
            detector.Visit(file);

            //assert
            Assert.Equal(13, logger.LogEntries.Count);
            Assert.Equal(13, logger.LogEntries.Where(x => x.Severity == Severity.Critical && x.Smell == Smell.OvercomplicatedBoolean_DoubleNegation).Count());
        }

        [Fact]
        public void DetectsDoubleNegation_NoCounterexamples()
        {
            //arrange
            var logger = new Logger();
            var detector = GetDetector(logger, severity_DoubleNegation: Severity.Critical, severity_NotIsNotNor: Severity.Debug);
            var file = GetTestCase("OvercomplicatedBoolean/DoubleNegation_Counterexamples", EntityType.ScriptedTrigger);

            //act
            detector.Visit(file);

            //assert
            Assert.DoesNotContain(logger.LogEntries, x => x.Severity > Severity.Debug);
        }

        [Fact]
        public void DetectsAbsorption()
        {
            //arrange
            var logger = new Logger();
            var detector = GetDetector(logger, severity_Absorption: Severity.Critical, severity_Associativity: Severity.Debug, severity_Distributivity: Severity.Debug);
            var file = GetTestCase("OvercomplicatedBoolean/Absorption", EntityType.ScriptedTrigger);

            //act
            detector.Visit(file);

            //assert
            Assert.Equal(2, logger.LogEntries.Where(x => x.Severity > Severity.Debug).Count());
            Assert.Equal(2, logger.LogEntries.Where(x => x.Severity == Severity.Critical && x.Smell == Smell.OvercomplicatedBoolean_Absorption).Count());
        }

        private static AnalysisVisitor GetDetector(Logger logger, 
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

            var detector = new OvercomplicatedBooleanDetector(logger, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
