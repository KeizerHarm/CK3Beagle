using CK3BeagleServer.Analysing.Common;
using CK3BeagleServer.Analysing.Diff;
using CK3BeagleServer.Analysing.Logging;
using CK3BeagleServer.Core.Comparing;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Parsing;
using CK3BeagleServer.Core.Parsing.Antlr;
using CK3BeagleServer.Core.Resources;
using CK3BeagleServer.Core.Resources.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CK3BeagleServer.Orchestration
{
    public class ProcessOrchestrator
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
            bool success = GlobalResources.Initiate(json, out string message);
            if (!success)
            {
                await _negativeProgressDelegate(message);
                return false;
            }

            await _positiveProgressDelegate(message);
            return true;
        }

        public async Task<bool> InitiatePartialFromJson(JsonElement json)
        {
            bool success = GlobalResources.Initiate(json, out string message, true);
            if (!success)
            {
                await _negativeProgressDelegate(message);
                return false;
            }
            success = GetEditedFiles(json, out var files);
            if (!success)
            {
                await _negativeProgressDelegate("Issue parsing partial files");
                return false;
            }
            GlobalResources.Modded.Whitelist = [.. files];

            await _positiveProgressDelegate(message);
            return true;
        }

        private bool GetEditedFiles(JsonElement json, out string[] files)
        {
            files = null;
            var exists = json.TryGetProperty("editedFiles", out var filesValue);
            if (exists)
            {
                var absoluteFiles = filesValue.Deserialize<string[]>();
                files = absoluteFiles.Select(x => Path.GetRelativePath(GlobalResources.Modded.Path, x)).ToArray();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<LogEntry>> HandleAnalysis(bool reportTiming = false, bool isPartialRun = false)
        {
            var logEntries = await HandleCommonAnalysis(reportTiming, isPartialRun);
            if (GlobalResources.Configuration.ReadVanilla && !isPartialRun)
            {
                var diffLogEntries = await HandleComparativeAnalysis(reportTiming);
                logEntries = logEntries.Union(diffLogEntries);
            }
            return logEntries;
        }

        public void InitiateFromMinimalConfig(string vanillaPath, string modPath, string logsPath)
        {
            DocsParser.ParseDocs(logsPath, out string _);
            GlobalResources.Vanilla = new Context(vanillaPath, ContextType.Vanilla);
            GlobalResources.Modded = new Context(modPath, ContextType.Modded);
            GlobalResources.Configuration = new Configuration(new JsonElement(), true);
            GlobalResources.SymbolTable = new SymbolTable();
            GlobalResources.StringTable = new StringTable();
            GlobalResources.VanillaModIntersect = [];
        }

        public void InitiatePartialFromMinimalConfig(string vanillaPath, string modPath, string logsPath, params string[] absEditedFiles)
        {
            GlobalResources.Vanilla = new Context(vanillaPath, ContextType.Vanilla);
            GlobalResources.Modded = new Context(modPath, ContextType.Modded);
            GlobalResources.Configuration = new Configuration(new JsonElement(), true);
            GlobalResources.VanillaModIntersect = [];
            var files = absEditedFiles.Select(x => Path.GetRelativePath(GlobalResources.Modded.Path, x)).ToArray();

            GlobalResources.Modded.Whitelist = [.. files];
        }

        private async Task<IEnumerable<LogEntry>> HandleCommonAnalysis(bool reportTiming = false, bool isPartialRun = false)
        {
            if (GlobalResources.Configuration.VanillaFileHandling == VanillaFileHandling.IgnoreEntirely)
            {
                ComparingService.BlacklistVanillaFilesInModContext(GlobalResources.Modded, GlobalResources.Vanilla, _positiveProgressDelegate);
            }

            if (!isPartialRun)
            {
                await ParsingService.ParseMacroEntities(() => new AntlrParser(), GlobalResources.Vanilla, _positiveProgressDelegate);
            }
            await ParsingService.ParseAllEntities(() => new AntlrParser(), GlobalResources.Modded, _positiveProgressDelegate, isPartialRun);
            await _positiveProgressDelegate("Mod parsing complete!");

            if (!isPartialRun && (GlobalResources.Configuration.ReadVanilla || GlobalResources.Configuration.VanillaFileHandling == VanillaFileHandling.AnalyseModsAdditions))
            {
                await ComparingService.ParseVanillaModIntersect(() => new AntlrParser(), GlobalResources.Modded, GlobalResources.Vanilla, _positiveProgressDelegate);
            }

            var analyser = new CommonAnalyser();
            await analyser.Analyse(GlobalResources.Modded, _positiveProgressDelegate);
            return analyser.LogEntries;
        }

        private async Task<IEnumerable<LogEntry>> HandleComparativeAnalysis(bool reportTiming = false)
        {
            ComparingService.ClearMemoryUnusedForComparison(GlobalResources.Modded);

            await _positiveProgressDelegate("Cleared unused ASTs");

            await ComparingService.BuildContextComparison(GlobalResources.Modded, GlobalResources.Vanilla, _positiveProgressDelegate);

            await _positiveProgressDelegate("Finished building context comparisons");
            var comparativeAnalyser = new DiffAnalyser();
            await comparativeAnalyser.Analyse(GlobalResources.Deltas, _positiveProgressDelegate);
            return comparativeAnalyser.LogEntries;
        }

        public void WrapUp()
        {
            GlobalResources.ClearButKeepConfig();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
        }
    }
}
