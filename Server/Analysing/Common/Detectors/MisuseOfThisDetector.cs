using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Resources.DetectorSettings;

namespace CK3BeagleServer.Analysing.Common.Detectors
{
    public class MisuseOfThisDetector : BaseDetector
    {
        private readonly MisuseOfThisSettings _settings;

        public MisuseOfThisDetector(ILogger logger, Context context, MisuseOfThisSettings settings) : base(logger, context)
        {
            _settings = settings;
        }

        public override void EnterNamedBlock(NamedBlock namedBlock)
        {
            if (namedBlock.Key != "this")
                return;

            logger.Log(
                Smell.MisuseOfThis,
                _settings.Severity,
                "Attempt to scope to 'this'",
                namedBlock);
        }
    }
}
