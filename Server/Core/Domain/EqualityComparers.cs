using CK3Analyser.Core.Domain.Entities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CK3Analyser.Core.Domain
{
    public class TrueHashBasedNodeComparer : IEqualityComparer<Node>
    {
        public bool Equals(Node x, Node y)
        {
            return x.GetType() == y.GetType()
                && x.GetTrueHash() == y.GetTrueHash();
        }

        public int GetHashCode([DisallowNull] Node obj)
        {
            return obj.GetTrueHash();
        }
    }
}
