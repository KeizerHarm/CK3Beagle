using System.Collections.Generic;

namespace CK3BeagleServer.Core.Resources.DetectorSettings
{
    public readonly struct HiddenDependenciesSettings : IGenericSettings
    {
        public bool Enabled { get; init; }

        public Severity UseOfRoot_Severity { get; init; }
        public bool UseOfRoot_AllowedIfInName { get; init; }
        public bool UseOfRoot_AllowedIfInComment { get; init; }
        public bool UseOfRoot_AllowedIfInEventFile { get; init; }

        public Severity UseOfPrev_Severity { get; init; }
        public bool UseOfPrev_AllowedIfInName { get; init; }
        public bool UseOfPrev_AllowedIfInComment { get; init; }
        public bool UseOfPrev_AllowedIfInEventFile { get; init; }

        public Severity UseOfSavedScope_Severity { get; init; }
        public bool UseOfSavedScope_AllowedIfInName { get; init; }
        public bool UseOfSavedScope_AllowedIfInComment { get; init; }
        public bool UseOfSavedScope_AllowedIfInEventFile { get; init; }
        public HashSet<string> UseOfSavedScope_Whitelist { get; init; }

        public Severity UseOfVariable_Severity { get; init; }
        public bool UseOfVariable_AllowedIfInName { get; init; }
        public bool UseOfVariable_AllowedIfInComment { get; init; }
        public bool UseOfVariable_AllowedIfInEventFile { get; init; }
        public HashSet<string> UseOfVariable_Whitelist { get; init; }

        public override string ToString() => this.GenericToString();
    }
}
