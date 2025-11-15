using System.Collections.Generic;

namespace CK3Analyser.Core.Resources.DetectorSettings
{
    public readonly struct MagicNumberSettings : IGenericSettings
    {
        public bool Enabled { get; init; }
        public Severity Severity { get; init; }
        public HashSet<string> StatementKeysToConsider { get; init; }

        public override string ToString() => this.GenericToString();
    }
}
