using Microsoft.CodeAnalysis;
using System.Linq;

namespace CK3Analyser.Generation
{
    [Generator]
    public class Generator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //Debugger.Launch();

            var schemas =
                context.AdditionalTextsProvider.Where(file => file.Path.EndsWith(".yml"))
                        .Select((x, _) => YmlToSchemaConverter.YmlToSchema(x.GetText().Lines));

            context.RegisterSourceOutput(schemas, (spc, schema) =>
            {
                var file = SchemaToFileTransformer.SchemaToFile(schema);
                spc.AddSource(schema.Key + ".cs", file);
            });
        }

    }
}
