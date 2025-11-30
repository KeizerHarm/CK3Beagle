using System;

namespace CK3BeagleServer.Core.Domain.Entities
{
    public class AnonymousBlock : Block
    {
        public override void Accept(IDomainVisitor visitor) => visitor.Visit(this);

        #region hashing
        private int _duplicationCheckingHash;
        public override int GetDuplicationCheckingHash()
        {
            if (_duplicationCheckingHash == 0)
            {
                var hashCode = new HashCode();
                hashCode.Add(1); //Acknowledge existence of anonymous block in hash algorithm but not with distinct identity
                Children.ForEach(x =>
                {
                    var code = x.GetDuplicationCheckingHash();
                    if (code != 0)
                        hashCode.Add(x.GetDuplicationCheckingHash());
                });
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
                hashCode.Add(1); //Acknowledge existence of anonymous block in hash algorithm but not with distinct identity
                Children.ForEach(x =>
                {
                    var code = x.GetTrueHash();
                    if (code != 0)
                        hashCode.Add(x.GetTrueHash());
                });
                _trueHash = hashCode.ToHashCode();
            }

            return _trueHash;
        }
        #endregion
    }
}
