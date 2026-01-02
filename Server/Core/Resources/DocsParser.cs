using System.Collections.Generic;
using System.IO;

namespace CK3BeagleServer.Core.Resources
{
    public static class DocsParser
    {
        private const string _sectionHeader = "--------------------";
        private const string _sharedErrorMessage = " Ensure this setting points to a folder with the CK3-generated trigger, effect and event target documentation logs!";

        public static bool ParseDocs(string path, out string message)
        {
            if (!GetKeys(path, "effects.log", out message, out HashSet<string> effectKeys))
                return false;
            GlobalResources.AddEffects(effectKeys);
            if (!GetKeys(path, "triggers.log", out message, out HashSet<string> triggerKeys))
                return false;
            GlobalResources.AddTriggers(triggerKeys);
            if (!GetKeys(path, "event_targets.log", out message, out HashSet<string> eventTargetsKeys))
                return false;
            GlobalResources.AddEventTargets(eventTargetsKeys);
            return true;
        }

        private static bool GetKeys(string path, string fileName, out string message, out HashSet<string> parsedKeys)
        {
            message = null; parsedKeys = [];
            string docsPath = Path.Combine(path, fileName);
            if (!File.Exists(docsPath))
            {
                message = "Cannot find file " + fileName + " in " + path + "." + _sharedErrorMessage;
                return false;
            }
            parsedKeys = GenericParse(docsPath);
            if (parsedKeys.Count < 10)
            {
                message = "Could not find any entries in " + docsPath + "." + _sharedErrorMessage;
                return false;
            }

            return true;
        }

        public static HashSet<string> GenericParse(string docPath)
        {
            var file = File.OpenText(docPath);

            var keys = new HashSet<string>();

            bool hadSectionHeader = false;
            while (!file.EndOfStream)
            {
                var line = file.ReadLine();

                if (line == _sectionHeader)
                {
                    hadSectionHeader = true;
                }

                if (line.Contains(" - ") && hadSectionHeader)
                {
                    keys.Add(line.Split('-')[0].Trim());
                    hadSectionHeader = false;
                }
            }

            file.Dispose();
            return keys;
        }

    }
}
