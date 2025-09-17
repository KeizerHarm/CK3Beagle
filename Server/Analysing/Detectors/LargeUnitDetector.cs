using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using System;

namespace CK3Analyser.Analysis.Detectors
{
    public class LargeUnitDetector : BaseDetector
    {
        public struct Settings
        {
            public int MaxSize { get; set; }
            public Severity Severity { get; set; }
        }

        private Settings _settings;

        public LargeUnitDetector(Action<LogEntry> logFunc, Settings settings) : base(logFunc)
        {
            _settings = settings;
        }

        public override void AnalyseDeclaration(Declaration declaration)
        {
            var length = declaration.Raw.Split('\n').Length;
            if (length > _settings.MaxSize)
            {
                var entry = new LogEntry
                {
                    Location = declaration.GetIdentifier(),
                    Message = $"Large declaration detected: {length} lines",
                    Severity = _settings.Severity
                };
                LogFunc(entry);
            }
        }
    }
}
