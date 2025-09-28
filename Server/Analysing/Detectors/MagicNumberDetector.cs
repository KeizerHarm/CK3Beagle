using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using System.Collections.Generic;

namespace CK3Analyser.Analysis.Detectors
{
    public class MagicNumberDetector : BaseDetector
    {
        public struct Settings
        {
            public Severity Severity { get; set; }
            public HashSet<string> StatementKeysToConsider { get; set; }
        }

        private Settings _settings;

        public MagicNumberDetector(ILogger logger, Context context, Settings settings) : base(logger, context)
        {
            _settings = settings;
        }

        public override void VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            if (!_settings.StatementKeysToConsider.Contains(binaryExpression.Key))
                return;

            if (int.TryParse(binaryExpression.Value, out int _))
            {
                logger.Log(
                    Smell.MagicNumber,
                    _settings.Severity,
                    $"Use of literal number as an argument for '{binaryExpression.Key}'; use a script value instead!",
                    binaryExpression.GetIdentifier());
            }
        }
    }
}
