using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources.DetectorSettings;

namespace CK3Analyser.Analysis.Detectors
{
    public class MagicNumberDetector : BaseDetector
    {
        private readonly MagicNumberSettings _settings;

        public MagicNumberDetector(ILogger logger, Context context, MagicNumberSettings settings) : base(logger, context)
        {
            _settings = settings;
        }

        public override void VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            if (!_settings.StatementKeysToConsider.Contains(binaryExpression.Key))
                return;

            if (IsNumeric(binaryExpression.Value))
            {
                logger.Log(
                    Smell.MagicNumber,
                    _settings.Severity,
                    $"Use of literal number as an argument for '{binaryExpression.Key}'; use a script value instead!",
                    binaryExpression);
            }
        }

        private bool IsNumeric(string s)
        {
            if (s.Length == 0) return false;
            int i = 0;
            if (s[0] == '-' || s[0] == '+') i++;
            for (; i < s.Length; i++)
                if (s[i] != '.' && (s[i] < '0' || s[i] > '9'))
                    return false;
            return true;
        }
    }
}
