using CK3Analyser.Analysis.Detectors;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using System.Collections.Generic;

namespace CK3Analyser.Analysis
{
    public class AnalysisVisitor : BaseDomainVisitor
    {
        public List<BaseDetector> Detectors { get; } = new List<BaseDetector>();

        public override void Visit(Block block)
        {
            foreach (var detector in Detectors)
            {
                detector.AnalyseBlock(block);
            }
            base.Visit(block);
        }

        public override void Visit(AnonymousBlock anonymousBlock)
        {
            foreach (var detector in Detectors)
            {
                detector.AnalyseBlock(anonymousBlock);
            }
            base.Visit(anonymousBlock);
        }

        public override void Visit(Comment comment)
        {
            foreach (var detector in Detectors)
            {
                detector.AnalyseComment(comment);
            }
            base.Visit(comment);
        }

        public override void Visit(Declaration declaration)
        {
            foreach (var detector in Detectors)
            {
                detector.AnalyseDeclaration(declaration);
            }
            base.Visit(declaration);
        }

        public override void Visit(BinaryExpression binaryExpression)
        {
            foreach (var detector in Detectors)
            {
                detector.AnalyseBinaryExpression(binaryExpression);
            }
            base.Visit(binaryExpression);
        }

        public override void Visit(NamedBlock namedBlock)
        {
            foreach (var detector in Detectors)
            {
                detector.AnalyseNamedBlock(namedBlock);
            }
            base.Visit(namedBlock);
        }

        public override void Visit(Node node)
        {
            foreach (var detector in Detectors)
            {
                detector.AnalyseNode(node);
            }
            base.Visit(node);
        }

        public override void Visit(ScriptFile scriptFile)
        {
            foreach (var detector in Detectors)
            {
                detector.AnalyseScriptFile(scriptFile);
            }
            base.Visit(scriptFile);
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
