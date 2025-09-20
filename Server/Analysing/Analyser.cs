using CK3Analyser.Analysis.Detectors;
using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;

namespace CK3Analyser.Analysis
{
    public class Analyser
    {
        public void Analyse(Context context)
        {
            var logger = new Logger();
            var visitor = new AnalysisVisitor();
            visitor.Detectors.Add(new LargeFileDetector(logger, context, new LargeFileDetector.Settings
            {
                Severity = Severity.Info,
                MaxSize = 10000
            }));
            visitor.Detectors.Add(new LargeUnitDetector(logger, context, 
                new LargeUnitDetector.Settings
                {
                    Severity = Severity.Info,
                    MaxSize = 100
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
            visitor.Detectors.Add(new InconsistentIndentationDetector(logger, context ,
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
            foreach (var file in context.Files)
            {
                file.Value.Accept(visitor);
            }
            visitor.Finish();
        }
    }
}
