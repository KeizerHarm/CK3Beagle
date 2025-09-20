using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Analysis.Detectors
{
    public enum IndentationType
    {
        Tab, TwoSpaces, ThreeSpaces, FourSpaces, Inconclusive
    }

    public class InconsistentIndentationDetector : BaseDetector
    {
        public struct Settings
        {
            public IndentationType ExpectedIndentationType { get; set; }
            public Severity Severity_UnexpectedType { get; set; }
            public bool DisregardBracketsInComments { get; set; }
            public Severity Severity_Inconsistency { get; set; }
        }

        private Settings _settings;

        public InconsistentIndentationDetector(ILogger logger, Context context, Settings settings) : base(logger, context)
        {
            _settings = settings;
        }

        public override void AnalyseScriptFile(ScriptFile scriptFile)
        {
            if (string.IsNullOrWhiteSpace(scriptFile.Raw)) return;

            (var detectedIndentationType, var lines) = DetectIndentation(scriptFile);
            logger.Log(Smell.None, Severity.Debug, "Detected indentation type " + detectedIndentationType, scriptFile.GetIdentifier());

            if (detectedIndentationType != _settings.ExpectedIndentationType)
            {
                logger.Log(
                    Smell.InconsistentIndentation_UnexpectedType,
                    _settings.Severity_UnexpectedType, 
                    $"File uses indentation type {detectedIndentationType} instead of {_settings.ExpectedIndentationType}", 
                    scriptFile.GetIdentifier());
            }

            if (detectedIndentationType == IndentationType.Inconclusive)
                return;

            int abberatingIndentedLines = 0;
            foreach (var line in lines)
            {
                if (!line.IndentationTypeWorks(detectedIndentationType)) abberatingIndentedLines++;
            }
            if (abberatingIndentedLines > 0)
            {
                logger.Log(
                    Smell.InconclusiveIndentation_Inconsistency,
                    _settings.Severity_Inconsistency,
                    $"File is detected to use {detectedIndentationType} but it has {abberatingIndentedLines} lines that don't",
                    scriptFile.GetIdentifier());
            }
        }

        private (IndentationType detectedIndentationType, IEnumerable<Line> lines) DetectIndentation(ScriptFile scriptFile)
        {
            int currentDepth = 0;
            var lines = new List<Line>();
            int[] linesThatWorkForIndentationTypes = new int[Enum.GetValues<IndentationType>().Length];
            foreach (var line in scriptFile.Raw.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var lineObj = new Line();
                lineObj.Depth = currentDepth;
                bool isInLeadingWhitespace = true;
                bool isInQuotedString = false;
                int localBracketBalance = 0;

                foreach (var charac in line)
                {
                    if (charac == '"') isInQuotedString = !isInQuotedString;
                    if (!isInQuotedString)
                    {
                        if (!char.IsWhiteSpace(charac)) isInLeadingWhitespace = false;

                        if (charac == '#' && _settings.DisregardBracketsInComments) break; ;
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
            }

            public int Depth { get; set; }
            public int LeadingTabs { get; set; }
            public int LeadingSpaces { get; set; }
            public int CharacterCountBeforeThisLine { get; set; }


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
