using System;
using System.Collections.Generic;
using System.IO;

namespace CK3Analyser.Core.Resources
{
    public static class LogsParser
    {
        private const string _sectionHeader = "--------------------";

        public static void ParseLogs(string path)
        {
            string effectsLogPath = Path.Combine(path, "effects.txt");
            GlobalResources.AddEffects(GenericParse(effectsLogPath));
            string triggersLogPath = Path.Combine(path, "triggers.txt");
            GlobalResources.AddTriggers(GenericParse(triggersLogPath));
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
