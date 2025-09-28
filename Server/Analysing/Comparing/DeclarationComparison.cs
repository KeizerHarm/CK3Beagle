using CK3Analyser.Core.Domain.Entities;

namespace CK3Analyser.Analysis.Comparing
{

    public partial class ContextComparison
    {
        public class DeclarationComparison
        {
            public Declaration Base { get; }
            public Declaration Edit { get; }
            public ScriptFile BaseFile { get; }
            public ScriptFile EditFile { get; }
            public ContextComparison Context { get; }
        }
    }
}
