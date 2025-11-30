using CK3BeagleServer.Core.Domain;
using System;
using System.IO;
using System.Linq;

namespace CK3BeagleServer.Rebasing
{
    public class Rebaser
    {
        public Context DoRebase(Context old, Context @new, Context modded)
        {
            (Context old_Relevant, Context new_Relevant) = RemoveUnchanged(old, @new);
            Context rebased = new Context(Path.Combine(modded.Path, "rebased"), ContextType.ModdedRebased);
            HandleEntityOverride(modded, old_Relevant, new_Relevant, rebased);
            HandleFileOverride(modded, old_Relevant, new_Relevant, rebased);
            
            return rebased;
        }

        private static void HandleFileOverride(Context modded, Context old_Relevant, Context new_Relevant, Context rebased)
        {
            //foreach (var file in modded.Files.Where(x => old_Relevant.Files.ContainsKey(x.Key)))
            //{
            //    var moddedFile = file.Value;
            //    var oldFile = old_Relevant.Files[file.Key];
            //    var newFile = new_Relevant.Files[file.Key];
            //    var rebasedFile = newFile.Clone();

            //    var declarationsChangedByMod = moddedFile.Declarations.Where(x => newFile.Declarations.Any(y => y.Key == x.Key));

            //    foreach (var decl in declarationsChangedByMod)
            //    {
            //        var moddedDecl = decl.Value;
            //        var oldDecl = oldFile.Declarations[decl.Key];
            //        var newDecl = newFile.Declarations[decl.Key];

            //        if (oldDecl.Raw == newDecl.Raw)
            //        {
            //            Console.WriteLine($"INFO: {moddedFile.RelativePath}:{decl.Key} - Modded declaration not changed by vanilla; using modded declaration!");
            //            rebasedFile.ReplaceDeclaration(moddedDecl);
            //        }
            //        else
            //        {
            //            Console.WriteLine($"WARNING: {moddedFile.RelativePath}:{decl.Key} - CONFLICT!");
            //        }
            //    }

            //    var declarationsAddedByMod = moddedFile.Declarations.Where(x => 
            //               !newFile.Declarations.Any(y => y.Key == x.Key)
            //            && !oldFile.Declarations.Any(y => y.Key == x.Key)
            //        );

            //    foreach (var decl in declarationsAddedByMod)
            //    {
            //        var moddedDecl = decl.Value;

            //        if (rebasedFile.Declarations.TryGetValue(moddedDecl.PrevSibling?.Key, out Declaration previousSibling)) {
            //            rebasedFile.InsertDeclaration(moddedDecl, previousSibling);
            //        }
            //        else
            //        {
            //            Console.WriteLine($"WARNING: {moddedFile.RelativePath}:{decl.Key} - TRIED TO ADD MODDED ENTITY BUT DIDN'T KNOW WHERE!");
            //        }
            //    }

            //    rebased.AddFile(rebasedFile);
            //}
        }

        private static void HandleEntityOverride(Context modded, Context old_Relevant, Context new_Relevant, Context rebased)
        {
            foreach (var file in modded.Files.Where(x => !old_Relevant.Files.ContainsKey(x.Key)))
            {
                var moddedFile = file.Value;
                var rebasedFile = moddedFile.Clone();

                foreach (var decl in moddedFile.Declarations)
                {
                    var moddedDecl = decl.Value;
                    if (old_Relevant.Declarations[(int)moddedDecl.DeclarationType].ContainsKey(decl.Key))
                    {
                        var oldDecl = old_Relevant.Declarations[(int)moddedDecl.DeclarationType][decl.Key];
                        var newDecl = new_Relevant.Declarations[(int)moddedDecl.DeclarationType][decl.Key];

                        if (oldDecl.StringRepresentation == newDecl.StringRepresentation)
                        {
                            Console.WriteLine($"INFO: {moddedFile.RelativePath}:{decl.Key} - Modded declaration not changed by vanilla; using modded declaration!");
                        }

                        if (moddedDecl.StringRepresentation == oldDecl.StringRepresentation)
                        {
                            Console.WriteLine($"INFO: {moddedFile.RelativePath}:{decl.Key} - Modded version identical to old version; replaced with new version.");
                            rebasedFile.ReplaceDeclaration(newDecl);
                        }
                        else if (moddedDecl.StringRepresentation == newDecl.StringRepresentation)
                        {
                            Console.WriteLine($"INFO: {moddedFile.RelativePath}:{decl.Key} - Modded version identical to new version; how fortunate!");
                        }
                        else
                        {
                            Console.WriteLine($"WARNING: {moddedFile.RelativePath}:{decl.Key} - CONFLICT!");
                        }

                    }
                    else
                    {
                        Console.WriteLine($"INFO: {moddedFile.RelativePath}:{decl.Key} - Modded declaration not found in vanilla changed files; no work needed!");
                    }
                }
                rebased.AddFile(rebasedFile);
            }
        }

        private (Context old_Relevant, Context new_Relevant) RemoveUnchanged(Context old, Context @new)
        {
            var changedFiles = old.Files.Where(x => 
                       @new.Files.ContainsKey(x.Key)
                    && !@new.Files[x.Key].ContentsAndLocationMatch(x.Value))
            .Select(x => x.Key).ToList();

            Context old_Relevant = new Context(old.Path, old.Type);
            foreach (var file in changedFiles) {
                old_Relevant.AddFile(old.Files[file]);
            }
            
            Context new_Relevant = new Context(@new.Path, @new.Type);
            foreach (var file in changedFiles)
            {
                new_Relevant.AddFile(@new.Files[file]);
            }

            return (old_Relevant, new_Relevant);
        }
    }
}
