using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Generated;
using System;
using System.Collections.Generic;

namespace CK3Analyser.Core.Comparing
{
    public class ContextComparison
    {
        public Context Base { get; }
        public Context Edit { get; }

        public HashSet<string> AddedFiles { get; private set; }
        public HashSet<string> ChangedFiles { get; private set; }
        public HashSet<string> RemovedFiles { get; private set; }
        public HashSet<string> UntouchedFiles { get; private set; }

        public HashSet<string>[] AddedDeclarations { get; private set; }
        public HashSet<string>[] ChangedDeclarations { get; private set; }
        public HashSet<string>[] RemovedDeclarations { get; private set; }
        public HashSet<string>[] UntouchedDeclarations { get; private set; }

        public Dictionary<string, FileComparison> FileComparisons { get; private set; }
        public Dictionary<string, DeclarationComparison>[] DeclarationComparisons { get; private set; }

        public ContextComparison(Context baseContext, Context editContext)
        {
            Base = baseContext;
            Edit = editContext;
            HandleFiles();
            HandleDeclarations();
        }

        private void HandleFiles()
        {
            (AddedFiles, RemovedFiles, ChangedFiles, UntouchedFiles) 
                = ComparisonHelpers.SimpleListComparison(Base.Files, Edit.Files, (first, second) => first.StringRepresentation == second.StringRepresentation);

            foreach (var file in ChangedFiles)
            {
                FileComparisons.Add(file, new FileComparison(Base.Files[file], Edit.Files[file], this));
            }
        }
        private void HandleDeclarations()
        {
            foreach (var declarationType in Enum.GetValues<DeclarationType>())
            {
                (AddedDeclarations[(int)declarationType], RemovedDeclarations[(int)declarationType], ChangedDeclarations[(int)declarationType], UntouchedDeclarations[(int)declarationType])
                    = ComparisonHelpers.SimpleListComparison(Base.Declarations[(int)declarationType], Edit.Declarations[(int)declarationType],
                        (first, second) => first.GetStrictHashCode() == second.GetStrictHashCode());
            }
        }
    }
}
