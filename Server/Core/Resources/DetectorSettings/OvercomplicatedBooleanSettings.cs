namespace CK3Analyser.Core.Resources.DetectorSettings
{
    public struct OvercomplicatedBooleanSettings : IGenericSettings
    {
        public bool Enabled { get; init; }
        public Severity DoubleNegation_Severity { get; set; }
        public Severity Associativity_Severity { get; set; }
        public Severity Distributivity_Severity { get; set; }
        public Severity Idempotency_Severity { get; set; }
        public Severity Complementation_Severity { get; set; }
        public Severity NotIsNotNor_Severity { get; set; }
        public Severity Absorption_Severity { get; set; }

        public override string ToString() => this.GenericToString();
    }
}
