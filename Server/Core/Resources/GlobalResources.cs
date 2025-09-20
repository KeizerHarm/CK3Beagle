using CK3Analyser.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK3Analyser.Core.Resources
{
    public static class GlobalResources
    {
        public static HashSet<string> EFFECTKEYS { get; private set; }
        public static HashSet<string> TRIGGERKEYS { get; private set; }

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
    }
}
