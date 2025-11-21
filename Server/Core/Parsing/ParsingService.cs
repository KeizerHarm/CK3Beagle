using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Parsing.SemanticPass;
using CK3Analyser.Core.Resources;
using CK3Analyser.Core.Generated;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CK3Analyser.Core.Parsing.Antlr;
using System.Diagnostics.CodeAnalysis;

namespace CK3Analyser.Core.Parsing
{
    public class ParsingService
    {
        public static async Task ParseAllEntities(Func<ICk3Parser> parserMaker, Context context, Func<string, Task> progressDelegate = null)
        {
            //Console.WriteLine($"Now reading files from {context.Path}");

            //ReadEverythingAsync(parserMaker, context, DeclarationType.Culture);

            foreach (var declarationType in Enum.GetValues<DeclarationType>())
            {
                ReadEverythingAsync(parserMaker, context, declarationType);
                await progressDelegate("Completed parsing " + declarationType.ToString());
            }
            GlobalResources.Lock();
            Console.WriteLine("Done with first pass");
            new SemanticPassHandler().ExecuteSemanticPass(context);
            //Console.WriteLine("Done with second pass");
        }

        public static void ReadEverythingAsync(Func<ICk3Parser> parserMaker, Context context, DeclarationType declarationType)
        {
            var entityFiles = new ConcurrentBag<ScriptFile>();

            var entityHome = Path.Combine(context.Path, declarationType.GetEntityHome());
            if (Directory.Exists(entityHome))
            {
                var files = Directory.GetFiles(entityHome, "*.txt", SearchOption.AllDirectories)
                    .Except(context.Blacklist).ToArray();

                //Console.WriteLine($"Found {files.Length} {declarationType.ToString()} files");

                var batchSize = 50;
                var fileBatches = files.Chunk(batchSize);
                Parallel.ForEach(fileBatches, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, batch =>
                {
                    var parser = parserMaker();
                    foreach (var filePath in batch)
                    {
                        var input = File.ReadAllText(filePath);
                        var scriptFile = new ScriptFile(context, Path.GetRelativePath(context.Path, filePath), declarationType, input);
                        scriptFile.AbsolutePath = filePath;

                        ParseScriptFile(scriptFile, parser);
                        entityFiles.Add(scriptFile);
                    }
                });
            }

            foreach (var file in entityFiles)
            {
                context.AddFile(file);
            }
        }

        private static void ParseScriptFile(ScriptFile scriptfile, ICk3Parser parser)
        {
            parser.ParseFile(scriptfile);
            if (scriptfile.ExpectedDeclarationType == DeclarationType.ScriptedEffect)
            {
                GlobalResources.AddEffects(scriptfile.Declarations.Keys);
            }
            if (scriptfile.ExpectedDeclarationType == DeclarationType.ScriptedTrigger)
            {
                GlobalResources.AddTriggers(scriptfile.Declarations.Keys);
            }
        }

        public static void BlacklistVanillaFilesInModContext(Context modContext, Context vanillaContext, Func<string, Task> progressDelegate)
        {
            List<string> filesToRemove = [];
            var potentialModFiles = Directory.GetFiles(modContext.Path, "*.txt", SearchOption.AllDirectories);
            foreach (var file in potentialModFiles)
            {
                var localPath = Path.GetRelativePath(modContext.Path, file);
                var vanillaAbsPath = Path.Combine(vanillaContext.Path, localPath);
                if (File.Exists(vanillaAbsPath))
                {
                    modContext.BlacklistFile(file);
                }
            }
        }

        public static async Task ParseMacroEntities(Func<ICk3Parser> parserMaker, Context context, Func<string, Task> progressDelegate)
        {
            throw new NotImplementedException();
        }
    }
}
