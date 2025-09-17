using System;
using System.Collections.Generic;

namespace CK3Analyser.Analysis.Logging
{
    public class Logger
    {
        public List<LogEntry> LogEntries { get; } = new List<LogEntry>();

        public void Log(LogEntry logEntry)
        {
            LogEntries.Add(logEntry);
            Console.WriteLine($"{logEntry.Severity.ToString().ToUpper()}: {logEntry.Message} at {logEntry.Location}");
        }
    }
}
