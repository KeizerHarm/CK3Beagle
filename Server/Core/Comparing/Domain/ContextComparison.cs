using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Generated;
using System;
using System.Collections.Generic;

namespace CK3Analyser.Core.Comparing.Domain
{
    public class ContextComparison
    {
        public Context Base { get; }
        public Context Edit { get; }

        public HashSet<string> AddedFiles { get; private set; }
        public HashSet<string> ChangedFiles { get; private set; }
        public HashSet<string> RemovedFiles { get; private set; }
        public HashSet<string> UntouchedFiles { get; private set; }

        public Dictionary<string, FileComparison> FileComparisons { get; private set; }
        public Dictionary<string, DeclarationComparison>[] DeclarationComparisons { get; private set; }

        public ContextComparison(Context baseContext, Context editContext)
        {
            Base = baseContext;
            Edit = editContext;
            HandleFiles();
        }

        private void HandleFiles()
        {
            (AddedFiles, RemovedFiles, ChangedFiles, UntouchedFiles) 
                = ComparisonHelpers.SimpleListComparison(Base.Files, Edit.Files, (first, second) => first.StringRepresentation == second.StringRepresentation);
        }
    }
}
