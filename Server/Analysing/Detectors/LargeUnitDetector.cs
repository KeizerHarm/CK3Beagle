using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using System;

namespace CK3Analyser.Analysis.Detectors
{
    public class LargeUnitDetector : BaseDetector
    {
        public struct Settings
        {
            public int MaxSize { get; set; }
            public Severity Severity { get; set; }
        }

        private Settings _settings;

        public LargeUnitDetector(ILogger logger, Context context, Settings settings) : base(logger, context)
        {
            _settings = settings;
        }

        public override void AnalyseDeclaration(Declaration declaration)
        {
            var length = declaration.Raw.Split('\n').Length;
            if (length > _settings.MaxSize)
            {
                logger.Log(
                    Smell.LargeUnit,
                    _settings.Severity,
                    $"Large declaration detected: {length} lines",
                    declaration.GetIdentifier());
            }
        }
    }
}
