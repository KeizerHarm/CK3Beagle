using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Resources;
using CK3BeagleServer.Core.Resources.DetectorSettings;
using System.Collections.Generic;
using System.Linq;

namespace CK3BeagleServer.Analysing.Common.Detectors
{
    public class KeywordAsScopeNameDetector : BaseDetector
    {
        private readonly KeywordAsScopeNameSettings _settings;

        private Dictionary<string, string> cachedMessages = new()
        {
            { "root", "Saving a scope under the name 'root'; this is almost certainly a misunderstanding. Use a more specific name for clarity" },
            { "this", "Saving a scope under the name 'this'; this is almost certainly a misunderstanding. Use a more specific name for clarity" },
            { "prev", "Saving a scope under the name 'prev'; this is almost certainly a misunderstanding. Use a more specific name for clarity" }
        };

        public KeywordAsScopeNameDetector(ILogger logger, Context context, KeywordAsScopeNameSettings settings) : base(logger, context)
        {
            _settings = settings;
        }

        public override void VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            if (binaryExpression.Key == "save_scope_as" || binaryExpression.Key == "save_temporary_scope_as")
            {
                ValidateUsedScopeName(binaryExpression.Value, binaryExpression);
            }
        }

        public override void EnterNamedBlock(NamedBlock namedBlock)
        {
            if (namedBlock.Key == "save_scope_value_as" || namedBlock.Key == "save_temporary_scope_value_as")
            {
                var value = namedBlock.Children
                    .OfType<BinaryExpression>()
                    .FirstOrDefault(x => x.Key == "name")?.Value;

                ValidateUsedScopeName(value, namedBlock);
            }
        }

        private void ValidateUsedScopeName(string value, Node node)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            var lowercaseValue = value.ToLower();
            if (lowercaseValue == "root" || lowercaseValue == "prev" || lowercaseValue == "this")
            {
                logger.Log(
                    Smell.KeywordAsScopeName_RootPrevThis,
                    _settings.RootPrevThis_Severity,
                    cachedMessages[lowercaseValue],
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
