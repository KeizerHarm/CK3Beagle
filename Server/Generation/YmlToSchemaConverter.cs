using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Generation
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
                            currentSchema.Peek().BLOCKTYPE = BlockType.Trigger;
                            break;
                        case "EFFECTBLOCK":
                            currentSchema.Peek().BLOCKTYPE = BlockType.Effect;
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
                    schema.Key = key;

                    if (currentSchema.Count != 0)
                    {
                        currentSchema.Peek().Children.Add(schema);
                    }
                    else
                    {
                        rootSchema = schema;
                    }

                    currentSchema.Push(schema);
                }
                else
                {
                    var leafSchema = new Schema
                    {
                        Key = key
                    };

                    switch (value)
                    {
                        case "TRIGGERBLOCK":
                            leafSchema.BLOCKTYPE = BlockType.Trigger;
                            break;
                        case "EFFECTBLOCK":
                            leafSchema.BLOCKTYPE = BlockType.Effect;
                            break;
                    }

                    currentSchema.Peek().Children.Add(leafSchema);
                }
            }

            return rootSchema;
        }
    }
}
