using System;

namespace CK3Analyser.Core.Domain.Entities
{
    public class AnonymousBlock : Block
    {
        public override void Accept(IDomainVisitor visitor) => visitor.Visit(this);

        public override string GetLoneIdentifier() => "<anonymous block>";

        #region hashing
        private int _strictHashCode;
        public override int GetStrictHashCode()
        {
            if (_strictHashCode == 0)
            {
                var hashCode = new HashCode();
                hashCode.Add(1); //Acknowledge existence of anonymous block in hash algorithm but not with distinct identity
                Children.ForEach(x =>
                {
                    var code = x.GetStrictHashCode();
                    if (code != 0)
                        hashCode.Add(x.GetStrictHashCode());
                });
                _strictHashCode = hashCode.ToHashCode();
            }

            return _strictHashCode;
        }
        #endregion
    }
}
