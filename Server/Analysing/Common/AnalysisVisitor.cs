using CK3BeagleServer.Analysing.Common.Detectors;
using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using System.Collections.Generic;

namespace CK3BeagleServer.Analysing.Common
{
    public sealed class AnalysisVisitor : BaseDomainVisitor
    {
        public List<BaseDetector> Detectors { get; } = new List<BaseDetector>();

        public override void Visit(ScriptFile scriptFile)
        {
            foreach (var detector in Detectors)
            {
                detector.EnterScriptFile(scriptFile);
            }
            base.Visit(scriptFile);
            foreach (var detector in Detectors)
            {
                detector.LeaveScriptFile(scriptFile);
            }
        }

        public override void Visit(Declaration declaration)
        {
            foreach (var detector in Detectors)
            {
                detector.EnterDeclaration(declaration);
            }
            base.Visit(declaration);
            foreach (var detector in Detectors)
            {
                detector.LeaveDeclaration(declaration);
            }
        }

        public override void Visit(AnonymousBlock anonymousBlock)
        {
            foreach (var detector in Detectors)
            {
                detector.EnterAnonymousBlock(anonymousBlock);
            }
            base.Visit(anonymousBlock);
            foreach (var detector in Detectors)
            {
                detector.LeaveAnonymousBlock(anonymousBlock);
            }
        }
        public override void Visit(NamedBlock namedBlock)
        {
            foreach (var detector in Detectors)
            {
                detector.EnterNamedBlock(namedBlock);
            }
            base.Visit(namedBlock);
            foreach (var detector in Detectors)
            {
                detector.LeaveNamedBlock(namedBlock);
            }
        }


        public override void Visit(BinaryExpression binaryExpression)
        {
            foreach (var detector in Detectors)
            {
                detector.VisitBinaryExpression(binaryExpression);
            }
            base.Visit(binaryExpression);
        }

        public override void Visit(AnonymousToken anonymousToken)
        {
            foreach (var detector in Detectors)
            {
                detector.VisitAnonymousToken(anonymousToken);
            }
            base.Visit(anonymousToken);
        }
        public override void Visit(Comment comment)
        {
            foreach (var detector in Detectors)
            {
                detector.VisitComment(comment);
            }
            base.Visit(comment);
        }


        public void Finish()
        {
            foreach (var detector in Detectors)
            {
                detector.Finish();
            }
        }
    }
}
