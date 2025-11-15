namespace CK3Analyser.Core.Resources.DetectorSettings
{
    public readonly struct DuplicationSettings : IGenericSettings
    {
        public bool Enabled { get; init; }
        public Severity Severity { get; init; }
        public int MinSize { get; init; }
        public int MaxNumberOfDifferentValues { get; init; }

        public override string ToString() => this.GenericToString();
    }
}
