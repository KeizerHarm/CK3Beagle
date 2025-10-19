using CK3Analyser.Core.Parsing.Antlr;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Parsing.Fast;
using CK3Analyser.Core.Resources;
using System;
using System.Diagnostics;
using CK3Analyser.Core.Parsing;

namespace CK3Analyser.CLI
{
    public class Program
    {
        static void Main(string[] args)
        {
            _ = new Program();
        }
        
        private static string OldVanillaPath = @"C:\Program Files (x86)\Steam\steamapps\common\Crusader Kings III\game";
        private static string ModdedPath = @"C:\Program Files (x86)\Steam\steamapps\common\Crusader Kings III\game";
        private static string NewVanillaPath = @"C:\Program Files (x86)\Steam\steamapps\common\Crusader Kings III\game";
        private static string LogsFolder = @"C:\Users\Harm\Documents\Paradox Interactive\Crusader Kings III\logs";

        public Program()
        {
            Console.WriteLine("Let's go! Parsing logs");
            LogsParser.ParseLogs(LogsFolder);
            //Console.WriteLine($"Parsed logs; found {GlobalResources.EFFECTKEYS.Count} effects, {GlobalResources.TRIGGERKEYS.Count} triggers, {GlobalResources.EVENTTARGETS.Count} event targets ");

            var inputDir = OldVanillaPath;

            GlobalResources.Old = new Context(OldVanillaPath, ContextType.Old);
            GlobalResources.Modded = new Context(ModdedPath, ContextType.Modded);
            GlobalResources.New = new Context(NewVanillaPath, ContextType.New);


            var fastParser = new FastParser();
            var antlrParser = new AntlrParser();

            //stopwatch.Start();
            //GatherDeclarations(fastParser, Old);

            //stopwatch.Stop();
            //Console.WriteLine($"Elapsed (old vanilla): {stopwatch.Elapsed}");

            var parsingTimer = new Stopwatch();
            parsingTimer.Start();
            //GatherDeclarationsForDeclarationType(antlrParser, GlobalResources.Old, DeclarationType.ScriptedTrigger);
            ParsingService.ParseAllEntities(() => new AntlrParser(), GlobalResources.Old);
            parsingTimer.Stop();

            //var analysisTimer = new Stopwatch();
            //analysisTimer.Start();
            //var analyser = new Analyser();
            //analyser.Analyse(GlobalResources.Old);
            //analysisTimer.Stop();
            Console.WriteLine($"Analysed a total of { GlobalResources.Old.Files.Count} files");
            //Console.WriteLine($"Found { analyser.LogEntries.Count()} issues");
            Console.WriteLine($"Parsing time: {parsingTimer.Elapsed}");
            //Console.WriteLine($"Analysis time: {analysisTimer.Elapsed}");

            //stopwatch.Restart();
            //GatherDeclarations(fastParser, Modded);
            //stopwatch.Stop();
            //Console.WriteLine($"Elapsed (old mod): {stopwatch.Elapsed}");
            //stopwatch.Restart();
            //GatherDeclarations(fastParser, New);
            //stopwatch.Stop();
            //Console.WriteLine($"Elapsed (new vanilla): {stopwatch.Elapsed}");
        }
    }
}
