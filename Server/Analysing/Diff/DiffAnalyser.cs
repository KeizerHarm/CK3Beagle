using CK3Analyser.Analysing.Diff.Detectors;
using CK3Analyser.Analysing.Logging;
using CK3Analyser.Core;
using CK3Analyser.Core.Comparing.Domain;
using CK3Analyser.Core.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CK3Analyser.Analysing.Diff
{
    public class DiffAnalyser
    {
        public IEnumerable<LogEntry> LogEntries { get; set; }

        public async Task Analyse(IEnumerable<Delta> deltas, Func<string, Task> progressDelegate = null)
        {
            var logger = new Logger();
            var visitor = new AnalysisDeltaVisitor();
            SetDetectors(logger, visitor);

            await deltas.ForEachWithProgress(
                delta => delta.Accept(visitor),
                percent => progressDelegate($"Diff analysis {percent}% complete"));

            visitor.Finish();

            LogEntries = logger.LogEntries;
        }

        private static void SetDetectors(Logger logger, AnalysisDeltaVisitor visitor)
        {
            if (GlobalResources.Configuration.UnencapsulatedAdditionSettings.Enabled)
            {
                visitor.Detectors.Add(new UnencapsulatedAdditionDetector(logger,
                    GlobalResources.Configuration.UnencapsulatedAdditionSettings));
            }
        }
    }
}
