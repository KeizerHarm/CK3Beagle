using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using System.Linq;

namespace CK3Analyser.Analysis.Detectors
{
    public class LargeUnitDetector : BaseDetector
    {
        public struct Settings
        {
            public int MaxSize_NonMacroBlock { get; set; }
            public Severity Severity_NonMacroBlock { get; set; }
            public int MaxSize_File { get; set; }
            public Severity Severity_File { get; set; }
            public int MaxSize_Macro { get; set; }
            public Severity Severity_Macro { get; set; }
        }

        private Settings _settings;

        public LargeUnitDetector(ILogger logger, Context context, Settings settings) : base(logger, context)
        {
            _settings = settings;
        }
        public override void AnalyseScriptFile(ScriptFile scriptFile)
        {
            var length = scriptFile.Raw.Split('\n').Length;
            if (length >= _settings.MaxSize_File)
            {
                logger.Log(
                    Smell.LargeUnit_File,
                    _settings.Severity_File,
                    $"Large file detected: {length} lines",
                    scriptFile.GetIdentifier());
            }
        }

        public override void AnalyseDeclaration(Declaration declaration)
        {
            var size = declaration.GetSize();
            if (size >= _settings.MaxSize_Macro && declaration.DeclarationType.IsMacroType())
            {
                logger.Log (
                    Smell.LargeUnit_Macro,
                    _settings.Severity_Macro,
                    $"Large macro detected: {size} statements",
                    declaration.GetIdentifier());
            }
            
            if (!declaration.DeclarationType.IsMacroType())
            {
                AnalyseMacroBlocks(declaration);
            }
        }

        private void AnalyseMacroBlocks(Block block)
        {
            if (block.Children.Any(x => x.NodeType == NodeType.Statement || x.NodeType == NodeType.Link)) {
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

            var blockChildren = block.Children.OfType<Block>();
            foreach (var blockChild in blockChildren)
            {
                AnalyseMacroBlocks(blockChild);
            }
        }
    }
}
