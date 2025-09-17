using System;

namespace CK3Analyser.Analysis.Logging
{
    public class Logger
    {
        public static void Log(LogEntry logEntry)
        {
            Console.WriteLine($"{logEntry.Severity.ToString().ToUpper()}: {logEntry.Message} at {logEntry.Location}");
        }
    }
}
