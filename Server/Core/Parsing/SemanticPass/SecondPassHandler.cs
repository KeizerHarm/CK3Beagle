using CK3Analyser.Core.Domain;

namespace CK3Analyser.Core.Parsing.SemanticPass
{
    public class SecondPassHandler
    {
        public void ExecuteSecondPass(Context context)
        {
            var visitor = new SecondPassVisitor();
            foreach (var file in context.Files)
            {
                file.Value.Accept(visitor);
            }
        }
    }
}
