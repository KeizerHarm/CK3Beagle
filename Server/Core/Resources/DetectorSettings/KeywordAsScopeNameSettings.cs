namespace CK3BeagleServer.Core.Resources.DetectorSettings
{
    public readonly struct KeywordAsScopeNameSettings : IGenericSettings
    {
        public bool Enabled { get; init; }
        public Severity RootPrevThis_Severity { get; init; }
        public Severity ScopeLink_Severity { get; init; }

        public override string ToString() => this.GenericToString();
    }
}
