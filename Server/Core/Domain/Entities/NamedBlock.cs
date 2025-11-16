using System;
using System.Linq;

namespace CK3Analyser.Core.Domain.Entities
{
    public class NamedBlock : Block
    {
        public string Key { get; }
        public string Scoper { get; }

        //private BlockType _ownBlockType;
        //public BlockType BlockType {
        //    get {
        //        if (_ownBlockType != BlockType.None)
        //            return _ownBlockType;

        //        if (Parent is NamedBlock blockParent)
        //            _ownBlockType = blockParent.BlockType;

        //        return _ownBlockType;
        //    }
        //}

        public NamedBlock(string key, string scoper = "=")
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
                hashCode.Add(Key);
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
