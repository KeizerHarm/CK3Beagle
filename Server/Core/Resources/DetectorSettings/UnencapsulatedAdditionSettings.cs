namespace CK3Analyser.Core.Resources.DetectorSettings
{
    public struct UnencapsulatedAdditionSettings : IGenericSettings
    {
        public bool Enabled { get; init; }
        public Severity Severity { get; set; }
        public int Threshold { get; set; }

        public override string ToString() => this.GenericToString();
    }
}
