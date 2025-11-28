using CK3Analyser.Core.Comparing.PreviousAttempts.Building;
using CK3Analyser.Core.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Core.Comparing.PreviousAttempts.Domain
{
    public class FileComparison
    {
        public ScriptFile Source { get; }
        public ScriptFile Edit { get; }
        public HashSet<string> AddedDeclarations { get; private set; }
        public HashSet<string> ChangedDeclarations { get; private set; }
        public HashSet<string> RemovedDeclarations { get; private set; }
        public HashSet<string> UntouchedDeclarations { get; private set; }
        public List<IEditOperation> EditScript { get; private set; }

        private BlockComparisonBuilder _comparisonBuilder;

        public FileComparison(ScriptFile sourceFile, ScriptFile editFile)
        {
            Source = sourceFile;
            Edit = editFile;

            (AddedDeclarations, RemovedDeclarations, ChangedDeclarations, UntouchedDeclarations)
                = ComparisonHelpers.SimpleListComparison(Source.Declarations.ToDictionary(), Edit.Declarations.ToDictionary(),
                    (first, second) => first.GetTrueHash() == second.GetTrueHash());

            _comparisonBuilder = new BlockComparisonBuilder();
            foreach (var changedDecl in ChangedDeclarations) {
                _comparisonBuilder.BuildComparison(Source.Declarations[changedDecl], Edit.Declarations[changedDecl]);
            }
            EditScript = _comparisonBuilder.EditScript;
            _comparisonBuilder = null;
        }
    }
}
