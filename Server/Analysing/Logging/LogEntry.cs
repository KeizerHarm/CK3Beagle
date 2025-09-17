using System;

namespace CK3Analyser.Analysis.Logging
{
    public enum Severity
    {
        Debug, Info, Warning, Critical
    }
    public enum Smell
    {
        InconsistentIndentation_UnexpectedType,
        InconclusiveIndentation_Inconsistency
    }
    public static class SmellExtensions
    {
        public static string GetCode(this Smell smell)
        {
            switch (smell)
            {
                case Smell.InconsistentIndentation_UnexpectedType:
                    return "II.1";
                case Smell.InconclusiveIndentation_Inconsistency:
                    return "II.2";
                default:
                    throw new ArgumentException();
            }
        }
    }
    public struct LogEntry
    {
        public Severity Severity { get; set; }
        public string Message { get; set; }
        public string Location { get; set; }
        public int AffectedAreaStart { get; set; }
        public int AffectedAreaEnd { get; set; }
        public Smell? Smell { get; set; }
    }
}
