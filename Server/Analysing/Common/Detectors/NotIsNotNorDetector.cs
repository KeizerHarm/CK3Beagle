using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Resources.DetectorSettings;
using System;
using System.Linq;

namespace CK3BeagleServer.Analysing.Common.Detectors
{
    public class NotIsNotNorDetector : BaseDetector
    {
        private readonly NotIsNotNorSettings _settings;

        public NotIsNotNorDetector(ILogger logger, Context context, NotIsNotNorSettings settings) : base(logger, context)
        {
            _settings = settings;
        }

        public override void EnterNamedBlock(NamedBlock namedBlock)
        {
            if (namedBlock.Key != "NOT")
                return;

            if (namedBlock.Children.Where(x => x.NodeType == NodeType.Trigger).Count() > 1)
            {
                logger.Log(
                    Smell.NotIsNotNor,
                    _settings.Severity,
                    "NOT containing multiple triggers",
                    namedBlock);
            }
        }
    }
}
