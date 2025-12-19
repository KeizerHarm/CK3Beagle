using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Generated;
using CK3BeagleServer.Core.Parsing.SemanticPass;
using CK3BeagleServer.Core.Resources;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CK3BeagleServer.Core.Parsing
{
    public class ParsingService
    {
        public static async Task ParseAllEntities(Func<ICk3Parser> parserMaker, Context context, Func<string, Task> progressDelegate = null, bool isPartialRun = false)
        {
            foreach (var declarationType in Enum.GetValues<DeclarationType>())
            {
                ParseAllDeclarationsOfType(parserMaker, context, declarationType);
                if (!isPartialRun) {
                    await progressDelegate("Completed parsing " + declarationType.ToString() + "s");
                }
            }
            GlobalResources.Lock();
            new SemanticPassHandler().ExecuteSemanticPass(context);
        }

        public static void ParseAllDeclarationsOfType(Func<ICk3Parser> parserMaker, Context context, DeclarationType declarationType)
        {
            var entityFiles = new ConcurrentBag<ScriptFile>();

            var entityHome = Path.Combine(context.Path, declarationType.GetEntityHome());
            if (Directory.Exists(entityHome))
            {
                string[] files = Directory.GetFiles(entityHome, "*.txt", SearchOption.AllDirectories)
                    .Where(x => Filter(context, Path.GetRelativePath(context.Path, x))).ToArray();

                if (files.Length == 0)
                    return;

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

        private static bool Filter(Context context, string path)
        {
            if (context.Whitelist.Count > 0 && !context.Whitelist.Contains(path))
                return false;

            if (context.Blacklist.Contains(path))
                return false;

            return true;
        }

        private static void ParseScriptFile(ScriptFile scriptfile, ICk3Parser parser)
        {
            parser.ParseFile(scriptfile);
            if (scriptfile.ExpectedDeclarationType == DeclarationType.ScriptedEffect)
            {
                GlobalResources.AddEffects(scriptfile.Declarations.Select(x => x.Key));
            }
            if (scriptfile.ExpectedDeclarationType == DeclarationType.ScriptedTrigger)
            {
                GlobalResources.AddTriggers(scriptfile.Declarations.Select(x => x.Key));
            }
        }

        public static async Task ParseMacroEntities(Func<ICk3Parser> parserMaker, Context context, Func<string, Task> progressDelegate)
        {
            ParseAllDeclarationsOfType(parserMaker, context, DeclarationType.ScriptedEffect);
            await progressDelegate("Parsed vanilla Scripted Effects");
            ParseAllDeclarationsOfType(parserMaker, context, DeclarationType.ScriptedTrigger);
            await progressDelegate("Parsed vanilla Scripted Triggers");
        }

    }
}
