using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Resources.DetectorSettings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CK3BeagleServer.Analysing.Common.Detectors
{
    public class EntityKeyReuseDetector : BaseDetector
    {
        private readonly EntityKeyReuseSettings _settings;

        public EntityKeyReuseDetector(ILogger logger, Context context, EntityKeyReuseSettings settings) : base(logger, context)
        {
            _settings = settings;
        }

        public override void Finish()
        {
            var keysWithSameTypeOverrides = context.OverriddenDeclarationCopies.Select(x => x.Key).Distinct().ToHashSet();
            Dictionary<string, Declaration> allDeclarations = [];

            foreach (var declArray in context.Declarations)
            {
                foreach (var declaration in declArray)
                {
                    if (keysWithSameTypeOverrides.Contains(declaration.Key))
                    {
                        LogSameTypeReuse(declaration.Value);
                    }
                    if (!allDeclarations.TryAdd(declaration.Key, declaration.Value))
                    {
                        LogDifferentTypeReuse(allDeclarations[declaration.Key], declaration.Value);
                    }
                }
            }
        }

        private void LogDifferentTypeReuse(Declaration decl1, Declaration decl2)
        {
            var secondReport = LogEntry.MinimalLogEntry("Other instance", decl1);
            var logEntry = new LogEntry(Smell.EntityKeyReuse_DifferentType, _settings.DifferentType_Severity,
                "Two declarations use the same key", decl2);
            logger.Log(logEntry, secondReport);
        }

        private void LogSameTypeReuse(Declaration declaration)
        {
            var overriddenCopies = context.OverriddenDeclarationCopies.Where(x => x.Key == declaration.Key);
            string msg = null;
            if (overriddenCopies.Count() > 1)
            {
                msg = "Declaration overrides " + overriddenCopies.Count() + " earlier declarations of the same type and key";
            }
            else
            {
                msg = "Declaration overrides earlier declaration of the same type and key";
            }

            var otherReports = overriddenCopies.Select(x => LogEntry.MinimalLogEntry("Earlier instance", x)).ToArray();
            var logEntry = new LogEntry(Smell.EntityKeyReuse_SameType, _settings.SameType_Severity, msg, declaration);
            logger.Log(logEntry, otherReports);
        }
    }
}