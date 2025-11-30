using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Generated;
using System;
using System.Collections.Generic;


namespace CK3BeagleServer.Core.Domain
{
    public enum ContextType
    {
        Vanilla,
        UpdatedVanilla,
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

            Declarations = new Dictionary<string, Declaration>[Enum.GetValues<DeclarationType>().Length];
            for (int i = 0; i < Enum.GetValues<DeclarationType>().Length; i++)
            {
                Declarations[i] = new Dictionary<string, Declaration>();
            }
        }

        public void AddFile(ScriptFile file)
        {
            Files.Add(file.RelativePath, file);

            foreach (var declaration in file.Declarations)
            {
                Declarations[(int)declaration.Value.DeclarationType].Add(declaration.Key, declaration.Value);
            }
        }

        /// <summary>
        /// Marks the files that 'shouldn't' be parsed for this context.
        /// </summary>
        internal HashSet<string> Blacklist { get; } = [];

        /// <summary>
        /// If set, marks the files that 'should' be parsed for this context - everything else shouldn't.
        /// </summary>
        internal HashSet<string> Whitelist { get; } = [];

    }
}
