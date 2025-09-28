using CK3Analyser.Analysis;
using CK3Analyser.Analysis.Detectors;
using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;

namespace CK3Analyser.Analysing.Detectors
{
    public class HiddenDependenciesTests : BaseDetectorTest
    {
        [Theory]
        [InlineData(false, false, 3)]
        [InlineData(false, true, 2)]
        [InlineData(true, false, 2)]
        [InlineData(true, true, 1)]
        public void HandlesRootUses_MacroFile(bool ignoreIfInComment, bool ignoreIfInName, int numberOfExpectedErrors)
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("HiddenDependencies/RootUses_MacroFile", DeclarationType.ScriptedEffect);
            var visitor = GetDetector(logger, testcase.Context,
                useOfRoot_IgnoreIfInComment: ignoreIfInComment,
                useOfRoot_IgnoreIfInName: ignoreIfInName,
                severity_UseOfRoot: Severity.Critical
            );

            //act
            visitor.Visit(testcase);

            //assert
            Assert.Equal(numberOfExpectedErrors, logger.LogEntries.Where(x => x.Smell == Smell.HiddenDependencies_UseOfRoot).Count());
        }

        [Theory]
        [InlineData(false, false, false, 3)]
        [InlineData(false, false, true, 2)]
        [InlineData(false, true, false, 2)]
        [InlineData(false, true, true, 1)]
        [InlineData(true, false, false, 0)]
        [InlineData(true, false, true, 0)]
        [InlineData(true, true, false, 0)]
        [InlineData(true, true, true, 0)]
        public void HandlesRootUses_EventFile(bool allowInEventFile, bool ignoreIfInComment, bool ignoreIfInName, int numberOfExpectedErrors)
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("HiddenDependencies/RootUses_EventFile", DeclarationType.Event);
            var visitor = GetDetector(logger, testcase.Context,
                useOfRoot_IgnoreIfInComment: ignoreIfInComment,
                useOfRoot_IgnoreIfInName: ignoreIfInName,
                useOfRoot_AllowInEventFile: allowInEventFile,
                severity_UseOfRoot: Severity.Critical
            );

            //act
            visitor.Visit(testcase);

            //assert
            Assert.Equal(numberOfExpectedErrors, logger.LogEntries.Where(x => x.Smell == Smell.HiddenDependencies_UseOfRoot).Count());
        }

        [Theory]
        [InlineData(false, false, 3)]
        [InlineData(false, true, 2)]
        [InlineData(true, false, 2)]
        [InlineData(true, true, 1)]
        public void HandlesPrevUses_MacroFile(bool ignoreIfInComment, bool ignoreIfInName, int numberOfExpectedErrors)
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("HiddenDependencies/PrevUses_MacroFile", DeclarationType.ScriptedEffect);
            var visitor = GetDetector(logger, testcase.Context,
                useOfPrev_IgnoreIfInComment: ignoreIfInComment,
                useOfPrev_IgnoreIfInName: ignoreIfInName,
                severity_UseOfPrev: Severity.Critical
            );

            //act
            visitor.Visit(testcase);

            //assert
            Assert.Equal(numberOfExpectedErrors, logger.LogEntries.Where(x => x.Smell == Smell.HiddenDependencies_UseOfPrev).Count());
        }

        [Theory]
        [InlineData(false, false, false, 3)]
        [InlineData(false, false, true, 2)]
        [InlineData(false, true, false, 2)]
        [InlineData(false, true, true, 1)]
        [InlineData(true, false, false, 0)]
        [InlineData(true, false, true, 0)]
        [InlineData(true, true, false, 0)]
        [InlineData(true, true, true, 0)]
        public void HandlesPrevUses_EventFile(bool allowInEventFile, bool ignoreIfInComment, bool ignoreIfInName, int numberOfExpectedErrors)
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("HiddenDependencies/PrevUses_EventFile", DeclarationType.Event);
            var visitor = GetDetector(logger, testcase.Context,
                useOfPrev_IgnoreIfInComment: ignoreIfInComment,
                useOfPrev_IgnoreIfInName: ignoreIfInName,
                useOfPrev_AllowInEventFile: allowInEventFile,
                severity_UseOfPrev: Severity.Critical
            );

            //act
            visitor.Visit(testcase);

            //assert
            Assert.Equal(numberOfExpectedErrors, logger.LogEntries.Where(x => x.Smell == Smell.HiddenDependencies_UseOfPrev).Count());
        }


        private static AnalysisVisitor GetDetector(
            Logger logger,
            Context context,
            Severity severity_UseOfPrev = Severity.Critical,
            Severity severity_UseOfRoot = Severity.Critical,
            Severity severity_UseOfSavedScope = Severity.Critical,
            Severity severity_UseOfVariable = Severity.Critical,
            bool useOfPrev_IgnoreIfInComment = false,
            bool useOfPrev_IgnoreIfInName = false,
            bool useOfPrev_AllowInEventFile = true,
            bool useOfRoot_IgnoreIfInComment = false,
            bool useOfRoot_IgnoreIfInName = false,
            bool useOfRoot_AllowInEventFile = true,
            bool useOfSavedScope_IgnoreIfInComment = false,
            bool useOfSavedScope_IgnoreIfInName = false,
            bool useOfSavedScope_AllowInEventFile = true,
            bool useOfVariable_IgnoreIfInComment = false,
            bool useOfVariable_IgnoreIfInName = false,
            bool useOfVariable_AllowInEventFile = true,
            HashSet<string>? variablesWhitelist = null)
        {
            variablesWhitelist ??= new HashSet<string>();
            var settings = new HiddenDependenciesDetector.Settings
            {
                Severity_UseOfPrev = severity_UseOfPrev,
                Severity_UseOfRoot = severity_UseOfRoot,
                Severity_UseOfSavedScope = severity_UseOfSavedScope,
                Severity_UseOfVariable = severity_UseOfVariable,
                UseOfPrev_IgnoreIfInComment = useOfPrev_IgnoreIfInComment,
                UseOfPrev_AllowInEventFile = useOfPrev_AllowInEventFile,
                UseOfPrev_IgnoreIfInName= useOfPrev_IgnoreIfInName,
                UseOfRoot_AllowInEventFile= useOfRoot_AllowInEventFile,
                UseOfRoot_IgnoreIfInComment= useOfRoot_IgnoreIfInComment,
                UseOfRoot_IgnoreIfInName = useOfRoot_IgnoreIfInName,
                UseOfSavedScope_AllowInEventFile = useOfSavedScope_AllowInEventFile,
                UseOfSavedScope_IgnoreIfInComment = useOfSavedScope_IgnoreIfInComment,
                UseOfSavedScope_IgnoreIfInName = useOfSavedScope_IgnoreIfInName,
                UseOfVariable_AllowInEventFile = useOfVariable_AllowInEventFile,
                UseOfVariable_IgnoreIfInComment = useOfVariable_IgnoreIfInComment,
                UseOfVariable_IgnoreIfInName = useOfVariable_IgnoreIfInName,
                VariablesWhitelist = variablesWhitelist
            };

            var visitor = new AnalysisVisitor();
            var detector = new HiddenDependenciesDetector(logger, context, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
