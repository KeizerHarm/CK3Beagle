using CK3Analyser.Core.Parsing.Antlr;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Resources;
using System;
using System.Diagnostics;
using CK3Analyser.Core.Parsing;
using System.Linq;
using CK3Analyser.Analysis;
using System.Threading.Tasks;
using CK3Analyser.Core.Resources.Storage;
using System.Collections.Generic;
using CK3Analyser.Analysis.Logging;

namespace CK3Analyser.CLI
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var program = new Program();
            await program.Go();
        }

        private static string OldVanillaPath = @"C:\Program Files (x86)\Steam\steamapps\common\Crusader Kings III\game";
        private static string ModdedPath = @"C:\Users\Harm\Documents\Paradox Interactive\Crusader Kings III\mod\T4N-CK3\T4N";
        private static string NewVanillaPath = @"C:\Program Files (x86)\Steam\steamapps\common\Crusader Kings III\game";
        private static string LogsFolder = @"C:\Users\Harm\Documents\Paradox Interactive\Crusader Kings III\logs";

        private async Task Go()
        {
            Task progressDelegate(string msg)
            {
                Console.WriteLine(msg);
                return Task.CompletedTask;
            }

            Console.WriteLine("Let's go! Parsing logs");
            LogsParser.ParseLogs(LogsFolder);
            //Console.WriteLine($"Parsed logs; found {GlobalResources.EFFECTKEYS.Count} effects, {GlobalResources.TRIGGERKEYS.Count} triggers, {GlobalResources.EVENTTARGETS.Count} event targets ");

            //var inputDir = OldVanillaPath;

            GlobalResources.Old = new Context(OldVanillaPath, ContextType.Old);
            GlobalResources.Modded = new Context(ModdedPath, ContextType.Modded);
            GlobalResources.New = new Context(NewVanillaPath, ContextType.New);
            GlobalResources.Configuration = new Configuration(true);
            GlobalResources.SymbolTable = new SymbolTable();
            GlobalResources.StringTable = new StringTable();

            //ParsingService.BlacklistVanillaFilesInModContext(GlobalResources.Modded, GlobalResources.Old, progressDelegate);
            //var fastParser = new FastParser();
            //var antlrParser = new AntlrParser();

            //stopwatch.Start();
            //GatherDeclarations(fastParser, Old);

            //stopwatch.Stop();
            //Console.WriteLine($"Elapsed (old vanilla): {stopwatch.Elapsed}");

            var parsingTimer = new Stopwatch();
            parsingTimer.Start();
            //GatherDeclarationsForDeclarationType(antlrParser, GlobalResources.Old, DeclarationType.ScriptedTrigger);
            await ParsingService.ParseAllEntities(() => new AntlrParser(), GlobalResources.Modded, progressDelegate);

            await ParsingService.ParseMacroEntities(() => new AntlrParser(), GlobalResources.Old, progressDelegate);
            await ParsingService.ParseVanillaEntitiesInMod(() => new AntlrParser(), GlobalResources.Modded, GlobalResources.Old, progressDelegate);

            parsingTimer.Stop();

            //var culturesGfxes = GlobalResources.Old.Declarations[(int)DeclarationType.Culture]
            //    .Select(x => x.Value.Children.OfType<NamedBlock>().First(c => c.Key == "clothing_gfx").Children.Select(c => c.Raw).First())
            //    .Distinct();

            //var gfxesWithCultures = new Dictionary<string, IEnumerable<string>>();

            //foreach (var gfx in culturesGfxes)
            //{
            //    gfxesWithCultures.Add(gfx,
            //        GlobalResources.Old.Declarations[(int)DeclarationType.Culture]
            //            .Where(x => x.Value.Children.OfType<NamedBlock>().First(c => c.Key == "clothing_gfx").Children.Select(c => c.Raw)
            //            .First() == gfx).Select(x => x.Value.Key));
            //}

            //foreach (var gfx in gfxesWithCultures.OrderByDescending(x => x.Value.Count()))
            //{
            //    Console.WriteLine($"{gfx.Key} has {gfx.Value.Count()} cultures: {string.Join(", ", gfx.Value)}");
            //    Console.WriteLine();
            //}

            var analysisTimer = new Stopwatch();
            analysisTimer.Start();
            var analyser = new Analyser();
            await analyser.Analyse(GlobalResources.Modded, progressDelegate);
            analysisTimer.Stop();
            Console.WriteLine($"Analysed a total of {(GlobalResources.Modded.Files.Count + GlobalResources.Old.Files.Count)} files");
            Console.WriteLine($"Found { analyser.LogEntries.Count()} issues");
            Console.WriteLine($"Parsing time: {parsingTimer.Elapsed}");
            Console.WriteLine($"Analysis time: {analysisTimer.Elapsed}");
            //stopwatch.Restart();
            //GatherDeclarations(fastParser, Modded);
            //stopwatch.Stop();
            //Console.WriteLine($"Elapsed (old mod): {stopwatch.Elapsed}");
            //stopwatch.Restart();
            //GatherDeclarations(fastParser, New);
            //stopwatch.Stop();
            //Console.WriteLine($"Elapsed (new vanilla): {stopwatch.Elapsed}");
       }

        private object GetReturnMessage(IEnumerable<LogEntry> logEntries)
        {
            return new
            {
                type = "analysis",
                payload = new
                {
                    summary = $"Found {logEntries.Count()} issues",
                    smells = string.Join(',', logEntries.Select(x =>
                        new
                        {
                            severity = x.Severity,
                            file = x.Location,
                            startLine = x.AffectedAreaStartLine,
                            endLine = x.AffectedAreaEndLine,
                            startIndex = x.AffectedAreaStartIndex,
                            endIndex = x.AffectedAreaEndIndex,
                            message = x.Message,
                            key = x.Smell.GetCode(),
                            relatedLogEntries = string.Join(',', x.RelatedLogEntries.Select(y =>
                                new
                                {
                                    file = y.Location,
                                    startLine = y.AffectedAreaStartLine,
                                    endLine = y.AffectedAreaEndLine,
                                    startIndex = y.AffectedAreaStartIndex,
                                    endIndex = y.AffectedAreaEndIndex,
                                    message = y.Message
                                }
                            ))
                        }
                    ))
                }
            };
        }
    }
}
