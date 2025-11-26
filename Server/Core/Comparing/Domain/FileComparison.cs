using CK3Analyser.Core.Comparing.Building;
using CK3Analyser.Core.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Core.Comparing.Domain
{
    public class FileComparison
    {
        public ScriptFile Base { get; }
        public ScriptFile Edit { get; }
        public HashSet<string> AddedDeclarations { get; private set; }
        public HashSet<string> ChangedDeclarations { get; private set; }
        public HashSet<string> RemovedDeclarations { get; private set; }
        public HashSet<string> UntouchedDeclarations { get; private set; }
        public List<IEditOperation> EditScript { get; private set; }

        private BlockComparisonBuilder _comparisonBuilder;

        public FileComparison(ScriptFile baseFile, ScriptFile editFile)
        {
            Base = baseFile;
            Edit = editFile;

            (AddedDeclarations, ChangedDeclarations, RemovedDeclarations, UntouchedDeclarations)
                = ComparisonHelpers.SimpleListComparison(Base.Declarations.ToDictionary(), Edit.Declarations.ToDictionary(),
                    (first, second) => first.GetStrictHashCode() == second.GetStrictHashCode());

            _comparisonBuilder = new BlockComparisonBuilder();
            _comparisonBuilder.BuildComparison(Base, Edit);
            EditScript = _comparisonBuilder.EditScript;
            _comparisonBuilder = null;
        }
    }
}
