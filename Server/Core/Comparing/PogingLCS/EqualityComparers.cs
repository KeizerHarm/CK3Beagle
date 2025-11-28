using CK3Analyser.Core.Domain.Entities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CK3Analyser.Core.Comparing.PogingLCS
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
    public class TrueHashAndDeclKeyBasedNodeComparer : IEqualityComparer<Node>
    {
        public bool Equals(Node x, Node y)
        {
            if (x is Declaration declX && y is Declaration declY)
            {
                return declX.Key == declY.Key;
            }

            return x.GetType() == y.GetType()
                && x.GetTrueHash() == y.GetTrueHash();
        }

        public int GetHashCode([DisallowNull] Node obj)
        {
            return obj.GetTrueHash();
        }
    }
}
