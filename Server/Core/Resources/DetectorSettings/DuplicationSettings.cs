namespace CK3Analyser.Core.Resources.DetectorSettings
{
    public readonly struct DuplicationSettings
    {
        public bool Enabled { get; init; }
        public Severity Severity { get; init; }
        public int MinSize { get; init; }
        public int MaxNumberOfDifferentValues { get; init; }
    }
}
