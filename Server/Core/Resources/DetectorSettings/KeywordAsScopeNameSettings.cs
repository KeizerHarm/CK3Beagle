namespace CK3Analyser.Core.Resources.DetectorSettings
{
    public readonly struct KeywordAsScopeNameSettings : IGenericSettings
    {
        public bool Enabled { get; init; }
        public Severity RootPrev_Severity { get; init; }
        public Severity ScopeLink_Severity { get; init; }
        public Severity ScopeType_Severity { get; init; }

        public override string ToString() => this.GenericToString();
    }
}
