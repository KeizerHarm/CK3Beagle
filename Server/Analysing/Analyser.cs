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
            //visitor.Detectors.Add(new LargeFileDetector(logFunc));
            //visitor.Detectors.Add(new LargeUnitDetector(logFunc));
            //visitor.Detectors.Add(new OvercomplicatedBooleanDetector(logger,
            //    new OvercomplicatedBooleanDetector.Settings
            //    {
            //        Severity_Absorption = Severity.Critical,
            //        Severity_Associativity = Severity.Critical,
            //        Severity_Complementation = Severity.Critical,
            //        Severity_Distributivity = Severity.Critical,
            //        Severity_DoubleNegation = Severity.Critical,
            //        Severity_Idempotency = Severity.Critical,
            //        Severity_NotIsNotNor = Severity.Critical
            //    }));
            //visitor.Detectors.Add(new InconsistentIndentationDetector(logFunc));

            visitor.Detectors.Add(new DuplicationDetector(logger, context));
            foreach (var file in context.Files)
            {
                file.Value.Accept(visitor);
            }
            visitor.Finish();
        }
    }
}
