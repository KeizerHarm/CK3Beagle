using CK3BeagleServer.Core.Domain.Entities;
using System.Collections.Generic;

namespace CK3BeagleServer.Core.Comparing.PreviousAttempts.Domain
{
    public class DeclarationComparison
    {
        public Declaration Source { get; }
        public Declaration Edit { get; }
        public ScriptFile SourceFile { get; }
        public ScriptFile EditFile { get; }
        public ContextComparison Context { get; }
        public List<IEditOperation> EditScript { get; set; }
    }
}
