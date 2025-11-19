using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources;
using System.Collections.Generic;

namespace CK3Analyser.Analysis.Logging
{
    public class Logger : ILogger
    {
        public List<LogEntry> LogEntries { get; } = new List<LogEntry>();

        public void Log(LogEntry logEntry)
        {
            LogEntries.Add(logEntry);
        }

        public void Log(Smell smell, Severity severity, string message, Node node)
        {
            Log(smell, severity, message, node, node);
        }

        public void Log(Smell smell, Severity severity, string message, Node firstNode, Node lastNode)
        {
            Log(smell, severity, message, firstNode.File.AbsolutePath, firstNode.Start.Line, firstNode.Start.Column, lastNode.End.Line, lastNode.End.Column);
        }

        public void Log(Smell smell, Severity severity, string message, string location,
            int affectedAreaStartLine = 0, int affectedAreaStartIndex = 0, int affectedAreaEndLine = 0, int affectedAreaEndIndex = 0)
        {
            Log(new LogEntry(smell, severity, message, location, affectedAreaStartLine, affectedAreaStartIndex, affectedAreaEndLine, affectedAreaEndIndex));
        }
    }
}
