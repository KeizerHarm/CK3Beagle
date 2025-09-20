using System;

namespace CK3Analyser.Core.Domain.Entities
{
    public class BinaryExpression : Node
    {
        public string Key { get; }
        public string Scoper { get; }
        public string Value { get; }

        public BinaryExpression(string key, string scoper, string value)
        {
            Key = key;
            Scoper = scoper;
            Value = value;
        }
        public override void Accept(IAnalysisVisitor visitor) => visitor.Visit(this);
        public override string GetLoneIdentifier() => Key;

        public override bool Equals(object obj)
        {
            return obj is BinaryExpression expression &&
                   Key == expression.Key &&
                   Scoper == expression.Scoper &&
                   Value == expression.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Scoper, Value);
        }
    }
}
