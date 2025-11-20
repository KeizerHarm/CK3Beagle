using System;
using System.Collections.Immutable;
using System.Linq;

namespace CK3Analyser.Generation
{
    public class SchemaToFileTransformer
    {
        public static string SchemaToFile(Schema schema)
        {
            var str = @"
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources;
using CK3Analyser.Core.Resources.Semantics;
using CK3Analyser.Core.Domain.Symbols;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Core.Generated
{";
            str += GetClass(schema);
            str += @"
}";
            return str;
        }


        private static string GetClass(Schema schema)
        {
            var str = @"
    public struct " + schema.FullCodeName + @" : ISymbol
    {
        public static int Construct(NamedBlock node)
        {
            var symbol = new " + schema.FullCodeName + @"
            {";

            foreach (var child in schema.Children)
            {
                if (child.IsPlural)
                {
                    str += @"
                " + child.LocalCodeName + @"s = GeneratedSymbolExtensions.ConstructMany<" + child.FullCodeName + @">(node, """ + child.ScriptKey + @""", " + child.FullCodeName + @".Construct),";
                }
                else
                {
                    str += @"
                " + child.LocalCodeName + @" = GeneratedSymbolExtensions.Construct<" + child.FullCodeName + @">(node, """ + child.ScriptKey + @""", " + child.FullCodeName + @".Construct),";
                }
            }
            str += @"
            };
            node.SemanticId = GlobalResources.SymbolTable.Insert(SymbolType." + schema.FullCodeName + @", symbol);
            node.SymbolType = SymbolType." + schema.FullCodeName + @";
            return node.SemanticId;
        }";

            foreach (var child in schema.Children)
            {
                if (child.IsPlural)
                {
                    str += @"

        public List<int> " + child.LocalCodeName + @"s { get; set; }
        public readonly IEnumerable<" + child.FullCodeName + @"> " + child.FullCodeName + @"Symbols(SymbolTable symbolTable) =>
            " + child.LocalCodeName + @"s.Select(x => (" + child.FullCodeName + @")symbolTable.Lookup(SymbolType." + child.FullCodeName + @", x));";
                } 
                else
                {
                    str += @"

        public int? " + child.LocalCodeName + @" { get; set; }
        public readonly " + child.FullCodeName + @" " + child.FullCodeName + @"Symbol(SymbolTable symbolTable) =>
            (" + child.FullCodeName + @")symbolTable.Lookup(SymbolType." + child.FullCodeName + @", " + child.LocalCodeName + @");";
                }
            }

            str += @"
    }";

            foreach (var child in schema.Children)
            {
                str += @"
";
                str += GetClass(child);
            }

            return str;
        }
    }
}
