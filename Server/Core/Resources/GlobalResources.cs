using CK3BeagleServer.Core.Comparing.Domain;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Resources.Storage;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

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
        public static HashSet<string> VanillaModIntersect;

        public static Configuration Configuration;
        public static SymbolTable SymbolTable;
        public static StringTable StringTable;

        public static IEnumerable<Delta> Deltas;

        public static bool Initiate(JsonElement json, out string message, bool isPartialRun = false)
        {
            message = "";
            if ((Configuration?.RawSettings != null && Configuration.RawSettings.Equals(json)))
            {
                message = "Settings unchanged, continuing with same configuration";
                ClearButKeepConfig();
                return true;
            }

            if (isPartialRun)
            {
                message = "Updated configuration for partial run!";
                ClearButKeepConfig();
                Configuration = new Configuration(json);
                return true;
            }

            ClearEverything();

            (bool success, string vanillaPath) = Helpers.GetFolderAndCheckExists(json, "vanillaCk3Path", out message);
            if (!success)
            {
                return false;
            }

            (success, string docsFolderPath) = Helpers.GetFolderAndCheckExists(json, "docsFolderPath", out message);
            if (!success)
            {
                return false;
            }

            success = DocsParser.ParseDocs(docsFolderPath, out message);
            if (!success)
            {
                return false;
            }

            var modPath = json.GetProperty("modPath").GetString();
            if (!string.IsNullOrWhiteSpace(modPath) && !Directory.Exists(Helpers.ExtendPath(modPath)))
            {
                message = "ModPath setting points to a non-existent directory";
                return false;
            }

            if (string.IsNullOrWhiteSpace(modPath))
                modPath = json.GetProperty("environmentPath").GetString();


            Vanilla = new Context(vanillaPath, ContextType.Vanilla);
            Modded = new Context(modPath, ContextType.Modded);

            Configuration = new Configuration(json);
            SymbolTable = new SymbolTable();
            StringTable = new StringTable();
            VanillaModIntersect = [];

            message = "Settings loaded succesfully: " + Configuration.ToString();
            return true;
        }

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
            var existingEffects = _effectKeys?.ToHashSet() ?? [];
            if (EFFECTKEYS != null)
                EFFECTKEYS = [.. EFFECTKEYS.Union(existingEffects)];
            else
                EFFECTKEYS = existingEffects;
            _effectKeys = null;

            var existingTriggers = _triggerKeys?.ToHashSet() ?? [];
            if (TRIGGERKEYS != null)
                TRIGGERKEYS = [.. TRIGGERKEYS.Union(existingTriggers)];
            else
                TRIGGERKEYS = existingTriggers;
            _triggerKeys = null;

            var existingEventTargets = _eventTargets?.ToHashSet() ?? [];
            if (EVENTTARGETS != null)
                EVENTTARGETS = [.. EVENTTARGETS.Union(existingEventTargets)];
            else
                EVENTTARGETS = existingEventTargets;
            _eventTargets = null;
        }

        public static void ClearEverything()
        {
            ClearButKeepConfig();
            Configuration = null;
            SymbolTable = null;
            StringTable = null;
            _effectKeys = null;
            _triggerKeys = null;
            EFFECTKEYS = null;
            TRIGGERKEYS = null;
        }

        public static void ClearButKeepConfig()
        {
            Vanilla?.ClearParsedData();
            Modded?.ClearParsedData();
            VanillaModIntersect = null;
            _eventTargets = null;
            EVENTTARGETS = null;
            Deltas = null;
        }
    }
}
