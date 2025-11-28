using CK3Analyser.Core.Domain.Entities;
using System;
using System.Collections.Generic;

namespace CK3Analyser.Core.Comparing.PogingLCS
{
    /// <summary>
    /// Not actually equals, but checks if they can be considered similar enough to be treated as (a changed version of) the same node.
    /// </summary>
    public class NodeComparator : INodeComparator<Node>
    {
        public Stack<Delta> DeltaStack;
        public static Dictionary<(Node, Node), bool> CachedComparisons { get; } = [];

        public bool Match(Node x, Node y)
        {
            if (CachedComparisons.TryGetValue((x, y), out bool cachedResult))
                return cachedResult;

            if (x.GetType() != y.GetType()) 
                return CacheAndReturn(x, y, false);

            //Leaf types
            if (x is Comment xComment && y is Comment yComment)
            {
                var matches = ComparisonHelpers.StringMatches(xComment.RawWithoutHashtag, yComment.RawWithoutHashtag);
                return CacheAndReturn(x, y, matches);
            }
            if (x is AnonymousToken xToken && y is AnonymousToken yToken)
            {
                var matches = ComparisonHelpers.StringMatches(xToken.StringRepresentation, yToken.StringRepresentation);
                return CacheAndReturn(x, y, matches);
            }
            if (x is BinaryExpression xBinExp && y is BinaryExpression yBinExp)
            {
                //Consider bin exps equal if keys are equal
                //Test to see if it produces sane results
                var matches = xBinExp.Key == yBinExp.Key;
                return CacheAndReturn(x, y, matches);
            }
            if (x is Declaration xDecl && y is Declaration yDecl)
            {
                var matches = xDecl.Key == yDecl.Key
                    && xDecl.DeclarationType == yDecl.DeclarationType;
                return CacheAndReturn(x, y, matches);
            }
            if (x is NamedBlock xNamedBlock  && y is NamedBlock yNamedBlock)
            {
                var matches = xNamedBlock.Key == yNamedBlock.Key 
                    && ChildrenMatch(xNamedBlock.Children, yNamedBlock.Children);
                return CacheAndReturn(x, y, matches);
            }
            if (x is AnonymousBlock xAnonBlock  && y is AnonymousBlock yAnonBlock)
            {
                var matches = ChildrenMatch(xAnonBlock.Children, yAnonBlock.Children);
                return CacheAndReturn(x, y, matches);
            }
            if (x is ScriptFile)
            {
                var matches = true;
                return CacheAndReturn(x, y, matches);
            }
            throw new Exception("What?");
        }

        private bool CacheAndReturn(Node x, Node y, bool matches)
        {
            CachedComparisons.Add((x, y), matches);
            return matches;
        }

        private bool ChildrenMatch(List<Node> xChildren, List<Node> yChildren)
        {
            if (xChildren.Count == 0 && yChildren.Count == 0)
                return true;

            var maxNoOfChildren = Math.Max(xChildren.Count, yChildren.Count);
            var lcsLength = 0;
            //var lcsLength = LcsCalculator.ComputeLcsLength(CollectionsMarshal.AsSpan(xChildren), CollectionsMarshal.AsSpan(yChildren), this);

            if (lcsLength == 0)
                return false;

            if (lcsLength == maxNoOfChildren)
                return true;
            
            if (maxNoOfChildren <= 4)
            {
                return (float)lcsLength / maxNoOfChildren >= 0.4f;
            }
            return (float)lcsLength / maxNoOfChildren >= 0.6f;
        }
    }

    public interface INodeComparator<T>
    {
        bool Match(Node x, Node y);
    }
}
