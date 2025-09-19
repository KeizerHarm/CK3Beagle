namespace CK3Analyser.Analysis.Logging
{
    public interface ILogger
    {
        void Log(LogEntry logEntry);
        void Log(Smell smell, Severity severity, string message, string location, int affectedAreaStart = 0, int affectedAreaEnd = 0);
    }
}