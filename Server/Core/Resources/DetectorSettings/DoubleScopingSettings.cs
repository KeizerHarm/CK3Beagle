namespace CK3BeagleServer.Core.Resources.DetectorSettings
{
    public readonly struct DoubleScopingSettings : IGenericSettings
    {
        public bool Enabled { get; init; }
        public Severity Severity { get; init; }

        public override string ToString() => this.GenericToString();
    }
}
