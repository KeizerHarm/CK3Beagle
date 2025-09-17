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
            var logFunc = logger.Log;
            var visitor = new AnalysisVisitor();
            //visitor.Detectors.Add(new LargeFileDetector(logFunc));
            //visitor.Detectors.Add(new LargeUnitDetector(logFunc));
            //visitor.Detectors.Add(new OvercomplicatedBooleanDetector(logFunc));
            //visitor.Detectors.Add(new InconsistentIndentationDetector(logFunc));
            foreach (var file in context.Files)
            {
                file.Value.Accept(visitor);
            }
        }
    }
}
