using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;

namespace CK3BeagleServer.Generation
{
    public class YmlToSchemaConverter
    {
        public static Schema YmlToSchema(TextLineCollection lines)
        {
            Schema rootSchema = null;
            var currentSchema = new Stack<Schema>();
            var currentDepth = 0;

            foreach (var line in lines)
            {
                var txt = line.ToString();
                var key = txt.Split(':')[0].Trim();
                var value = txt.Split(':')[1].Trim();
                var leadingSpaces = txt.TakeWhile(x => x == ' ').Count();
                var changeInDepth = leadingSpaces - currentDepth;
                if (changeInDepth < 0)
                {
                    while (currentDepth != leadingSpaces)
                    {
                        currentDepth--;
                        currentSchema.Pop();
                    }
                }
                currentDepth = leadingSpaces;

                if (key == "HOME")
                {
                    currentSchema.Peek().Home = txt.Split(':')[1].Trim();
                }
                else if (key == "CONTENTS")
                {
                    switch (value)
                    {
                        case "TRIGGERBLOCK":
                            currentSchema.Peek().BLOCKTYPE = BlockContext.Trigger;
                            break;
                        case "EFFECTBLOCK":
                            currentSchema.Peek().BLOCKTYPE = BlockContext.Effect;
                            break;
                    }
                }
                else if (value == "")
                {
                    var schema = new Schema();

                    if (key.EndsWith("*"))
                    {
                        key = key.Replace("*", "");
                        schema.IsPlural = true;
                    }
                    schema.ScriptKey = key;

                    if (currentSchema.Count != 0)
                    {
                        currentSchema.Peek().AddChild(schema);
                    }
                    else
                    {
                        rootSchema = schema;
                        rootSchema.FullCodeName = rootSchema.LocalCodeName;
                        rootSchema.SchemasInTree.Add(schema);
                    }

                    currentSchema.Push(schema);
                }
                else
                {
                    var leafSchema = new Schema()
                    {
                        ScriptKey = key
                    };

                    switch (value)
                    {
                        case "TRIGGERBLOCK":
                            leafSchema.BLOCKTYPE = BlockContext.Trigger;
                            break;
                        case "EFFECTBLOCK":
                            leafSchema.BLOCKTYPE = BlockContext.Effect;
                            break;
                    }

                    currentSchema.Peek().AddChild(leafSchema);
                }
            }

            return rootSchema;
        }
    }
}
