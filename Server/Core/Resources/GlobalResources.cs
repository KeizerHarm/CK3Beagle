using CK3BeagleServer.Core.Comparing.Domain;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Resources.Storage;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CK3BeagleServer.Core.Resources
{
    public static class GlobalResources
    {

        private static ConcurrentBag<string> _effectKeys;
        private static ConcurrentBag<string> _triggerKeys;
        private static ConcurrentBag<string> _eventTargets;
        public static HashSet<string> EFFECTKEYS { get; private set; }
        public static HashSet<string> TRIGGERKEYS { get; private set; }
        public static HashSet<string> EVENTTARGETS { get; private set; }

        public static Context Vanilla;
        public static Context Modded;
        public static HashSet<string> VanillaModIntersect = [];

        public static Configuration Configuration;
        public static SymbolTable SymbolTable;
        public static StringTable StringTable;

        public static IEnumerable<Delta> Deltas;

        public static void AddEffects(IEnumerable<string> effects)
        {
            _effectKeys ??= new ConcurrentBag<string>();
            foreach (var effect in effects) {
                _effectKeys.Add(effect.ToLowerInvariant());
            }
        }

        public static void AddTriggers(IEnumerable<string> triggers)
        {
            _triggerKeys ??= new ConcurrentBag<string>();
            foreach (var trigger in triggers)
            {
                _triggerKeys.Add(trigger.ToLowerInvariant());
            }
        }

        public static void AddEventTargets(IEnumerable<string> eventTargets)
        {
            _eventTargets ??= new ConcurrentBag<string>();
            foreach (var eventTarget in eventTargets)
            {
                _eventTargets.Add(eventTarget.ToLowerInvariant());
            }
        }

        public static void Lock()
        {
            EFFECTKEYS = _effectKeys.ToHashSet() ?? new HashSet<string>();
            TRIGGERKEYS = _triggerKeys?.ToHashSet() ?? new HashSet<string>();
            EVENTTARGETS = _eventTargets?.ToHashSet() ?? new HashSet<string>();

            _effectKeys = null;
            _triggerKeys = null;
            _eventTargets = null;
        }

        public static void ClearEverything()
        {
            Vanilla = null;
            Modded = null;

            _effectKeys = null;
            _triggerKeys = null;
            _eventTargets = null;
            EFFECTKEYS = null;
            TRIGGERKEYS = null;
            EVENTTARGETS = null;

            Configuration = null;
            SymbolTable = null;
            StringTable = null;
        }
    }
}
