using System;
using System.Collections.Generic;
using System.Text;

namespace CK3Analyser.Generation
{
    public class SchemaToFileTransformer
    {
        public static string SchemaToFile(Schema schema)
        {
            var str = @"
using CK3Analyser.Core.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Core.Generated
{";
            str += GetClass(schema, 1);
            str += @"
}";
            return str;
        }

        private static string GetClass(Schema schema, int indent)
        {
            var indentation = new string('\t', indent);
            var str = @"
" + indentation + @"public class T_" + schema.Key + @" : NamedBlock
" + indentation + @"{";

            if (schema.Home != null)
            {
                str += @"
" + indentation + @"    public static readonly string Home = " + schema.Home + ";";
            }

            if (schema.BLOCKTYPE != BlockType.Other)
            {
                str += @"
" + indentation + @"    public static readonly BlockType BLOCKTYPE = " + schema.BLOCKTYPE.GetName() + ";";
            }

            str += @"
" + indentation + @"    public T_" + schema.Key + @"(string key) : base(key)
" + indentation + @"    {
" + indentation + @"    }";
            foreach (var child in schema.Children)
            {
                if (child.IsPlural)
                {
                    str += @"

" + indentation + @"    public IEnumerable<T_" + child.Key + "> " + child.Key + @" => Children.OfType<NamedBlock>().Where(x => x.Key == """ + child.Key + @""").Select(x => (T_" + child.Key + ")x);";
                }
                else
                {
                    str += @"

" + indentation + @"    public T_" + child.Key + " " + child.Key + @" => (T_" + child.Key + @")Children.OfType<NamedBlock>().FirstOrDefault(x => x.Key == """ + child.Key + @""");";
                }

                str += @"

" + GetClass(child, indent + 1);
            }

            str += @"
" + indentation + @"}";


            return str;
        }
    }
}
