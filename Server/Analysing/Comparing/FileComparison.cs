using CK3Analyser.Core.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Analysis.Comparing
{

    public partial class ContextComparison
    {
        public class FileComparison
        {
            public ScriptFile Base { get; }
            public ScriptFile Edit { get; }
            public ContextComparison Context { get; }
            public HashSet<string> AddedDeclarations { get; private set; }
            public HashSet<string> ChangedDeclarations { get; private set; }
            public HashSet<string> RemovedDeclarations { get; private set; }
            public HashSet<string> UntouchedDeclarations { get; private set; }

            public FileComparison(ScriptFile baseFile, ScriptFile editFile, ContextComparison context)
            {
                Base = baseFile;
                Edit = editFile;
                Context = context;

                (AddedDeclarations, ChangedDeclarations, RemovedDeclarations, UntouchedDeclarations)
                    = ComparisonHelpers.SimpleListComparison(Base.Declarations.ToDictionary(), Edit.Declarations.ToDictionary());
            }
        }
    }
}
