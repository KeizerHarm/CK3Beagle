using System.Collections.Generic;

namespace CK3Analyser.Core.Resources.DetectorSettings
{
    public readonly struct HiddenDependenciesSettings
    {
        public bool Enabled { get; init; }
        public Severity UseOfRoot_Severity { get; init; }
        public bool UseOfRoot_IgnoreIfInName { get; init; }
        public bool UseOfRoot_IgnoreIfInComment { get; init; }
        public bool UseOfRoot_AllowInEventFile { get; init; }

        public Severity UseOfPrev_Severity { get; init; }
        public bool UseOfPrev_IgnoreIfInName { get; init; }
        public bool UseOfPrev_IgnoreIfInComment { get; init; }
        public bool UseOfPrev_AllowInEventFile { get; init; }

        public Severity UseOfSavedScope_Severity { get; init; }
        public bool UseOfSavedScope_IgnoreIfInName { get; init; }
        public bool UseOfSavedScope_IgnoreIfInComment { get; init; }
        public bool UseOfSavedScope_AllowInEventFile { get; init; }

        public Severity UseOfVariable_Severity { get; init; }
        public bool UseOfVariable_IgnoreIfInName { get; init; }
        public bool UseOfVariable_IgnoreIfInComment { get; init; }
        public bool UseOfVariable_AllowInEventFile { get; init; }

        public HashSet<string> VariablesWhitelist { get; init; }
    }
}
