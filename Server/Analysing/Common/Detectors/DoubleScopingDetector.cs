using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Resources.DetectorSettings;

namespace CK3BeagleServer.Analysing.Common.Detectors
{
    public class DoubleScopingDetector : BaseDetector
    {
        private readonly DoubleScopingSettings _settings;

        public DoubleScopingDetector(ILogger logger, Context context, DoubleScopingSettings settings) : base(logger, context)
        {
            _settings = settings;
        }

        //public override void EnterNamedBlock(NamedBlock namedBlock)
        //{
        //    var mostRecentScopeChange = FindMostRecentScopeChange(namedBlock.Parent);
        //    if (mostRecentScopeChange != null)
        //        return;


        //    var key = namedBlock.Key;
        //    var scopeKey = GetScopeKey(key);

        //    if ()

        //}

        //private static string FindMostRecentScopeChange(Block block)
        //{
        //    if (block == null || block.NodeType != NodeType.Link)
        //        return null;

        //    if (block is NamedBlock namedBlock)
        //    {
        //        var scope = GetScopeKey(namedBlock.Key);
        //        if (scope != null)
        //        {
        //            return scope;
        //        }
        //    }

        //    return FindMostRecentScopeChange(block.Parent);
        //}

        //private static string GetScopeKey(string key)
        //{
        //    var foundScope = false;
        //    for (int i = 0; i < key.Length; i++)
        //    {
        //        var c = key[i];
        //        if (c == '.')
        //        {
        //            if (foundScope)
        //            {
        //                return key.Substring(i);
        //            }
        //            return null;
        //        }
        //        if (c == ':')
        //        {
        //            foundScope = true;
        //        }
        //    }

        //    return foundScope ? key : null;
        //}
    }
}