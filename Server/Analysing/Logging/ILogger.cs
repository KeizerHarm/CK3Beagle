using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources;

namespace CK3Analyser.Analysing.Logging
{
    public interface ILogger
    {
        void Log(LogEntry logEntry, params LogEntry[] relatedLogs);
        void Log(Smell smell, Severity severity, string message, string location,
            int affectedAreaStartLine = 0, int affectedAreaStartIndex = 0, int affectedAreaEndLine = 0, int affectedAreaEndIndex = 0, params LogEntry[] relatedLogs);
        void Log(Smell smell, Severity severity, string message, Node node, params LogEntry[] relatedLogs);
        void Log(Smell smell, Severity severity, string message, Node firstNode, Node lastNode, params LogEntry[] relatedLogs);
    }
}