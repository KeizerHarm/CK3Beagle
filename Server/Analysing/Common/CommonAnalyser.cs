using CK3BeagleServer.Analysing.Common.Detectors;
using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CK3BeagleServer.Analysing.Common
{
    public class CommonAnalyser
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
            if (GlobalResources.Configuration.NotIsNotNorSettings.Enabled)
            {
                visitor.Detectors.Add(new NotIsNotNorDetector(logger, context,
                    GlobalResources.Configuration.NotIsNotNorSettings));
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
            if (GlobalResources.Configuration.EntityKeyReuseSettings.Enabled)
            {
                visitor.Detectors.Add(new EntityKeyReuseDetector(logger, context,
                    GlobalResources.Configuration.EntityKeyReuseSettings));
            }
            if (GlobalResources.Configuration.MisuseOfThisSettings.Enabled)
            {
                visitor.Detectors.Add(new MisuseOfThisDetector(logger, context,
                    GlobalResources.Configuration.MisuseOfThisSettings));
            }
            if (GlobalResources.Configuration.LinkAsParameterSettings.Enabled)
            {
                visitor.Detectors.Add(new LinkAsParameterDetector(logger, context,
                    GlobalResources.Configuration.LinkAsParameterSettings));
            }
        }
    }
}
