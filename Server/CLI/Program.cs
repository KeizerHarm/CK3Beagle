using CK3Analyser.Analysis;
using CK3Analyser.Core;
using CK3Analyser.Core.Parsing.Antlr;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Parsing.Fast;
using CK3Analyser.Core.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CK3Analyser.Core.Parsing.SecondPass;

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
            LogsParser.ParseLogs(LogsFolder);

            var inputDir = OldVanillaPath;

            GlobalResources.Old = new Context(OldVanillaPath, ContextType.Old);
            GlobalResources.Modded = new Context(ModdedPath, ContextType.Modded);
            GlobalResources.New = new Context(NewVanillaPath, ContextType.New);

            var stopwatch = new Stopwatch();

            var fastParser = new FastParser();
            var antlrParser = new AntlrParser();

            //stopwatch.Start();
            //GatherDeclarations(fastParser, Old);

            //stopwatch.Stop();
            //Console.WriteLine($"Elapsed (old vanilla): {stopwatch.Elapsed}");

            stopwatch.Start();
            //GatherDeclarationsForDeclarationType(antlrParser, GlobalResources.Old, DeclarationType.ScriptedTrigger);
            GatherDeclarations(antlrParser, GlobalResources.Old);

            stopwatch.Stop();
            Console.WriteLine($"Parsing time: {stopwatch.Elapsed} ({GlobalResources.Old.Files.Count} files)");

            stopwatch.Restart();
            var analyser = new Analyser();
            analyser.Analyse(GlobalResources.Old);
            stopwatch.Stop();
            Console.WriteLine($"Analysis time: {stopwatch.Elapsed}");

            //stopwatch.Restart();
            //GatherDeclarations(fastParser, Modded);
            //stopwatch.Stop();
            //Console.WriteLine($"Elapsed (old mod): {stopwatch.Elapsed}");
            //stopwatch.Restart();
            //GatherDeclarations(fastParser, New);
            //stopwatch.Stop();
            //Console.WriteLine($"Elapsed (new vanilla): {stopwatch.Elapsed}");

        }

        public static void GatherDeclarations(ICk3Parser parser, Context context)
        {
            Console.WriteLine($"Now reading {context.Type.ToString()}");
            foreach (var declarationType in Enum.GetValues<DeclarationType>())
            {
                GatherDeclarationsForDeclarationType(parser, context, declarationType);
            }
            new SecondPassHandler().ExecuteSecondPass(context);
        }

        public static void GatherDeclarationsForDeclarationType(ICk3Parser parser, Context context, DeclarationType declarationType)
        {
            var entityDeclarations = new Dictionary<string, Declaration>();

            var entityHome = Path.Combine(context.Path, declarationType.GetEntityHome());
            if (Directory.Exists(entityHome))
            {
                var files = Directory.GetFiles(entityHome, "*.txt", SearchOption.AllDirectories);
                Console.WriteLine($"Found {files.Length} {declarationType.ToString()} files");
                foreach (var file in files)
                {
                    var scriptFile = parser.ParseFile(file, context, declarationType);
                    if (declarationType == DeclarationType.ScriptedEffect)
                    {
                        GlobalResources.AddEffects(scriptFile.Declarations.Keys);
                    }
                    if (declarationType == DeclarationType.ScriptedTrigger)
                    {
                        GlobalResources.AddTriggers(scriptFile.Declarations.Keys);
                    }
                    context.AddFile(scriptFile);
                }
            }
        }
    }
}
