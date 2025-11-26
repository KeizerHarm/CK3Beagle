using CK3Analyser.Core.Resources;
using System;

namespace CK3Analyser.Core.Domain.Entities
{
    public class AnonymousToken : Node
    {
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

        #region hashing
        private int _duplicationCheckingHash;
        public override int GetDuplicationCheckingHash()
        {
            if (_duplicationCheckingHash == 0)
            {
                var hashCode = new HashCode();
                hashCode.Add(Value);
                _duplicationCheckingHash = hashCode.ToHashCode();
            }

            return _duplicationCheckingHash;
        }
        public override int GetTrueHash() => GetDuplicationCheckingHash();
        #endregion
        public override void Accept(IDomainVisitor visitor) => visitor.Visit(this);
    }
}
