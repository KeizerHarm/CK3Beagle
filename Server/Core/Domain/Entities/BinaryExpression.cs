using System;

namespace CK3Analyser.Core.Domain.Entities
{
    public class BinaryExpression : Node
    {
        public string Key { get; set; }
        public string Scoper { get; set; }
        public string Value { get; set; }

        public BinaryExpression(string key = "", string scoper = "", string value = "")
        {
            Key = key;
            Scoper = scoper;
            Value = value;
        }
        public override void Accept(IDomainVisitor visitor) => visitor.Visit(this);
        public override string GetLoneIdentifier() => Key;

        public override bool Equals(object obj)
        {
            return obj is BinaryExpression expression &&
                   Key == expression.Key &&
                   Scoper == expression.Scoper &&
                   Value == expression.Value;
        }

        public override int GetHashCode() => GetStrictHashCode();


        #region hashing
        private int _looseHashCode;
        public override int GetLooseHashCode()
        {
            if (_looseHashCode == 0)
            {
                var hashCode = new HashCode();
                hashCode.Add(Key);
                hashCode.Add(Scoper);
                _looseHashCode = hashCode.ToHashCode();
            }

            return _looseHashCode;
        }

        private int _strictHashCode;
        public override int GetStrictHashCode()
        {
            if (_strictHashCode == 0 && NodeType != NodeType.NonStatement)
            {
                var hashCode = new HashCode();
                hashCode.Add(Key);
                hashCode.Add(Scoper);
                hashCode.Add(Value);
                _strictHashCode = hashCode.ToHashCode();
            }

            return _strictHashCode;
        }
        #endregion
    }
}
