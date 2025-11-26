using CK3Analyser.Core.Comparing.Building;
using CK3Analyser.Core.Comparing.Domain;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Generated;
using CK3Analyser.Core.Parsing;
using CK3Analyser.Core.Resources;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CK3Analyser.Core.Comparing
{
    public class ComparingService
    {
        public static void BlacklistVanillaFilesInModContext(Context modContext, Context vanillaContext, Func<string, Task> progressDelegate)
        {
            List<string> filesToRemove = [];

            // Not the real intersect - this takes place before actually parsing the mod (for performance reasons),
            // so grab wide for potential mod files regardless of what's actually parsed later on
            var potentialModFiles = Directory.GetFiles(modContext.Path, "*.txt", SearchOption.AllDirectories);
            foreach (var file in potentialModFiles)
            {
                var localPath = Path.GetRelativePath(modContext.Path, file);
                var vanillaAbsPath = Path.Combine(vanillaContext.Path, localPath);
                if (File.Exists(vanillaAbsPath))
                {
                    modContext.Blacklist.Add(localPath);
                }
            }
        }

        public static async Task ParseVanillaModIntersect(
            Func<ICk3Parser> parserMaker,
            Context modContext,
            Context vanillaContext,
            Func<string, Task> progressDelegate)
        {
            //First, determine and set the actual vanilla-mod file intersect
            var modFiles = modContext.Files.Keys;
            foreach (var file in modFiles)
            {
                var vanillaAbsPath = Path.Combine(vanillaContext.Path, file);
                if (File.Exists(vanillaAbsPath))
                {
                    GlobalResources.VanillaModIntersect.Add(file);
                    GlobalResources.Vanilla.Whitelist.Add(file);
                }
            }

            //Parse what's in the intersect.
            //Because these were already parsed in ParseMacroEntities, which necessarily also ran if this is being run
            var macroTypesToSkip = Enum.GetValues<DeclarationType>().Where(x => x.IsMacroType());
            foreach (var declarationType in Enum.GetValues<DeclarationType>().Except(macroTypesToSkip))
            {
                ParsingService.ParseAllDeclarationsOfType(parserMaker, vanillaContext, declarationType);
                await progressDelegate("Completed parsing vanilla " + declarationType.ToString());
            }

            await progressDelegate("Parsed all vanilla entities from files intersecting with mod");
        }


        public static async Task BuildContextComparison(Context modContext, Context vanillaContext, Func<string, Task> positiveProgressDelegate)
        {
            //Run the actual comparison builder on what's left
            var fileComparisons = new ConcurrentBag<FileComparison>();

            var comparisonMaker = () => new BlockComparisonBuilder();

            var contextComparison = new ContextComparison(vanillaContext, modContext);

            var files = contextComparison.ChangedFiles;

            var batchSize = 50;
            var fileBatches = files.Chunk(batchSize);
            Parallel.ForEach(fileBatches, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, batch =>
            {
                var comparisonBuilder = comparisonMaker();
                foreach (var localFilePath in batch)
                {
                    var vanillaFile = vanillaContext.Files[localFilePath];
                    var modFile = modContext.Files[localFilePath];

                    fileComparisons.Add(BuildFileComparison(modFile, vanillaFile));
                }
            });
        }

        public static void ClearMemoryUnusedForComparison(Context modContext)
        {
            //Clear declarations from mod context that are not in the file intersect
            for (int i = 0; i < modContext.Declarations.Length; i++)
            {
                Dictionary<string, Declaration> declType = modContext.Declarations[i];
                var declsToRemove = declType.Where(x => !GlobalResources.VanillaModIntersect.Contains(x.Value.File.RelativePath));
                modContext.Declarations[i] = declType.Except(declsToRemove).ToDictionary();
            }

            //Clear files from mod context that are not in the file intersect
            foreach (var fileNotInIntersect in modContext.Files.Keys.Except(GlobalResources.VanillaModIntersect))
            {
                modContext.Files.Remove(fileNotInIntersect);
            }

            //Trigger garbage collection
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
        }

        public static FileComparison BuildFileComparison(ScriptFile modFile, ScriptFile vanillaFile)
        {
            return new FileComparison(vanillaFile, modFile);
        }
    }
}
