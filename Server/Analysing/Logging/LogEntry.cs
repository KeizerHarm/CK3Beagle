namespace CK3Analyser.Analysis.Logging
{
    public enum Severity
    {
        Info, Warning, Critical
    }
    public struct LogEntry
    {
        public Severity Severity { get; set; }
        public string Message { get; set; }
        public string Location { get; set; }
    }
}
