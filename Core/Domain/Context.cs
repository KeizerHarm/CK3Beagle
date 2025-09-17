using System;
using System.Collections.Generic;

namespace CK3Analyser.Core.Domain
{
    public enum ContextType
    {
        Old,
        New,
        Modded,
        ModdedRebased
    }

    public class Context
    {
        public string Path { get; set; }
        public ContextType Type { get; set; }
        public Dictionary<string, Declaration>[] Declarations { get; }
        public Dictionary<string, ScriptFile> Files { get; }

        public Context(string path, ContextType type)
        {
            Path = path;
            Type = type;
            Files =  new Dictionary<string, ScriptFile>();

            Declarations = new Dictionary<string, Declaration>[Enum.GetValues<EntityType>().Length];
            for (int i = 0; i < Enum.GetValues<EntityType>().Length; i++)
            {
                Declarations[i] = new Dictionary<string, Declaration>();
            }
        }

        public void AddFile(ScriptFile file)
        {
            Files.Add(file.RelativePath, file);

            foreach (var declaration in file.Declarations)
            {
                Declarations[(int)declaration.Value.EntityType].Add(declaration.Key, declaration.Value);
            }
        }
    }
}
