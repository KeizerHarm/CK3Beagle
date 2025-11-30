namespace CK3BeagleServer.Core.Resources.DetectorSettings
{
    public struct NotIsNotNorSettings : IGenericSettings
    {
        public bool Enabled { get; init; }
        public Severity Severity { get; set; }

        public override string ToString() => this.GenericToString();
    }
}
