using System;
using System.Collections.Generic;

namespace CK3Analyser.Analysis.Logging
{
    public class Logger : ILogger
    {
        public List<LogEntry> LogEntries { get; } = new List<LogEntry>();

        public void Log(LogEntry logEntry)
        {
            LogEntries.Add(logEntry);
            if (logEntry.Severity >= Severity.Info)
            {
                Console.WriteLine(logEntry.Print());
                Console.WriteLine();
            }
        }

        public void Log(Smell smell, Severity severity, string message, string location, int affectedAreaStart = 0, int affectedAreaEnd = 0)
        {
            Log(new LogEntry(smell, severity, message, location, affectedAreaStart, affectedAreaEnd));
        }
    }
}
