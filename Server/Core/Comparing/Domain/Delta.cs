using CK3Analyser.Core.Domain.Entities;
using System.Collections.Generic;

namespace CK3Analyser.Core.Comparing.Domain
{
    public enum DeltaKind
    {
        Unchanged, Added, Deleted, Changed, ChangeInChildren
    }

    public class Delta
    {
        public static Delta Deleted(ShadowNode shadow)
        {
            return new Delta
            {
                Shadow = shadow,
                Kind = DeltaKind.Deleted
            };
        }
        public static Delta Changed(Node node, ShadowNode shadow)
        {
            return new Delta
            {
                Shadow = shadow,
                Node = node,
                Kind = DeltaKind.Changed
            };
        }
        public static Delta ChangeInChildren(Node node)
        {
            return new Delta
            {
                Node = node,
                Kind = DeltaKind.ChangeInChildren
            };
        }
        public static Delta Added(Node node)
        {
            return new Delta
            {
                Node = node,
                Kind = DeltaKind.Added
            };
        }
        public static Delta Unchanged(Node node)
        {
            return new Delta
            {
                Node = node,
                Kind = DeltaKind.Unchanged
            };
        }

        public Node Node;
        public ShadowNode Shadow;

        public DeltaKind Kind;

        public List<Delta> Children;
    }
}
