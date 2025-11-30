using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Resources;
using CK3BeagleServer.Core.Resources.DetectorSettings;
using System.Linq;

namespace CK3BeagleServer.Analysing.Common.Detectors
{
    internal class KeywordAsScopeNameDetector : BaseDetector
    {
        private readonly KeywordAsScopeNameSettings _settings;

        public KeywordAsScopeNameDetector(ILogger logger, Context context, KeywordAsScopeNameSettings settings) : base(logger, context)
        {
            _settings = settings;
        }

        public override void VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            if (binaryExpression.Key == "save_scope_as" || binaryExpression.Key == "save_temporary_scope_as")
            {
                Sniff(binaryExpression.Value, binaryExpression);
            }
        }

        public override void EnterNamedBlock(NamedBlock namedBlock)
        {
            if (namedBlock.Key == "save_scope_value_as" || namedBlock.Key == "save_temporary_scope_value_as")
            {
                var value = ((BinaryExpression)namedBlock.Children.FirstOrDefault(x => x is BinaryExpression binExp && binExp.Key == "name")).Value;
                Sniff(value, namedBlock);
            }
        }

        private void Sniff(string value, Node node)
        {
            var lowercaseValue = value.ToLower();
            if (lowercaseValue == "root" || lowercaseValue == "prev")
            {
                logger.Log(
                    Smell.KeywordAsScopeName_RootOrPrev,
                    _settings.RootOrPrev_Severity,
                    $"Saving a scope under the name {lowercaseValue}; this is almost certainly a misunderstanding. Use a more specific name for clarity",
                    node);
                return;
            }

            if (GlobalResources.EVENTTARGETS.Contains(lowercaseValue))
            {
                logger.Log(
                    Smell.KeywordAsScopeName_ScopeLink,
                    _settings.ScopeLink_Severity,
                    "Saving a scope under the name of a scope link; use a more specific name for clarity",
                    node);
                return;
            }
        }
        
    }
}
