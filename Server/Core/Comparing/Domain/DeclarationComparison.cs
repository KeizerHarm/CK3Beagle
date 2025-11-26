using CK3Analyser.Core.Domain.Entities;
using System.Collections.Generic;

namespace CK3Analyser.Core.Comparing.Domain
{
    public class DeclarationComparison
    {
        public Declaration Base { get; }
        public Declaration Edit { get; }
        public ScriptFile BaseFile { get; }
        public ScriptFile EditFile { get; }
        public ContextComparison Context { get; }
        public List<IEditOperation> EditScript { get; set; }
    }
}
