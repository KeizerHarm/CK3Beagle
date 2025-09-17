using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using System;

namespace CK3Analyser.Analysis.Detectors
{
    public class LargeUnitDetector : BaseDetector
    {
        public LargeUnitDetector(Action<LogEntry> logFunc) : base(logFunc)
        {
        }

        public override void AnalyseDeclaration(Declaration declaration)
        {
            var length = declaration.Raw.Split('\n').Length;
            if (length > 50)
            {
                var entry = new LogEntry
                {
                    Location = declaration.GetIdentifier(),
                    Message = $"Large declaration detected: {length} lines",
                    Severity = Severity.Info
                };
                LogFunc(entry);
            }
        }
    }
}
