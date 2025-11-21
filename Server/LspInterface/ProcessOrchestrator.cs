using CK3Analyser.Analysis;
using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Parsing;
using CK3Analyser.Core.Parsing.Antlr;
using CK3Analyser.Core.Resources;
using CK3Analyser.Core.Resources.Semantics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CK3Analyser.LspInterface
{
    public class ProcessOrchestrator
    {
        public bool Initiate(JsonElement jsonElement, out string response)
        {
            GlobalResources.ClearEverything();


            var vanillaPath = jsonElement.GetProperty("vanillaCk3Path").GetString();
            if (string.IsNullOrEmpty(vanillaPath))
            {
                response = "Missing VanillaPath setting";
                return false;
            }
            if (!Directory.Exists(vanillaPath))
            {
                response = "VanillaPath setting points to a non-existent directory";
                return false;
            }

            var logsFolderPath = jsonElement.GetProperty("logsFolderPath").GetString();
            if (string.IsNullOrEmpty(logsFolderPath))
            {
                response = "Missing LogsFolderPath setting";
                return false;
            }
            if (!Directory.Exists(logsFolderPath))
            {
                response = "LogsFolderPath setting points to a non-existent directory";
                return false;
            }

            var modPath = jsonElement.GetProperty("modPath").GetString();
            if (!string.IsNullOrWhiteSpace(modPath) && !Directory.Exists(modPath))
            {
                response = "ModPath setting points to a non-existent directory";
                return false;
            }

            if (string.IsNullOrWhiteSpace(modPath))
                modPath = jsonElement.GetProperty("environmentPath").GetString();

            LogsParser.ParseLogs(logsFolderPath);

            GlobalResources.Old = new Context(vanillaPath, ContextType.Old);
            GlobalResources.Modded = new Context(modPath, ContextType.Modded);

            GlobalResources.Configuration = new Configuration(jsonElement);
            GlobalResources.SymbolTable = new SymbolTable();

            response = "Settings loaded succesfully: " + GlobalResources.Configuration.ToString();
            return true;
        }


        public async Task<IEnumerable<LogEntry>> HandleAnalysis(Func<string, Task> progressDelegate)
        {
            if (GlobalResources.Configuration.VanillaFileHandling == VanillaFileHandling.IgnoreEntirely)
            {
                ParsingService.BlacklistVanillaFilesInModContext(GlobalResources.Modded, GlobalResources.Old, progressDelegate);
            }

            await ParsingService.ParseAllEntities(() => new AntlrParser(), GlobalResources.Modded, progressDelegate);
            await progressDelegate("Mod parsing complete!");

            //if (GlobalResources.Configuration.ReadVanilla || GlobalResources.Configuration.VanillaFileHandling == VanillaFileHandling.AnalyseModsAdditions)
            //{
            //    await ParsingService.ParseMacroEntities(() => new AntlrParser(), GlobalResources.Old, progressDelegate);
            //}

            var analyser = new Analyser();
            await analyser.Analyse(GlobalResources.Modded, progressDelegate);
            return analyser.LogEntries;
        }

        internal void WrapUp()
        {
            GlobalResources.ClearEverything();
        }
    }
}
