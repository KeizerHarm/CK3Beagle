using CK3Analyser.Analysing.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources;
using CK3Analyser.Core.Resources.DetectorSettings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Analysing.Common.Detectors
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
            if (string.IsNullOrWhiteSpace(scriptFile.StringRepresentation)) return;

            (var detectedIndentationType, var lines) = DetectIndentation(scriptFile);

            if (lines.Count  == 0) return;

            logger.Log(Smell.None, Severity.Debug, "Detected indentation type " + detectedIndentationType, scriptFile);

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

            string[] rawLines = scriptFile.StringRepresentation.Split('\n');

            for (int i = 0; i < rawLines.Length; i++)
            {
                string line = rawLines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var lineObj = new Line
                {
                    LineIndexInFile = i
                };

                int index = 0;
                int leadingClosers = 0;
                bool inQuoted = false;

                // Count whitespace
                while (index < line.Length && char.IsWhiteSpace(line[index]))
                {
                    if (line[index] == ' ') lineObj.LeadingSpaces++;
                    else if (line[index] == '\t') lineObj.LeadingTabs++;
                    index++;
                }

                // Count closing parens
                while (index < line.Length && (line[index] == '}' || char.IsWhiteSpace(line[index]) ||
                    line[index] == '#' && _settings.CommentHandling == CommentHandling.CommentedBracketsCount))
                {
                    if (line[index] ==  '}')
                        leadingClosers++;

                    index++;
                }

                // Determine current depth
                currentDepth -= leadingClosers;
                if (currentDepth < 0) currentDepth = 0;

                lineObj.Depth = currentDepth;

                // Rest of line
                int balance = 0;
                bool inLeadingWhitespace = true;

                while (index < line.Length)
                {
                    char c = line[index];

                    if (c == '"')
                    {
                        inQuoted = !inQuoted;
                        index++;
                        continue;
                    }

                    if (!inQuoted)
                    {
                        // Comment handling
                        if (c == '#')
                        {
                            if (_settings.CommentHandling == CommentHandling.CommentsIgnored)
                            {
                                if (inLeadingWhitespace)
                                    goto NEXTLINE;
                                else
                                    break;
                            }

                            if (_settings.CommentHandling == CommentHandling.NoSpecialTreatment)
                                break;
                        }

                        if (!char.IsWhiteSpace(c))
                            inLeadingWhitespace = false;

                        if (c == '{') balance++;
                        if (c == '}') balance--;
                    }

                    index++;
                }

                // Reckon all
                currentDepth += balance;
                if (currentDepth < 0) currentDepth = 0;

                if (lineObj.Depth != 0)
                {
                    foreach (var indentationType in Enum.GetValues<IndentationType>())
                    {
                        if (lineObj.IndentationTypeWorks(indentationType) && lineObj.Depth > 0)
                            linesThatWorkForIndentationTypes[(int)indentationType]++;
                    }
                    lines.Add(lineObj);
                }

            NEXTLINE:;
            }

            var relevantLineCount = lines.Count(x => x.Depth > 0);
            var detectedIndentationType = IndentationType.Inconclusive;
            foreach (var indentationType in Enum.GetValues<IndentationType>())
            {
                if (linesThatWorkForIndentationTypes[(int)indentationType] >= relevantLineCount * 2 / 3)
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
                return type switch
                {
                    IndentationType.Tab => Depth == LeadingTabs,
                    IndentationType.TwoSpaces => Depth * 2 == LeadingSpaces,
                    IndentationType.ThreeSpaces => Depth * 3 == LeadingSpaces,
                    IndentationType.FourSpaces => Depth * 4 == LeadingSpaces,
                    _ => false,
                };
            }
        }
    }
}
