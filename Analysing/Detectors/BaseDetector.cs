using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using System;

namespace CK3Analyser.Analysis.Detectors
{
    public abstract class BaseDetector
    {
        protected Action<LogEntry> LogFunc;

        public BaseDetector(Action<LogEntry> logFunc)
        {
            LogFunc = logFunc;
        }

        public virtual void AnalyseBlock(Block block) { }
        public virtual void AnalyseComment(Comment comment) { }
        public virtual void AnalyseDeclaration(Declaration declaration) { }
        public virtual void AnalyseKeyValuePair(KeyValuePair keyValuePair) { }
        public virtual void AnalyseNamedBlock(NamedBlock namedBlock) { }
        public virtual void AnalyseNode(Node node) { }
        public virtual void AnalyseScriptFile(ScriptFile scriptFile) { }
    }
}
