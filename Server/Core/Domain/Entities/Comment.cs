using System;

namespace CK3BeagleServer.Core.Domain.Entities
{
    public class Comment : Node
    {
        public override void Accept(IDomainVisitor visitor) => visitor.Visit(this);

        public override int GetDuplicationCheckingHash()
        {
            //Comments don't count
            return 0;
        }

        private int _trueHash;
        public override int GetTrueHash()
        {
            if (_trueHash == 0)
            {
                var hashCode = new HashCode();
                hashCode.Add(WhitespaceAgnosticStringRepresentation);
                _trueHash = hashCode.ToHashCode();
            }

            return _trueHash;
        }
    }
}
