using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain.Entities;

namespace CK3Analyser.Analysis.Detectors
{
    public abstract class BaseDetector
    {
        protected ILogger logger;

        public BaseDetector(ILogger logger)
        {
            this.logger = logger;
        }

        public virtual void AnalyseBlock(Block block) { }
        public virtual void AnalyseComment(Comment comment) { }
        public virtual void AnalyseDeclaration(Declaration declaration) { }
        public virtual void AnalyseBinaryExpression(BinaryExpression binaryExpression) { }
        public virtual void AnalyseNamedBlock(NamedBlock namedBlock) { }
        public virtual void AnalyseNode(Node node) { }
        public virtual void AnalyseScriptFile(ScriptFile scriptFile) { }
    }
}
