using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Resources.DetectorSettings;

namespace CK3BeagleServer.Analysing.Common.Detectors
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
            if (!_settings.KeysToConsider.Contains(binaryExpression.Key))
                return;

            if (IsNumeric(binaryExpression.Value))
            {
                //Don't count 0 or 1 as magic numbers.
                var parsed = float.Parse(binaryExpression.Value);
                if (parsed == 0 || parsed == 1)
                    return;

                logger.Log(
                    Smell.MagicNumber,
                    _settings.Severity,
                    $"Use of literal number as an argument for '{binaryExpression.Key}'; use a script value instead!",
                    binaryExpression);
            }
        }

        private static bool IsNumeric(string s)
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
