using CK3Analyser.Analysis;
using CK3Analyser.Core;
using CK3Analyser.Core.Antlr;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Fast;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace CK3Analyser.CLI
{

    public class Program
    {
        static void Main(string[] args)
        {
            new Program();
        }


        private static string OldVanillaPath = @"C:\Program Files (x86)\Steam\steamapps\common\Crusader Kings III Beta\game";
        private static string ModdedPath = @"C:\Program Files (x86)\Steam\steamapps\common\Crusader Kings III\game";
        private static string NewVanillaPath = @"C:\Program Files (x86)\Steam\steamapps\common\Crusader Kings III\game";

        public static Context Old;
        public static Context Modded;
        public static Context New;

        public Program()
        {
            var inputDir = OldVanillaPath;

            Old = new Context(OldVanillaPath, ContextType.Old);
            Modded = new Context(ModdedPath, ContextType.Modded);
            New = new Context(NewVanillaPath, ContextType.New);

            var stopwatch = new Stopwatch();

            var fastParser = new FastParser();
            var antlrParser = new AntlrParser();

            //stopwatch.Start();
            //GatherDeclarations(fastParser, Old);

            //stopwatch.Stop();
            //Console.WriteLine($"Elapsed (old vanilla): {stopwatch.Elapsed}");

            stopwatch.Start();
            GatherDeclarations(antlrParser, Old);

            stopwatch.Stop();
            Console.WriteLine($"Parsing time: {stopwatch.Elapsed} ({Old.Files.Count} files)");

            stopwatch.Restart();
            var analyser = new Analyser();
            analyser.Analyse(Old);
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
            foreach (var entityType in Enum.GetValues<EntityType>())
            {
                var entityDeclarations = new Dictionary<string, Declaration>();

                var entityHome = Path.Combine(context.Path, entityType.GetEntityHome());
                if (Directory.Exists(entityHome))
                {
                    var files = Directory.GetFiles(entityHome, "*.txt", SearchOption.AllDirectories);
                    Console.WriteLine($"Found {files.Length} {entityType.ToString()} files");
                    foreach (var file in files)
                    {
                        var scriptFile = parser.ParseFile(file, context, entityType);
                        context.AddFile(scriptFile);
                    }
                }
            }
        }
    }
}
