using System;
using System.Linq;

namespace CK3Analyser.Core.Domain.Entities
{
    public class AnonymousBlock : Block
    {
        public override void Accept(IDomainVisitor visitor) => visitor.Visit(this);

        public override string GetLoneIdentifier() => "<anonymous block>";

        #region hashing
        private int _looseHashCode;
        public override int GetLooseHashCode()
        {
            if (_looseHashCode == 0)
            {
                var hashCode = new HashCode();
                hashCode.Add(1); //Acknowledge existence of anonymous block in hash algorithm but not with distinct identity
                foreach (var relevantChild in Children.Where(x => x.NodeType != NodeType.NonStatement))
                {
                    hashCode.Add(relevantChild.GetLooseHashCode());
                }
                _looseHashCode = hashCode.ToHashCode();
            }

            return _looseHashCode;
        }

        private int _strictHashCode;
        public override int GetStrictHashCode()
        {
            if (_strictHashCode == 0)
            {
                var hashCode = new HashCode();
                hashCode.Add(1); //Acknowledge existence of anonymous block in hash algorithm but not with distinct identity
                foreach (var relevantChild in Children.Where(x => x.NodeType != NodeType.NonStatement))
                {
                    hashCode.Add(relevantChild.GetStrictHashCode());
                }
                _strictHashCode = hashCode.ToHashCode();
            }

            return _strictHashCode;
        }
        #endregion
    }
}
