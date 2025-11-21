using CK3Analyser.Core.Resources;
using System;

namespace CK3Analyser.Core.Domain.Entities
{
    public class BinaryExpression : Node
    {
        private int _key;
        public string Key
        {
            get
            {
                return _key != -1
                    ? GlobalResources.StringTable.GetString(_key)
                    : "";
            }
            set
            {
                _key =
                    value != ""
                    ? GlobalResources.StringTable.GetId(value)
                    : -1;
            }
        }
        public Scoper Scoper;
        private int _value;
        public string Value
        {
            get
            {
                return _value != -1
                    ? GlobalResources.StringTable.GetString(_value)
                    : "";
            }
            set
            {
                _value =
                    value != ""
                    ? GlobalResources.StringTable.GetId(value)
                    : -1;
            }
        }

        public BinaryExpression(string key = "", Scoper scoper = Scoper.Equal, string value = "")
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
