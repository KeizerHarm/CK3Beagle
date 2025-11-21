using System;
using System.Linq;

namespace CK3Analyser.Core.Domain.Entities
{
    public class NamedBlock : Block
    {
        public string Key { get; set; }
        public string Scoper { get; set; }

        public NamedBlock(string key = "", string scoper = "=")
        {
            Key = key;
            Scoper = scoper;
        }

        public override void Accept(IDomainVisitor visitor) => visitor.Visit(this);
        public override string GetLoneIdentifier() => Key;

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Children.Where(x => x.GetType() != typeof(Comment)));
        }

        #region hashing
        private int _looseHashCode;
        public override int GetLooseHashCode()
        {
            if (_looseHashCode == 0)
            {
                var hashCode = new HashCode();
                hashCode.Add(Key);
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
                if (NodeType != NodeType.NonStatement)
                {
                    hashCode.Add(Key);
                }
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
