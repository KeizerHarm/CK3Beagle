namespace CK3Analyser.Core.Resources.DetectorSettings
{
    public enum IndentationType
    {
        Tab, TwoSpaces, ThreeSpaces, FourSpaces, Inconclusive
    }
    public readonly struct InconsistentIndentationSettings
    {
        public bool Enabled { get; init; }
        public IndentationType ExpectedIndentationType { get; init; }
        public Severity UnexpectedType_Severity { get; init; }
        public bool DisregardBracketsInComments { get; init; }
        public Severity Inconsistency_Severity { get; init; }
    }
}
