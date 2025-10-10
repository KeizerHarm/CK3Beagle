using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using System.Linq;

namespace CK3Analyser.Analysis.Detectors
{
    public class LargeUnitDetector : BaseDetector
    {
        public readonly struct Settings
        {
            public int MaxSize_NonMacroBlock { get; init; }
            public Severity Severity_NonMacroBlock { get; init; }

            public int MaxSize_File { get; init; }
            public Severity Severity_File { get; init; }

            public int MaxSize_Macro { get; init; }
            public Severity Severity_Macro { get; init; }
        }

        private readonly Settings _settings;

        public LargeUnitDetector(ILogger logger, Context context, Settings settings) : base(logger, context)
        {
            _settings = settings;
        }

        public override void EnterScriptFile(ScriptFile scriptFile)
        {
            int noOfLines = 1;
            foreach (char c in scriptFile.Raw)
                if (c == '\n') noOfLines++;

            if (noOfLines >= _settings.MaxSize_File)
            {
                logger.Log(
                    Smell.LargeUnit_File,
                    _settings.Severity_File,
                    $"Large file detected: {noOfLines} lines",
                    scriptFile.GetIdentifier());
            }
        }

        public override void EnterDeclaration(Declaration declaration)
        {
            if (declaration.DeclarationType.IsMacroType())
            {
                var size = declaration.GetSize();
                if (size >= _settings.MaxSize_Macro)
                {
                    logger.Log(
                        Smell.LargeUnit_Macro,
                        _settings.Severity_Macro,
                        $"Large macro detected: {size} statements",
                        declaration.GetIdentifier());
                }
            }
            else
            {
                AnalyseMacroBlocks(declaration);
            }
        }

        private void AnalyseMacroBlocks(Block block)
        {
            bool interpretAsScriptBlock = false;
            foreach (var child in block.Children)
            {
                if (child.NodeType != NodeType.Other)
                    interpretAsScriptBlock = true;
            }

            if (interpretAsScriptBlock) {
                var size = block.GetSize();
                if (size >= _settings.MaxSize_NonMacroBlock)
                {
                    logger.Log(
                        Smell.LargeUnit_NonMacroBlock,
                        _settings.Severity_NonMacroBlock,
                        $"Large block detected: {size} statements",
                        block.GetIdentifier());
                }
                return;
            }

            foreach (var child in block.Children)
            {
                if (child is Block blockChild)
                {
                    AnalyseMacroBlocks(blockChild);
                }
            }
        }
    }
}
