using CK3Analyser.Analysis.Detectors;
using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Resources;
using CK3Analyser.Core.Resources.DetectorSettings;
using System;
using System.Collections.Generic;

namespace CK3Analyser.Analysis
{
    public class Analyser
    {
        public IEnumerable<LogEntry> LogEntries { get; set; }

        public void Analyse(Context context, Action<string> progressDelegate = null)
        {
            var logger = new Logger();
            logger.progressDelegate = progressDelegate;
            var visitor = new AnalysisVisitor();
            //SetDefaultDetectors(context, logger, visitor);
            SetDetectorsFromSettings(context, logger, visitor);

            progressDelegate($"Now starting on {context.Files.Count} files");
            try
            {

                foreach (var file in context.Files)
                {
                    file.Value.Accept(visitor);
                }
            }
            catch (Exception ex)
            {
                progressDelegate("Error!" + ex.Message);
            }
            //progressDelegate($"Read all files!");
            visitor.Finish();
            //progressDelegate($"Finished final pass!");

            LogEntries = logger.LogEntries;
        }

        private void SetDetectorsFromSettings(Context context, Logger logger, AnalysisVisitor visitor)
        {
            if (GlobalResources.Configuration.LargeUnitSettings.Enabled)
            {
                visitor.Detectors.Add(new LargeUnitDetector(logger, context,
                    GlobalResources.Configuration.LargeUnitSettings));
            }
        }

        private void SetDefaultDetectors(Context context, Logger logger, AnalysisVisitor visitor)
        {
            visitor.Detectors.Add(new LargeUnitDetector(logger, context,
                new LargeUnitSettings
                {
                    File_Severity = Severity.Info,
                    File_MaxSize = 10000,
                    Macro_Severity = Severity.Info,
                    Macro_MaxSize = 50,
                    NonMacroBlock_Severity = Severity.Info,
                    NonMacroBlock_MaxSize = 50
                }));
            //visitor.Detectors.Add(new OvercomplicatedBooleanDetector(logger, context,
            //    new OvercomplicatedBooleanDetector.Settings
            //    {
            //        Absorption_Severity = Severity.Info,
            //        Associativity_Severity = Severity.Warning,
            //        Complementation_Severity = Severity.Critical,
            //        Distributivity_Severity = Severity.Info,
            //        DoubleNegation_Severity = Severity.Warning,
            //        Idempotency_Severity = Severity.Warning,
            //        NotIsNotNor_Severity = Severity.Warning
            //    }));
            //visitor.Detectors.Add(new InconsistentIndentationDetector(logger, context,
            //    new InconsistentIndentationDetector.Settings
            //    {
            //        Inconsistency_Severity = Severity.Warning,
            //        UnexpectedType_Severity = Severity.Info,
            //        DisregardBracketsInComments = true,
            //        ExpectedIndentationType = IndentationType.Tab
            //    }));

            //visitor.Detectors.Add(new DuplicationDetector(logger, context,
            //    new DuplicationDetector.Settings
            //    {
            //        Severity = Severity.Warning,
            //        MinSize = 5
            //    }));

            //visitor.Detectors.Add(new HiddenDependenciesDetector(logger, context,
            //    new HiddenDependenciesDetector.Settings
            //    {
            //        UseOfPrev_Severity = Severity.Warning,
            //        UseOfRoot_Severity = Severity.Warning,
            //        UseOfSavedScope_Severity = Severity.Warning,
            //        UseOfVariable_Severity = Severity.Warning,
            //        UseOfPrev_IgnoreIfInComment = true,
            //        UseOfPrev_IgnoreIfInName = true,
            //        UseOfPrev_AllowInEventFile = false,
            //        UseOfRoot_IgnoreIfInComment = true,
            //        UseOfRoot_IgnoreIfInName = true,
            //        UseOfRoot_AllowInEventFile = false,
            //        UseOfSavedScope_IgnoreIfInComment = true,
            //        UseOfSavedScope_IgnoreIfInName = false,
            //        UseOfSavedScope_AllowInEventFile = false,
            //        UseOfVariable_IgnoreIfInComment = true,
            //        UseOfVariable_IgnoreIfInName = false,
            //        UseOfVariable_AllowInEventFile = false,
            //        VariablesWhitelist = []
            //    }));
            //visitor.Detectors.Add(new MagicNumberDetector(logger, context,
            //    new MagicNumberDetector.Settings
            //    {
            //        Severity = Severity.Info,
            //        StatementKeysToConsider = ["add_gold"]
            //    }));
        }
    }
}
