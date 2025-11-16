using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources.DetectorSettings;

namespace CK3Analyser.Analysis.Detectors
{
    public class LargeUnitDetector : BaseDetector
    {
        private readonly LargeUnitSettings _settings;

        public LargeUnitDetector(ILogger logger, Context context, LargeUnitSettings settings) : base(logger, context)
        {
            _settings = settings;
        }

        public override void EnterScriptFile(ScriptFile scriptFile)
        {
            int noOfLines = 1;
            foreach (char c in scriptFile.Raw)
                if (c == '\n') noOfLines++;

            if (noOfLines >= _settings.File_MaxSize)
            {
                logger.Log(
                    Smell.LargeUnit_File,
                    _settings.File_Severity,
                    $"Large file detected: {noOfLines} lines",
                    scriptFile);
            }
        }

        public override void EnterDeclaration(Declaration declaration)
        {
            if (declaration.DeclarationType.IsMacroType())
            {
                var size = declaration.GetSize();
                if (size >= _settings.Macro_MaxSize)
                {
                    logger.Log(
                        Smell.LargeUnit_Macro,
                        _settings.Macro_Severity,
                        $"Large macro detected: {size} statements",
                        declaration);
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
                if (child.NodeType != NodeType.NonStatement)
                    interpretAsScriptBlock = true;
            }

            if (interpretAsScriptBlock) {
                var size = block.GetSize();
                if (size >= _settings.NonMacroBlock_MaxSize)
                {
                    logger.Log(
                        Smell.LargeUnit_NonMacroBlock,
                        _settings.NonMacroBlock_Severity,
                        $"Large block detected: {size} statements",
                        block);
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
