using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources;
using System.Collections.Generic;

namespace CK3Analyser.Analysis.Logging
{
    public class Logger : ILogger
    {
        public List<LogEntry> LogEntries { get; } = new List<LogEntry>();

        public void Log(LogEntry logEntry, params LogEntry[] relatedLogs)
        {
            logEntry.RelatedLogEntries = relatedLogs;
            LogEntries.Add(logEntry);
        }

        public void Log(Smell smell, Severity severity, string message, Node node, params LogEntry[] relatedLogs)
        {
            Log(smell, severity, message, node, node, relatedLogs);
        }

        public void Log(Smell smell, Severity severity, string message, Node firstNode, Node lastNode, params LogEntry[] relatedLogs)
        {
            Log(smell, severity, message, firstNode.File.AbsolutePath, firstNode.Start.Line, firstNode.Start.Column, lastNode.End.Line, lastNode.End.Column, relatedLogs);
        }

        public void Log(Smell smell, Severity severity, string message, string location,
            int affectedAreaStartLine = 0, int affectedAreaStartIndex = 0, int affectedAreaEndLine = 0, int affectedAreaEndIndex = 0, params LogEntry[] relatedLogs)
        {
            var logEntry = new LogEntry(smell, severity, message, location, affectedAreaStartLine, affectedAreaStartIndex, affectedAreaEndLine, affectedAreaEndIndex);
            Log(logEntry, relatedLogs);
        }
    }
}
