using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Resources;

namespace CK3BeagleServer.Analysing.Logging
{
    public struct LogEntry
    {
        public Severity Severity { get; set; }
        public string Message { get; set; }
        public string Location { get; set; }
        public int AffectedAreaStartLine { get; set; }
        public int AffectedAreaEndLine { get; set; }
        public int AffectedAreaStartIndex { get; set; }
        public int AffectedAreaEndIndex { get; set; }
        public Smell Smell { get; set; }
        public LogEntry[] RelatedLogEntries { get; set; }

        public LogEntry (Smell smell, Severity severity, string message, string location, Position startPosition, Position endPosition)
        {
            Severity = severity;
            Message = message;
            Location = location;
            Smell = smell;
            AffectedAreaStartLine = startPosition.Line;
            AffectedAreaStartIndex = startPosition.Column;
            AffectedAreaEndLine = endPosition.Line;
            AffectedAreaEndIndex = endPosition.Column;
        }

        public LogEntry (Smell smell, Severity severity, string message, Node node)
        {
            Severity = severity;
            Message = message;
            Location = node.File.AbsolutePath;
            Smell = smell;
            AffectedAreaStartLine = node.Start.Line;
            AffectedAreaStartIndex = node.Start.Column;
            AffectedAreaEndLine = node.End.Line;
            AffectedAreaEndIndex = node.End.Column;
        }

        public LogEntry(Smell smell, Severity severity, string message, string location,
            int affectedAreaStartLine = 0, int affectedAreaStartIndex = 0, int affectedAreaEndLine = 0, int affectedAreaEndIndex = 0) : this()
        {
            Severity = severity;
            Message = message;
            Location = location;
            Smell = smell;
            AffectedAreaStartLine = affectedAreaStartLine;
            AffectedAreaStartIndex = affectedAreaStartIndex;
            AffectedAreaEndLine = affectedAreaEndLine;
            AffectedAreaEndIndex = affectedAreaEndIndex;
        }

        /// <summary>
        /// Should only be used for secondary entries, aka related info such as duplicate sources
        /// </summary>
        public static LogEntry MinimalLogEntry(string message, string location, Position startPosition, Position endPosition)
        {
            return new LogEntry
            {
                Message = message,
                Location = location,
                AffectedAreaStartLine = startPosition.Line,
                AffectedAreaStartIndex = startPosition.Column,
                AffectedAreaEndLine = endPosition.Line,
                AffectedAreaEndIndex = endPosition.Column
            };
        }

        /// <summary>
        /// Should only be used for secondary entries, aka related info such as duplicate sources
        /// </summary>
        public static LogEntry MinimalLogEntry(string message, Node node)
        {
            return MinimalLogEntry(
                message,
                node.File.AbsolutePath,
                node.Start,
                node.End);
        }

        public readonly string Print()
        {
            return $"{Severity.ToString().ToUpper()}: {Smell.GetCode()} - \"{Message}\" at {Location}";
        }
    }
}
