using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Resources;
using CK3BeagleServer.Core.Resources.DetectorSettings;
using CK3BeagleServer.Core.Generated;

namespace CK3BeagleServer.Analysing.Common.Detectors
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
                UseOfRoot_severity: Severity.Critical
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
                UseOfRoot_severity: Severity.Critical
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
                UseOfPrev_severity: Severity.Critical
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
                UseOfPrev_severity: Severity.Critical
            );

            //act
            visitor.Visit(testcase);

            //assert
            Assert.Equal(numberOfExpectedErrors, logger.LogEntries.Where(x => x.Smell == Smell.HiddenDependencies_UseOfPrev).Count());
        }


        [Theory]
        [InlineData(false, false, false, 3)]
        [InlineData(false, true, false, 2)]
        [InlineData(true, false, false, 2)]
        [InlineData(true, true, false, 1)]
        [InlineData(false, false, true, 0)]
        [InlineData(false, true, true, 0)]
        [InlineData(true, false, true, 0)]
        [InlineData(true, true, true, 0)]
        public void HandlesSavedScopeUses_MacroFile(bool allowIfInComment, bool allowIfInName, bool whiteListed, int numberOfExpectedErrors)
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("HiddenDependencies/RootUses_MacroFile", DeclarationType.ScriptedEffect);
            HashSet<string> whitelist = whiteListed ? ["scopename"] : [];
            var visitor = GetDetector(logger, testcase.Context,
                useOfSavedScope_AllowIfInComment: allowIfInComment,
                useOfSavedScope_AllowIfInName: allowIfInName,
                savedScopesWhitelist: whitelist,
                UseOfSavedScope_severity: Severity.Critical
            );

            //act
            visitor.Visit(testcase);

            //assert
            Assert.Equal(numberOfExpectedErrors, logger.LogEntries.Where(x => x.Smell == Smell.HiddenDependencies_UseOfRoot).Count());
        }

        private static AnalysisVisitor GetDetector(
            Logger logger,
            Context context,
            Severity UseOfPrev_severity = Severity.Critical,
            Severity UseOfRoot_severity = Severity.Critical,
            Severity UseOfSavedScope_severity = Severity.Critical,
            Severity UseOfVariable_severity = Severity.Critical,
            bool useOfPrev_IgnoreIfInComment = false,
            bool useOfPrev_IgnoreIfInName = false,
            bool useOfPrev_AllowInEventFile = true,
            bool useOfRoot_IgnoreIfInComment = false,
            bool useOfRoot_IgnoreIfInName = false,
            bool useOfRoot_AllowInEventFile = true,
            bool useOfSavedScope_AllowIfInComment = false,
            bool useOfSavedScope_AllowIfInName = false,
            bool useOfSavedScope_AllowInEventFile = true,
            bool useOfVariable_IgnoreIfInComment = false,
            bool useOfVariable_IgnoreIfInName = false,
            bool useOfVariable_AllowInEventFile = true,
            HashSet<string>? savedScopesWhitelist = null,
            HashSet<string>? variablesWhitelist = null)
        {
            variablesWhitelist ??= new HashSet<string>();
            var settings = new HiddenDependenciesSettings
            {
                UseOfPrev_Severity = UseOfPrev_severity,
                UseOfRoot_Severity = UseOfRoot_severity,
                UseOfSavedScope_Severity = UseOfSavedScope_severity,
                UseOfVariable_Severity = UseOfVariable_severity,
                UseOfPrev_AllowedIfInComment = useOfPrev_IgnoreIfInComment,
                UseOfPrev_AllowedIfInEventFile = useOfPrev_AllowInEventFile,
                UseOfPrev_AllowedIfInName= useOfPrev_IgnoreIfInName,
                UseOfRoot_AllowedIfInEventFile= useOfRoot_AllowInEventFile,
                UseOfRoot_AllowedIfInComment= useOfRoot_IgnoreIfInComment,
                UseOfRoot_AllowedIfInName = useOfRoot_IgnoreIfInName,
                UseOfSavedScope_AllowedIfInEventFile = useOfSavedScope_AllowInEventFile,
                UseOfSavedScope_AllowedIfInComment = useOfSavedScope_AllowIfInComment,
                UseOfSavedScope_AllowedIfInName = useOfSavedScope_AllowIfInName,
                UseOfSavedScope_Whitelist = savedScopesWhitelist,
                UseOfVariable_AllowedIfInEventFile = useOfVariable_AllowInEventFile,
                UseOfVariable_AllowedIfInComment = useOfVariable_IgnoreIfInComment,
                UseOfVariable_AllowedIfInName = useOfVariable_IgnoreIfInName,
                UseOfVariable_Whitelist = variablesWhitelist
            };

            var visitor = new AnalysisVisitor();
            var detector = new HiddenDependenciesDetector(logger, context, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}
