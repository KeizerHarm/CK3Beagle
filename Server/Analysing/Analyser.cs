using CK3Analyser.Analysis.Detectors;
using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CK3Analyser.Analysis
{
    public class Analyser
    {
        public IEnumerable<LogEntry> LogEntries { get; set; }

        public async Task Analyse(Context context, Func<string, Task> progressDelegate = null)
        {
            var logger = new Logger();
            var visitor = new AnalysisVisitor();
            SetDetectors(context, logger, visitor);

            await context.Files.ForEachWithProgress(
                file => file.Value.Accept(visitor),
                percent => progressDelegate($"Analysis {percent}% complete"));

            visitor.Finish();

            LogEntries = logger.LogEntries;
        }

        private static void SetDetectors(Context context, Logger logger, AnalysisVisitor visitor)
        {
            if (GlobalResources.Configuration.LargeUnitSettings.Enabled)
            {
                visitor.Detectors.Add(new LargeUnitDetector(logger, context,
                    GlobalResources.Configuration.LargeUnitSettings));
            }
            if (GlobalResources.Configuration.OvercomplicatedTriggerSettings.Enabled)
            {
                visitor.Detectors.Add(new OvercomplicatedTriggerDetector(logger, context,
                    GlobalResources.Configuration.OvercomplicatedTriggerSettings));
            }
            if (GlobalResources.Configuration.DuplicationSettings.Enabled)
            {
                visitor.Detectors.Add(new DuplicationDetector(logger, context,
                    GlobalResources.Configuration.DuplicationSettings));
            }
            if (GlobalResources.Configuration.HiddenDependenciesSettings.Enabled)
            {
                visitor.Detectors.Add(new HiddenDependenciesDetector(logger, context,
                    GlobalResources.Configuration.HiddenDependenciesSettings));
            }
            if (GlobalResources.Configuration.InconsistentIndentationSettings.Enabled)
            {
                visitor.Detectors.Add(new InconsistentIndentationDetector(logger, context,
                    GlobalResources.Configuration.InconsistentIndentationSettings));
            }
            if (GlobalResources.Configuration.MagicNumberSettings.Enabled)
            {
                visitor.Detectors.Add(new MagicNumberDetector(logger, context,
                    GlobalResources.Configuration.MagicNumberSettings));
            }
            if (GlobalResources.Configuration.KeywordAsScopeNameSettings.Enabled)
            {
                visitor.Detectors.Add(new KeywordAsScopeNameDetector(logger, context,
                    GlobalResources.Configuration.KeywordAsScopeNameSettings));
            }
        }
    }
}
