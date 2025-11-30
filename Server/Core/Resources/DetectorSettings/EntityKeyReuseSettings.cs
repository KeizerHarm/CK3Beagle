namespace CK3BeagleServer.Core.Resources.DetectorSettings
{
    public struct EntityKeyReuseSettings : IGenericSettings
    {
        public bool Enabled { get; init; }
        public Severity SameType_Severity { get; init; }
        public Severity DifferentType_Severity { get; init; }

        public override string ToString() => this.GenericToString();
    }
}
