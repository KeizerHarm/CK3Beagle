namespace CK3Analyser.Analysis.Logging
{
    public enum Severity
    {
        Debug, Info, Warning, Critical
    }
    public struct LogEntry
    {
        public Severity Severity { get; set; }
        public string Message { get; set; }
        public string Location { get; set; }
        public int AffectedAreaStart { get; set; }
        public int AffectedAreaEnd { get; set; }
        public Smell Smell { get; set; }

        public LogEntry(Smell smell, Severity severity, string message, string location, int affectedAreaStart = 0, int affectedAreaEnd = 0) : this()
        {
            Severity = severity;
            Message = message;
            Location = location;
            Smell = smell;
            AffectedAreaStart = affectedAreaStart;
            AffectedAreaEnd = affectedAreaEnd;
        }

        public readonly string Print()
        {
            return $"{Severity.ToString().ToUpper()}: {Smell.GetCode()} - \"{Message}\" at {Location}";
        }
    }
}
