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
        public override string GetLoneIdentifier() => Key;

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Children.Where(x => x.GetType() != typeof(Comment)));
        }

        #region hashing

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
