using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources;

namespace CK3Analyser.Analysis.Logging
{
    public interface ILogger
    {
        void Log(LogEntry logEntry);
        void Log(Smell smell, Severity severity, string message, string location,
            int affectedAreaStartLine = 0, int affectedAreaStartIndex = 0, int affectedAreaEndLine = 0, int affectedAreaEndIndex = 0);
        void Log(Smell smell, Severity severity, string message, Node node);
        void Log(Smell smell, Severity severity, string message, Node firstNode, Node lastNode);
    }
}