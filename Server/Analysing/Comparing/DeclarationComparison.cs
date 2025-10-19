using CK3Analyser.Analysis.Comparing.Building;
using CK3Analyser.Core.Domain.Entities;
using System.Collections.Generic;

namespace CK3Analyser.Analysis.Comparing
{
    public class DeclarationComparison
    {
        public Declaration Base { get; }
        public Declaration Edit { get; }
        public ScriptFile BaseFile { get; }
        public ScriptFile EditFile { get; }
        public ContextComparison Context { get; }

        private Dictionary<int, (Node, Node)> MatchedNodes = [];
        public List<EditOperation> EditScript { get; set; }


    }
}
