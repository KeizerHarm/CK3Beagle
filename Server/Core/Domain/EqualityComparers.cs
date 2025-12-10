using CK3BeagleServer.Core.Comparing;
using CK3BeagleServer.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CK3BeagleServer.Core.Domain
{
    /// <summary>
    /// Compare by True Hash
    /// </summary>
    public class TrueHashBasedNodeComparer : IEqualityComparer<Node>
    {
        public bool Equals(Node x, Node y)
        {
            return x.GetType() == y.GetType()
                && x.GetTrueHash() == y.GetTrueHash()
                && x.ParentSymbolType == y.ParentSymbolType;
        }

        public int GetHashCode([DisallowNull] Node obj)
        {
            return obj.GetTrueHash();
        }
    }

    /// <summary>
    /// If the entity has a key, compare by key. If not, compare by True Hash
    /// </summary>
    public class KeyBasedNodeComparer : IEqualityComparer<Node>
    {
        public bool Equals(Node x, Node y)
        {
            if (x.GetType() != y.GetType())
                return false;

            if (x.ParentSymbolType != y.ParentSymbolType)
                return false;

            var xKey = ""; var yKey = "";
            if (x is BinaryExpression binExpX)
                xKey = binExpX.Key;
            if (x is NamedBlock namedBlockX)
                xKey = namedBlockX.Key;
            if (y is BinaryExpression binExpY)
                yKey = binExpY.Key;
            if (y is NamedBlock namedBlockY)
                yKey = namedBlockY.Key;

            if (xKey != "" && xKey == yKey)
                return true;

            //Effectively empty key
            if (x is AnonymousBlock && y is AnonymousBlock)
                return true;

            //Fallback for non-keyed 
            return x.GetTrueHash() == y.GetTrueHash();
        }

        public int GetHashCode([DisallowNull] Node obj)
        {
            return obj.GetTrueHash();
        }
    }

    public class MixedComparer : IEqualityComparer<Node>
    {
        private readonly Dictionary<(Node, Node), bool> cache = new();

        public bool Equals(Node x, Node y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return CompareInternal(x, y, new HashSet<(Node, Node)>());
        }

        public int GetHashCode(Node obj)
            => obj.GetTrueHash();

        private bool CompareInternal(Node x, Node y, HashSet<(Node, Node)> visiting)
        {
            //Canonical ordering of pair avoids asymmetry
            var key = CanonicalPair(x, y);
            if (cache.TryGetValue(key, out var result))
                return result;

            //Detect recursion cycles
            if (!visiting.Add(key))
                return true; //cycle → treat as equal structurally

            result = CompareCore(x, y, visiting);

            //Record result symmetrically
            cache[key] = result;

            visiting.Remove(key);
            return result;
        }

        private bool CompareCore(Node x, Node y, HashSet<(Node, Node)> visiting)
        {
            //Basic type tests
            if (x.GetType() != y.GetType())
                return false;

            if (x.ParentSymbolType != y.ParentSymbolType)
                return false;

            //Hash equality → fast path
            if (x.GetTrueHash() == y.GetTrueHash())
                return true;

            //Label tests (BinaryExpression / NamedBlock)
            if (!LabelMatches(x, y))
                return false;

            //Block comparison
            if (x is Block bx && y is Block by)
                return CompareBlocks(bx, by, visiting);

            //BinaryExpression (value similarity)
            if (x is BinaryExpression b1 && y is BinaryExpression b2)
                return StringSimilarEnough(b1.Value, b2.Value);

            //Everything else → string similarity
            return StringSimilarEnough(x.StringRepresentation, y.StringRepresentation);
        }

        private bool CompareBlocks(Block bx, Block by, HashSet<(Node, Node)> visiting)
        {
            if (bx.Children.Count == 0 && by.Children.Count == 0)
                return true;

            //Greedy max-matching: each child of X may match at most one child of Y
            var matched = 0;
            var used = new bool[by.Children.Count];

            foreach (var xc in bx.Children)
            {
                for (var i = 0; i < by.Children.Count; i++)
                {
                    if (used[i])
                        continue;

                    if (CompareInternal(xc, by.Children[i], visiting))
                    {
                        matched++;
                        used[i] = true;
                        break;
                    }
                }
            }

            var maxCount = Math.Max(bx.Children.Count, by.Children.Count);
            var ratio = (float)matched / maxCount;

            return ratio >= 0.5f;
        }

        private bool LabelMatches(Node x, Node y)
        {
            string keyX = GetLabelKey(x);
            string keyY = GetLabelKey(y);

            if (keyX.Length == 0 && keyY.Length == 0)
                return false;

            return keyX == keyY;
        }

        private string GetLabelKey(Node node) =>
            node switch
            {
                BinaryExpression b => b.Key ?? "",
                NamedBlock nb => nb.Key ?? "",
                _ => ""
            };

        private bool StringSimilarEnough(string a, string b)
        {
            if (a == b)
                return true;

            var sim = ComparisonHelpers.CalcBigramSimilarity(a, b);
            return sim >= 0.6f;
        }

        private static (Node, Node) CanonicalPair(Node a, Node b)
            => a.GetHashCode() < b.GetHashCode() ? (a, b) : (b, a);
    }
}
