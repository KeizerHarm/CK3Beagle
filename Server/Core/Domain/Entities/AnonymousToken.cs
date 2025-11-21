using System;

namespace CK3Analyser.Core.Domain.Entities
{
    public class AnonymousToken : Node
    {
        public string Value;

        #region hashing
        private int _strictHashCode;
        public override int GetStrictHashCode()
        {
            if (_strictHashCode == 0)
            {
                var hashCode = new HashCode();
                hashCode.Add(Value);
                _strictHashCode = hashCode.ToHashCode();
            }

            return _strictHashCode;
        }
        #endregion
        public override void Accept(IDomainVisitor visitor) => visitor.Visit(this);
    }
}
