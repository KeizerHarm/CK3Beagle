namespace CK3Analyser.Core.Domain
{
    public class KeyValuePair : Node
    {
        public string Key { get; }
        public string Scoper { get; }
        public string Value { get; }

        public KeyValuePair(string key, string scoper, string value)
        {
            Key = key;
            Scoper = scoper;
            Value = value;
        }
        public override void Accept(IAnalysisVisitor visitor) => visitor.Visit(this);
        public override string GetLoneIdentifier() => Key;
    }
}
