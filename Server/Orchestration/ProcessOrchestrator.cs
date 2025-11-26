using CK3Analyser.Analysing.Common;
using CK3Analyser.Analysing.Logging;
using CK3Analyser.Core.Comparing;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Parsing;
using CK3Analyser.Core.Parsing.Antlr;
using CK3Analyser.Core.Resources;
using CK3Analyser.Core.Resources.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CK3Analyser.Orchestration
{
    public class ProcessOrchestrator : IProcessOrchestrator
    {
        Func<string, Task> _positiveProgressDelegate;
        Func<string, Task> _negativeProgressDelegate;

        public ProcessOrchestrator(Func<string, Task> positiveProgressDelegate, Func<string, Task> negativeProgressDelegate)
        {
            _positiveProgressDelegate = positiveProgressDelegate;
            _negativeProgressDelegate = negativeProgressDelegate;
        }

        public async Task<bool> InitiateFromJson(JsonElement json)
        {
            GlobalResources.ClearEverything();

            (bool success, string vanillaPath) = await GetFolderAndCheckExists(json, "vanillaCk3Path");

            if (!success)
            {
                return false;
            }

            (success, string logsFolderPath) = await GetFolderAndCheckExists(json, "logsFolderPath");

            if (!success)
            {
                return false;
            }

            var modPath = json.GetProperty("modPath").GetString();
            if (!string.IsNullOrWhiteSpace(modPath) && !Directory.Exists(modPath))
            {
                await _negativeProgressDelegate("ModPath setting points to a non-existent directory");
                return false;
            }

            if (string.IsNullOrWhiteSpace(modPath))
                modPath = json.GetProperty("environmentPath").GetString();

            LogsParser.ParseLogs(logsFolderPath);

            GlobalResources.Vanilla = new Context(vanillaPath, ContextType.Vanilla);
            GlobalResources.Modded = new Context(modPath, ContextType.Modded);

            GlobalResources.Configuration = new Configuration(json);
            GlobalResources.SymbolTable = new SymbolTable();
            GlobalResources.StringTable = new StringTable();

            await _positiveProgressDelegate("Settings loaded succesfully: " + GlobalResources.Configuration.ToString());
            return true;

            async Task<(bool, string)> GetFolderAndCheckExists(JsonElement json, string folderName)
            {
                var folderPath = json.GetProperty(folderName).GetString();
                if (string.IsNullOrEmpty(folderPath))
                {
                    await _negativeProgressDelegate($"Missing {folderName} setting");
                    return (false, null);
                }
                if (!Directory.Exists(folderPath))
                {
                    await _negativeProgressDelegate($"{folderName} setting points to a non-existent directory");
                    return (false, null);
                }
                return (true, folderPath);
            }
        }

        public void InitiateFromMinimalConfig(string vanillaPath, string modPath, string logsPath)
        {
            LogsParser.ParseLogs(logsPath);
            GlobalResources.Vanilla = new Context(vanillaPath, ContextType.Vanilla);
            GlobalResources.Modded = new Context(modPath, ContextType.Modded);
            GlobalResources.Configuration = new Configuration(true);
            GlobalResources.SymbolTable = new SymbolTable();
            GlobalResources.StringTable = new StringTable();
        }

        public async Task<IEnumerable<LogEntry>> HandleAnalysis(bool reportTiming = false)
        {
            if (GlobalResources.Configuration.VanillaFileHandling == VanillaFileHandling.IgnoreEntirely)
            {
                ComparingService.BlacklistVanillaFilesInModContext(GlobalResources.Modded, GlobalResources.Vanilla, _positiveProgressDelegate);
            }

            await ParsingService.ParseAllEntities(() => new AntlrParser(), GlobalResources.Modded, _positiveProgressDelegate);
            await _positiveProgressDelegate("Mod parsing complete!");

            if (GlobalResources.Configuration.ReadVanilla || GlobalResources.Configuration.VanillaFileHandling == VanillaFileHandling.AnalyseModsAdditions)
            {
                await ParsingService.ParseMacroEntities(() => new AntlrParser(), GlobalResources.Vanilla, _positiveProgressDelegate);
            }
            if (GlobalResources.Configuration.VanillaFileHandling == VanillaFileHandling.AnalyseModsAdditions)
            {
                await ComparingService.ParseVanillaModIntersect(() => new AntlrParser(), GlobalResources.Modded, GlobalResources.Vanilla, _positiveProgressDelegate);
            }

            var analyser = new CommonAnalyser();
            await analyser.Analyse(GlobalResources.Modded, _positiveProgressDelegate);
            return analyser.LogEntries;
        }

        public async Task<IEnumerable<LogEntry>> HandleComparativeAnalysis(bool reportTiming = false)
        {
            ComparingService.ClearMemoryUnusedForComparison(GlobalResources.Modded);

            await _positiveProgressDelegate("Cleared unused ASTs");

            await ComparingService.BuildContextComparison(GlobalResources.Modded, GlobalResources.Vanilla, _positiveProgressDelegate);

            await _positiveProgressDelegate("Finished building comparison");

            var comparativeAnalyser = new CommonAnalyser();
            
            return comparativeAnalyser.LogEntries;

        }
        public void WrapUp()
        {
            GlobalResources.ClearEverything();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
        }
    }
}
