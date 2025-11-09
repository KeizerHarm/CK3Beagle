using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources;
using System;
using System.Collections.Generic;

namespace CK3Analyser.Analysis.Logging
{
    public class Logger : ILogger
    {
        public List<LogEntry> LogEntries { get; } = new List<LogEntry>();

        public Action<string> progressDelegate { get; set; }

        public void Log(LogEntry logEntry)
        {
            progressDelegate("Found one smell!\n\n" + logEntry.ToString());
            LogEntries.Add(logEntry);
            //if (logEntry.Severity >= Severity.Info)
            //{
            //    Console.WriteLine(logEntry.Print());
            //    Console.WriteLine();
            //}
        }

        public void Log(Smell smell, Severity severity, string message, Node node)
        {
            Log(smell, severity, message, node, node);
        }

        public void Log(Smell smell, Severity severity, string message, Node firstNode, Node lastNode)
        {
            Log(new LogEntry(smell, severity, message, firstNode.File.AbsolutePath, firstNode.StartLine, firstNode.StartIndex, lastNode.EndLine, lastNode.EndIndex));
        }

        public void Log(Smell smell, Severity severity, string message, string location,
            int affectedAreaStartLine = 0, int affectedAreaStartIndex = 0, int affectedAreaEndLine = 0, int affectedAreaEndIndex = 0)
        {
            Log(new LogEntry(smell, severity, message, location, affectedAreaStartLine, affectedAreaStartIndex, affectedAreaEndLine, affectedAreaEndIndex));
        }
    }
}
