using System.Collections.Immutable;
using System.Linq;

namespace CK3Analyser.Generation
{
    public static class GeneralFilesWriter
    {
        public static string WriteGeneralDeclarationsFile(ImmutableArray<Schema> schemas)
        {
            var declarationSchemas = schemas.SelectMany(x => x.SchemasInTree).Where(x => !string.IsNullOrEmpty(x.Home))
                .OrderBy(x => x.ScriptKey);

            var str = @"
using CK3Analyser.Core.Domain.Entities;

namespace CK3Analyser.Core.Generated
{
    public enum DeclarationType
    {
        Debug";
            foreach (var schema in declarationSchemas)
            {
                str += @",
        " + schema.FullCodeName;
            }
            str += @"
    }

    public static class GeneratedDeclarationTypeExtensions
    {
        public static string GetEntityHome(this DeclarationType declarationType)
        {
            return declarationType switch
            {
                DeclarationType.Debug => ""test""";

            foreach (var schema in declarationSchemas)
            {
                str += @",
                DeclarationType." + schema.FullCodeName + @" => " + schema.Home;
            }

            str += @",
                _ => """",
            };
        }

        public static int ConstructDeclarationSymbol(this DeclarationType type, Declaration node)
        {
            return type switch
            {";
            foreach(var schema in declarationSchemas)
            {
                str += @"
                DeclarationType." + schema.FullCodeName + @" => " + schema.FullCodeName + @".Construct(node),";
            }

            str += @"
                _ => 0
            };
        }
    }
}";
            return str;
        }

        public static string WriteGeneralSymbolsFile(ImmutableArray<Schema> schemas)
        {
            var allSchemas = schemas.SelectMany(x => x.SchemasInTree);

            var str = @"
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Domain.Symbols;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Resources.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Core.Generated
{
    public enum SymbolType
    {
        Undefined";

            foreach (var schema in allSchemas)
            {
                str += @",
        " + schema.FullCodeName;
            }
            str += @"
    }

    public static class GeneratedSymbolExtensions
    {
        public static BlockContext GetContextType(this SymbolType symbolType)
        {
            return symbolType switch
            {";
            foreach (var schema in allSchemas)
            {
                str += @"
                SymbolType." + schema.FullCodeName + " => BlockContext." + schema.BLOCKTYPE.ToString() + ",";
            }
            str += @"
                _ => BlockContext.None
            };
        }

        public static int? Construct<TSymbol>(NamedBlock node, string key, Func<NamedBlock, int> constructor)
        {
            return node.Children.OfType<NamedBlock>().Where(x => x.Key == key)
                           .Select(x => constructor(x))
                           .FirstOrDefault(-1);
        }

        public static List<int> ConstructMany<TSymbol>(NamedBlock node, string key, Func<NamedBlock, int> constructor)
            where TSymbol : struct, ISymbol
        {
            return node.Children.OfType<NamedBlock>().Where(x => x.Key == key)
                           .Select(x => constructor(x))
                           .ToList();
        }

        public static void RegisterAllTypes(SymbolTable symbolTable)
        {";
            foreach (var schema in allSchemas)
            {
                str += @"
            symbolTable.Register<" + schema.FullCodeName + @">(SymbolType." + schema.FullCodeName + @");";
            }

            str += @"
        }
    }
}";
            return str;
        }
    }
}
