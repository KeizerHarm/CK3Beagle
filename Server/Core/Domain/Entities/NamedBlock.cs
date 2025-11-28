using CK3Analyser.Core.Resources;
using System;
using System.Linq;

namespace CK3Analyser.Core.Domain.Entities
{
    public class NamedBlock : Block
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

        public NamedBlock(string key = "", Scoper scoper = Scoper.Equal)
        {
            Key = key;
            Scoper = scoper;
        }

        public override void Accept(IDomainVisitor visitor) => visitor.Visit(this);

        #region hashing

        private int _duplicationCheckingHash;
        public override int GetDuplicationCheckingHash()
        {
            if (_duplicationCheckingHash == 0)
            {
                var hashCode = new HashCode();
                if (NodeType != NodeType.NonStatement)
                {
                    hashCode.Add(Key);
                    hashCode.Add(Scoper);
                }
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
                hashCode.Add(Key);
                hashCode.Add(Scoper);
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
