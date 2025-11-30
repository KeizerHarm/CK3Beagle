using System.Collections.Generic;
using System.IO;

namespace CK3BeagleServer.Core.Resources
{
    public static class LogsParser
    {
        private const string _sectionHeader = "--------------------";

        public static void ParseLogs(string path)
        {
            string effectsLogPath = Path.Combine(path, "effects.log");
            GlobalResources.AddEffects(GenericParse(effectsLogPath));
            string triggersLogPath = Path.Combine(path, "triggers.log");
            GlobalResources.AddTriggers(GenericParse(triggersLogPath));
            string eventTargetsPath = Path.Combine(path, "event_targets.log");
            GlobalResources.AddEventTargets(GenericParse(eventTargetsPath));
        }

        public static HashSet<string> GenericParse(string logPath)
        {
            var file = File.OpenText(logPath);

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
