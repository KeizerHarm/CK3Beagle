using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources;
using CK3Analyser.Core.Resources.DetectorSettings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Analysis.Detectors
{
    public class InconsistentIndentationDetector : BaseDetector
    {
        private readonly InconsistentIndentationSettings _settings;

        public InconsistentIndentationDetector(ILogger logger, Context context, InconsistentIndentationSettings settings) : base(logger, context)
        {
            _settings = settings;
        }

        public override void EnterScriptFile(ScriptFile scriptFile)
        {
            if (string.IsNullOrWhiteSpace(scriptFile.Raw)) return;

            (var detectedIndentationType, var lines) = DetectIndentation(scriptFile);
            logger.Log(Smell.None, Severity.Debug, "Detected indentation type " + detectedIndentationType, scriptFile.GetIdentifier());

            if (!_settings.AllowedIndentationTypes.Contains(detectedIndentationType))
            {
                logger.Log(
                    Smell.InconsistentIndentation_UnexpectedType,
                    _settings.UnexpectedType_Severity, 
                    $"File uses indentation type {detectedIndentationType} instead of any of {string.Join(',', _settings.AllowedIndentationTypes)}", 
                    scriptFile);
            }

            if (detectedIndentationType == IndentationType.Inconclusive)
                return;

            int firstAbberatingLine = -1;
            int lastAbberatingLine = -1;
            foreach (Line line in lines)
            {
                if (!line.IndentationTypeWorks(detectedIndentationType))
                {
                    if (firstAbberatingLine == -1)
                        firstAbberatingLine = line.LineIndexInFile;

                    lastAbberatingLine = line.LineIndexInFile;
                }
                else
                {
                    if (firstAbberatingLine != -1)
                    {
                        logger.Log (
                            Smell.InconsistentIndentation_Inconsistency,
                            _settings.AbberatingLines_Severity,
                            $"Lines are not consistent with indentation type {detectedIndentationType} used by rest of file",
                            scriptFile.AbsolutePath,
                            firstAbberatingLine,
                            0,
                            lastAbberatingLine,
                            int.MaxValue);

                        firstAbberatingLine = -1;
                        lastAbberatingLine = -1;
                    }
                }
            }
            if (firstAbberatingLine != -1)
            {
                logger.Log(
                    Smell.InconsistentIndentation_Inconsistency,
                    _settings.AbberatingLines_Severity,
                    $"Lines are not consistent with indentation type {detectedIndentationType} used by rest of file",
                    scriptFile.AbsolutePath,
                    firstAbberatingLine,
                    0,
                    lastAbberatingLine,
                    int.MaxValue);
            }
        }

        private (IndentationType detectedIndentationType, List<Line> lines) DetectIndentation(ScriptFile scriptFile)
        {
            int currentDepth = 0;
            var lines = new List<Line>();
            int[] linesThatWorkForIndentationTypes = new int[Enum.GetValues<IndentationType>().Length];
            string[] rawLines = scriptFile.Raw.Split('\n');
            for (int i = 0; i < rawLines.Length; i++)
            {
                string line = rawLines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var lineObj = new Line
                {
                    Depth = currentDepth,
                    LineIndexInFile = i
                };
                bool isInLeadingWhitespace = true;
                bool isInQuotedString = false;
                int localBracketBalance = 0;

                foreach (var charac in line)
                {
                    if (charac == '"') isInQuotedString = !isInQuotedString;
                    if (!isInQuotedString)
                    {
                        if (!char.IsWhiteSpace(charac)) isInLeadingWhitespace = false;

                        if (charac == '#' && !_settings.AccountCommentedBrackets) break; ;
                        if (charac == ' ' && isInLeadingWhitespace) lineObj.LeadingSpaces++;
                        if (charac == '\t' && isInLeadingWhitespace) lineObj.LeadingTabs++;
                        if (charac == '{') localBracketBalance++;
                        if (charac == '}') localBracketBalance--;
                    }
                }
                currentDepth += localBracketBalance;
                if (localBracketBalance < 0)
                {
                    lineObj.Depth += localBracketBalance;
                }

                foreach (var indentationType in Enum.GetValues<IndentationType>())
                {
                    if (lineObj.IndentationTypeWorks(indentationType) && lineObj.Depth > 0)
                        linesThatWorkForIndentationTypes[(int)indentationType]++;
                }
                lines.Add(lineObj);
            }

            var lineCount = lines.Where(x => x.Depth > 0).Count();
            var detectedIndentationType = IndentationType.Inconclusive;
            foreach (var indentationType in Enum.GetValues<IndentationType>())
            {
                if (linesThatWorkForIndentationTypes[(int)indentationType] >= lineCount * 2 / 3)
                {
                    detectedIndentationType = indentationType;
                }
            }

            return (detectedIndentationType, lines);
        }

        internal struct Line
        {
            public Line()
            {
                Depth = 0;
                LeadingSpaces = 0;
                LeadingTabs = 0;
                CharacterCountBeforeThisLine = 0;
                LineIndexInFile = 0;
            }

            public int Depth { get; set; }
            public int LeadingTabs { get; set; }
            public int LeadingSpaces { get; set; }
            public int CharacterCountBeforeThisLine { get; set; }
            public int LineIndexInFile { get; set; }


            public readonly bool IndentationTypeWorks(IndentationType type)
            {
                switch (type)
                {
                    case IndentationType.Tab:
                        return Depth == LeadingTabs;
                    case IndentationType.TwoSpaces:
                        return Depth * 2 == LeadingSpaces;
                    case IndentationType.ThreeSpaces:
                        return Depth * 3 == LeadingSpaces;
                    case IndentationType.FourSpaces:
                        return Depth * 4 == LeadingSpaces;
                    default:
                        return false;
                }
            }
        }
    }
}
