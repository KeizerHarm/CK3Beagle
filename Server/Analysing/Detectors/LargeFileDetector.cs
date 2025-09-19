using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using System;

namespace CK3Analyser.Analysis.Detectors
{
    public class LargeFileDetector : BaseDetector
    {
        public struct Settings
        {
            public int MaxSize { get; set; }
            public Severity Severity { get; set; }
        }

        private Settings _settings;

        public LargeFileDetector(ILogger logger, Settings settings) : base(logger)
        {
            _settings = settings;
        }

        public override void AnalyseScriptFile(ScriptFile scriptFile) {
            var length = scriptFile.Raw.Split('\n').Length;
            if (length > _settings.MaxSize)
            {
                logger.Log(
                    Smell.LargeFile, 
                    _settings.Severity,
                    $"Large file detected: {length} lines",
                    scriptFile.GetIdentifier());
            }
        }
    }
}
