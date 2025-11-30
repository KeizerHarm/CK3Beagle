using System.Collections.Generic;

namespace CK3BeagleServer.Core.Resources.DetectorSettings
{
    public readonly struct MagicNumberSettings : IGenericSettings
    {
        public bool Enabled { get; init; }
        public Severity Severity { get; init; }
        public HashSet<string> KeysToConsider { get; init; }

        public override string ToString() => this.GenericToString();
    }
}
