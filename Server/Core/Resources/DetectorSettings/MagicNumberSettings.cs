using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK3Analyser.Core.Resources.DetectorSettings
{
    public readonly struct MagicNumberSettings
    {
        public bool Enabled { get; init; }
        public Severity Severity { get; init; }
        public HashSet<string> StatementKeysToConsider { get; init; }
    }
}
