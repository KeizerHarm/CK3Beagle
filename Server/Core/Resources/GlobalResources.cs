using CK3Analyser.Core.Domain;
using System.Collections.Generic;

namespace CK3Analyser.Core.Resources
{
    public static class GlobalResources
    {
        public static HashSet<string> EFFECTKEYS { get; private set; }
        public static HashSet<string> TRIGGERKEYS { get; private set; }
        public static HashSet<string> EVENTTARGETS { get; private set; }

        public static Context Old {  get; set; }
        public static Context Modded { get; set; }
        public static Context New { get; set; }

        public static void AddEffects(IEnumerable<string> effects)
        {
            EFFECTKEYS ??= new HashSet<string>();
            EFFECTKEYS.UnionWith(effects);
        }

        public static void AddTriggers(IEnumerable<string> triggers)
        {
            TRIGGERKEYS ??= new HashSet<string>();
            TRIGGERKEYS.UnionWith(triggers);
        }

        public static void AddEventTargets(IEnumerable<string> eventTargets)
        {
            EVENTTARGETS ??= new HashSet<string>();
            EVENTTARGETS.UnionWith(eventTargets);
        }
    }
}
