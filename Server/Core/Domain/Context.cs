using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Generated;
using CK3BeagleServer.Core.Resources;
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
        public Dictionary<string, Declaration>[] Declarations { get; private set; }
        public List<Declaration> OverriddenDeclarationCopies { get; private set; }
        public Dictionary<string, ScriptFile> Files { get; private set; }

        public Context(string path, ContextType type)
        {
            Path = path;
            Type = type;
            Files =  new Dictionary<string, ScriptFile>();

            Declarations = new Dictionary<string, Declaration>[Enum.GetValues<DeclarationType>().Length];
            OverriddenDeclarationCopies = new List<Declaration>();
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
                AddDeclaration(declaration);
            }
        }

        private void AddDeclaration(Declaration declaration)
        {
            if (declaration.DeclarationType == DeclarationType.ScriptedTrigger)
            {
                GlobalResources.AddTriggers([declaration.Key]);
            }
            if (declaration.DeclarationType == DeclarationType.ScriptedEffect)
            {
                GlobalResources.AddEffects([declaration.Key]);
            }
            var array = Declarations[(int)declaration.DeclarationType];
            if (array.TryGetValue(declaration.Key, out Declaration existingCopy))
            {
                if (string.Compare(declaration.File.RelativePath, existingCopy.File.RelativePath) > 0){
                    OverriddenDeclarationCopies.Add(existingCopy);
                    array[declaration.Key] = declaration;
                }
                else
                {
                    OverriddenDeclarationCopies.Add(declaration);
                }
            }
            else
            {
                Declarations[(int)declaration.DeclarationType].Add(declaration.Key, declaration);
            }
            
        }

        /// <summary>
        /// Marks the files that 'shouldn't' be parsed for this context.
        /// </summary>
        public HashSet<string> Blacklist { get; set; } = [];

        /// <summary>
        /// If set, marks the files that 'should' be parsed for this context - everything else shouldn't.
        /// </summary>
        public HashSet<string> Whitelist { get; set; } = [];

        public void ClearParsedData()
        {
            Files = new Dictionary<string, ScriptFile>();

            Declarations = new Dictionary<string, Declaration>[Enum.GetValues<DeclarationType>().Length];
            OverriddenDeclarationCopies = new List<Declaration>();
            for (int i = 0; i < Enum.GetValues<DeclarationType>().Length; i++)
            {
                Declarations[i] = new Dictionary<string, Declaration>();
            }
            Blacklist = [];
            Whitelist = [];
        }
    }
}
