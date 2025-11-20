using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Generated;
using System;

namespace CK3Analyser.Core.Parsing.SemanticPass
{
    public class SemanticPassHandler
    {
        public void ExecuteSemanticPass(Context context)
        {
            foreach (var declarationType in Enum.GetValues<DeclarationType>())
            {
                foreach (var declaration in context.Declarations[(int)declarationType])
                {
                    var _ = declarationType.ConstructDeclarationSymbol(declaration.Value);
                }
            }

            var visitor = new SecondPassVisitor();
            foreach (var file in context.Files)
            {
                file.Value.Accept(visitor);
            }
        }
    }
}
