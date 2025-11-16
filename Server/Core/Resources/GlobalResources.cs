using CK3Analyser.Core.Domain;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Core.Resources
{
    public static class GlobalResources
    {
        public static HashSet<string> EFFECTKEYS { get; private set; }
        public static HashSet<string> TRIGGERKEYS { get; private set; }
        public static HashSet<string> EVENTTARGETS { get; private set; }

        private static ConcurrentBag<string> _effectKeys;
        private static ConcurrentBag<string> _triggerKeys;
        private static ConcurrentBag<string> _eventTargets;

        public static Context Old {  get; set; }
        public static Context Modded { get; set; }
        public static Context New { get; set; }

        public static Configuration Configuration { get; set; }

        public static void AddEffects(IEnumerable<string> effects)
        {
            _effectKeys ??= new ConcurrentBag<string>();
            foreach (var effect in effects) {
                _effectKeys.Add(effect);
            }
        }

        public static void AddTriggers(IEnumerable<string> triggers)
        {
            _triggerKeys ??= new ConcurrentBag<string>();
            foreach (var trigger in triggers)
            {
                _triggerKeys.Add(trigger);
            }
        }

        public static void AddEventTargets(IEnumerable<string> eventTargets)
        {
            _eventTargets ??= new ConcurrentBag<string>();
            foreach (var eventTarget in eventTargets)
            {
                _eventTargets.Add(eventTarget);
            }
        }

        public static void Lock()
        {
            EFFECTKEYS = _effectKeys.ToHashSet() ?? new HashSet<string>();
            TRIGGERKEYS = _triggerKeys?.ToHashSet() ?? new HashSet<string>();
            EVENTTARGETS = _eventTargets?.ToHashSet() ?? new HashSet<string>();
        }

        public static void ClearEverything()
        {
            Old = null;
            New = null;
            Modded = null;
            _effectKeys = null;
            _triggerKeys = null;
            _eventTargets = null;
            EFFECTKEYS = null;
            TRIGGERKEYS = null;
            EVENTTARGETS = null;
            Configuration = null;
        }
    }
}
