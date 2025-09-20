using CK3Analyser.Analysis.Detectors;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using System.Collections.Generic;

namespace CK3Analyser.Analysis
{
    public class AnalysisVisitor : IAnalysisVisitor
    {
        public List<BaseDetector> Detectors { get; } = new List<BaseDetector>();


        public void Visit(Block block)
        {
            foreach (var detector in Detectors)
            {
                detector.AnalyseBlock(block);
            }

            foreach (var child in block.Children)
            {
                child.Accept(this);
            }
        }

        public void Visit(AnonymousBlock anonymousBlock)
        {
            foreach (var detector in Detectors)
            {
                detector.AnalyseBlock(anonymousBlock);
            }

            foreach (var child in anonymousBlock.Children)
            {
                child.Accept(this);
            }
        }

        public void Visit(Comment comment)
        {
            foreach (var detector in Detectors)
            {
                detector.AnalyseComment(comment);
            }
        }

        public void Visit(Declaration declaration)
        {
            foreach (var detector in Detectors)
            {
                detector.AnalyseDeclaration(declaration);
            }

            foreach (var child in declaration.Children)
            {
                child.Accept(this);
            }
        }

        public void Visit(BinaryExpression binaryExpression)
        {
            foreach (var detector in Detectors)
            {
                detector.AnalyseBinaryExpression(binaryExpression);
            }
        }

        public void Visit(NamedBlock namedBlock)
        {
            foreach (var detector in Detectors)
            {
                detector.AnalyseNamedBlock(namedBlock);
            }

            foreach (var child in namedBlock.Children)
            {
                child.Accept(this);
            }
        }

        public void Visit(Node node)
        {
            foreach (var detector in Detectors)
            {
                detector.AnalyseNode(node);
            }
        }

        public void Visit(ScriptFile scriptFile)
        {
            foreach (var detector in Detectors)
            {
                detector.AnalyseScriptFile(scriptFile);
            }

            foreach (var child in scriptFile.Children)
            {
                child.Accept(this);
            }
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
