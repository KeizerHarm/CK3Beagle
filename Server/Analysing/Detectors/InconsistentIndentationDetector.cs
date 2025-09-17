using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Analysis.Detectors
{
    public enum IndentationType
    {
        Tab, TwoSpaces, ThreeSpaces, FourSpaces, Inconclusive
    }

    public struct Line
    {
        public Line()
        {
            Depth = 0;
            LeadingSpaces = 0;
            LeadingTabs = 0;
        }

        public int Depth { get; set; }
        public int LeadingTabs { get; set; }
        public int LeadingSpaces { get; set; }



        public readonly bool IndentationTypeWorks(IndentationType type)
        {
            switch (type)
            {
                case IndentationType.Tab:
                    return Depth == LeadingTabs;
                case IndentationType.TwoSpaces:
                    return Depth == LeadingSpaces * 2;
                case IndentationType.ThreeSpaces:
                    return Depth == LeadingSpaces * 3;
                case IndentationType.FourSpaces:
                    return Depth == LeadingSpaces * 4;
                default:
                    return false;
            }
        }
    }

    public class InconsistentIndentationDetector : BaseDetector
    {
        public InconsistentIndentationDetector(Action<LogEntry> logFunc) : base(logFunc)
        {
        }

        public override void AnalyseScriptFile(ScriptFile scriptFile)
        {
            (var detectedIndentationType, var lines) = DetectIndentation(scriptFile);
            var expectedIndentationType = IndentationType.Tab;

            if (string.IsNullOrWhiteSpace(scriptFile.Raw)) return;

            if (detectedIndentationType != expectedIndentationType)
            {
                var logEntry = new LogEntry
                {
                    Location = scriptFile.GetIdentifier(),
                    Severity = Severity.Info,
                    Message = $"File uses indentation type {detectedIndentationType} instead of {expectedIndentationType}"
                };
                LogFunc(logEntry);
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
                var logEntry = new LogEntry
                {
                    Location = scriptFile.GetIdentifier(),
                    Severity = Severity.Warning,
                    Message = $"File is detected to use {detectedIndentationType} but it has {abberatingIndentedLines} lines that don't"
                };
                LogFunc(logEntry);
            }
        }

        private static (IndentationType detectedIndentationType, IEnumerable<Line> lines) DetectIndentation(ScriptFile scriptFile)
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

                        if (charac == '#') continue;
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
    }
}
