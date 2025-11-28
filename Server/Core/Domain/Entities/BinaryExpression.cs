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

        private Scoper _scoper;
        public Scoper Scoper
        {
            get
            {
                return _scoper;
            }
            set
            {
                _scoper = value;
            }
        }
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

        #region hashing
        private int _duplicationCheckingHash;
        public override int GetDuplicationCheckingHash()
        {
            if (_duplicationCheckingHash == 0 && NodeType != NodeType.NonStatement)
            {
                var hashCode = new HashCode();
                hashCode.Add(Key);
                hashCode.Add(Scoper);
                hashCode.Add(Value);
                _duplicationCheckingHash = hashCode.ToHashCode();
            }

            return _duplicationCheckingHash;
        }

        private int _trueHash;
        public override int GetTrueHash()
        {
            if (_trueHash == 0)
            {
                var hashCode = new HashCode();
                hashCode.Add(Key);
                hashCode.Add(Scoper);
                hashCode.Add(Value);
                _trueHash = hashCode.ToHashCode();
            }

            return _trueHash;
        }
        #endregion
    }
}
