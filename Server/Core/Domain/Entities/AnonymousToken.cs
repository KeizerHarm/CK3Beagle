using System;

namespace CK3Analyser.Core.Domain.Entities
{
    public class AnonymousToken : Node
    {
        public string Value { get; set; }

        #region hashing
        private int _looseHashCode;
        public override int GetLooseHashCode()
        {
            if (_looseHashCode == 0)
            {
                var hashCode = new HashCode();
                hashCode.Add(Value);
                _looseHashCode = hashCode.ToHashCode();
            }

            return _looseHashCode;
        }

        public override int GetStrictHashCode()
        {
            return GetLooseHashCode();
        }
        #endregion
        public override void Accept(IDomainVisitor visitor) => visitor.Visit(this);
    }
}
