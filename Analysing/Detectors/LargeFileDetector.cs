using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using System;

namespace CK3Analyser.Analysis.Detectors
{
    public class LargeFileDetector : BaseDetector
    {
        public LargeFileDetector(Action<LogEntry> logFunc) : base(logFunc)
        {
        }

        public override void AnalyseScriptFile(ScriptFile scriptFile) {
            var length = scriptFile.Raw.Split('\n').Length;
            if (length > 5000)
            {
                var entry = new LogEntry
                {
                    Location = scriptFile.GetIdentifier(),
                    Message = $"Large file detected: {length} lines",
                    Severity = Severity.Info
                };
                LogFunc(entry);
            }
        }
    }
}
