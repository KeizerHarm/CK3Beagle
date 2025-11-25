using CK3Analyser.Analysing.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;

namespace CK3Analyser.Analysing.Detectors
{
    public abstract class BaseDetector
    {
        protected ILogger logger;
        protected Context context;

        public BaseDetector(ILogger logger, Context context)
        {
            this.logger = logger;
            this.context = context;
        }

        public virtual void EnterScriptFile(ScriptFile scriptFile) { }
        public virtual void LeaveScriptFile(ScriptFile scriptFile) { }
        public virtual void EnterDeclaration(Declaration declaration) { }
        public virtual void LeaveDeclaration(Declaration declaration) { }
        public virtual void EnterNamedBlock(NamedBlock namedBlock) { }
        public virtual void LeaveNamedBlock(NamedBlock namedBlock) { }
        public virtual void EnterAnonymousBlock(AnonymousBlock anonymousBlock) { }
        public virtual void LeaveAnonymousBlock(AnonymousBlock anonymousBlock) { }
        public virtual void VisitBinaryExpression(BinaryExpression binaryExpression) { }
        public virtual void VisitAnonymousToken(AnonymousToken anonymousToken) { }
        public virtual void VisitComment(Comment comment) { }
        public virtual void Finish() { }

    }
}
