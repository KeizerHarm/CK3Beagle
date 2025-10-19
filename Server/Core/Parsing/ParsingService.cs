using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Parsing.SecondPass;
using CK3Analyser.Core.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CK3Analyser.Core.Parsing
{
    public class ParsingService
    {
        public static void ParseAllEntities(ICk3Parser parser, Context context)
        {
            Console.WriteLine($"Now reading files from {context.Path}");
            foreach (var declarationType in Enum.GetValues<DeclarationType>())
            {
                GatherDeclarationsForDeclarationType(parser, context, declarationType);
            }
            Console.WriteLine("Done with first pass");
            new SecondPassHandler().ExecuteSecondPass(context);
            Console.WriteLine("Done with second pass");
        }

        public static void GatherDeclarationsForDeclarationType(ICk3Parser parser, Context context, DeclarationType declarationType)
        {
            var entityDeclarations = new Dictionary<string, Declaration>();

            var entityHome = Path.Combine(context.Path, declarationType.GetEntityHome());
            if (Directory.Exists(entityHome))
            {
                var files = Directory.GetFiles(entityHome, "*.txt", SearchOption.AllDirectories);
                Console.WriteLine($"Found {files.Length} {declarationType.ToString()} files");

                //foreach(var file in files)
                //{
                //    var scriptFile = parser.ParseFile(file, context, declarationType);
                //    if (declarationType == DeclarationType.ScriptedEffect)
                //    {
                //        GlobalResources.AddEffects(scriptFile.Declarations.Keys);
                //    }
                //    if (declarationType == DeclarationType.ScriptedTrigger)
                //    {
                //        GlobalResources.AddTriggers(scriptFile.Declarations.Keys);
                //    }
                //    context.AddFile(scriptFile);
                //};
                Parallel.ForEach(files, filePath =>
                {
                    var relativePath = Path.GetRelativePath(context.Path, filePath);
                    var input = File.ReadAllText(filePath);
                    var scriptfile = new ScriptFile(context, relativePath, declarationType, input);

                    parser.ParseFile(scriptfile);
                    if (declarationType == DeclarationType.ScriptedEffect)
                    {
                        GlobalResources.AddEffects(scriptfile.Declarations.Keys);
                    }
                    if (declarationType == DeclarationType.ScriptedTrigger)
                    {
                        GlobalResources.AddTriggers(scriptfile.Declarations.Keys);
                    }
                    context.AddFile(scriptfile);
                });
            }
            GlobalResources.Lock();
        }
    }
}
