using CK3Analyser.Analysis.Detectors;
using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using System.Collections.Generic;

namespace CK3Analyser.Analysis
{
    public class Analyser
    {
        public IEnumerable<LogEntry> LogEntries { get; set; }

        public void Analyse(Context context)
        {
            var logger = new Logger();
            var visitor = new AnalysisVisitor();
            SetDefaultDetectors(context, logger, visitor);

            foreach (var file in context.Files)
            {
                file.Value.Accept(visitor);
            }
            visitor.Finish();

            LogEntries = logger.LogEntries;
        }

        private static void SetDefaultDetectors(Context context, Logger logger, AnalysisVisitor visitor)
        {
            visitor.Detectors.Add(new LargeUnitDetector(logger, context,
                new LargeUnitDetector.Settings
                {
                    Severity_File = Severity.Info,
                    MaxSize_File = 10000,
                    Severity_Macro = Severity.Info,
                    MaxSize_Macro = 50,
                    Severity_NonMacroBlock = Severity.Info,
                    MaxSize_NonMacroBlock = 50
                }));
            visitor.Detectors.Add(new OvercomplicatedBooleanDetector(logger, context,
                new OvercomplicatedBooleanDetector.Settings
                {
                    Severity_Absorption = Severity.Info,
                    Severity_Associativity = Severity.Warning,
                    Severity_Complementation = Severity.Critical,
                    Severity_Distributivity = Severity.Info,
                    Severity_DoubleNegation = Severity.Warning,
                    Severity_Idempotency = Severity.Warning,
                    Severity_NotIsNotNor = Severity.Warning
                }));
            visitor.Detectors.Add(new InconsistentIndentationDetector(logger, context,
                new InconsistentIndentationDetector.Settings
                {
                    Severity_Inconsistency = Severity.Warning,
                    Severity_UnexpectedType = Severity.Info,
                    DisregardBracketsInComments = true,
                    ExpectedIndentationType = IndentationType.Tab
                }));

            visitor.Detectors.Add(new DuplicationDetector(logger, context,
                new DuplicationDetector.Settings
                {
                    Severity = Severity.Warning,
                    MinSize = 5
                }));

            visitor.Detectors.Add(new HiddenDependenciesDetector(logger, context,
                new HiddenDependenciesDetector.Settings
                {
                    Severity_UseOfPrev = Severity.Warning,
                    Severity_UseOfRoot = Severity.Warning,
                    Severity_UseOfSavedScope = Severity.Warning,
                    Severity_UseOfVariable = Severity.Warning,
                    UseOfPrev_IgnoreIfInComment = true,
                    UseOfPrev_IgnoreIfInName = true,
                    UseOfPrev_AllowInEventFile = false,
                    UseOfRoot_IgnoreIfInComment = true,
                    UseOfRoot_IgnoreIfInName = true,
                    UseOfRoot_AllowInEventFile = false,
                    UseOfSavedScope_IgnoreIfInComment = true,
                    UseOfSavedScope_IgnoreIfInName = false,
                    UseOfSavedScope_AllowInEventFile = false,
                    UseOfVariable_IgnoreIfInComment = true,
                    UseOfVariable_IgnoreIfInName = false,
                    UseOfVariable_AllowInEventFile = false,
                    VariablesWhitelist = []
                }));
            visitor.Detectors.Add(new MagicNumberDetector(logger, context,
                new MagicNumberDetector.Settings
                {
                    Severity = Severity.Info,
                    StatementKeysToConsider = ["add_gold"]
                }));
        }
    }
}
