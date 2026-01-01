using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Linq;

namespace CK3BeagleServer.Core.Comparing.Domain
{
    public enum DeltaKind
    {
        Unchanged, Added, Deleted, Changed, ChangedInChildren
    }

    public class Delta
    {
        public Delta Parent;
        public Delta GetPrevSibling()
        {
            if (Parent == null)
                return null;

            var indexInParent = Parent.Children.IndexOf(this);
            return Parent.Children.ElementAtOrDefault(indexInParent - 1);
        }
        public Delta GetNextSibling()
        {
            if (Parent == null)
                return null;

            var indexInParent = Parent.Children.IndexOf(this);
            return Parent.Children.ElementAtOrDefault(indexInParent + 1);
        }

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
                Kind = DeltaKind.ChangedInChildren
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
        public void AddChild(Delta delta)
        {
            Children.Add(delta);
            delta.Parent = this;
        }

        public string ToTreeString(int indent = 0)
        {
            string thisDelta = "{ " + Kind + " }\n";
            if (indent != 0)
            {
                thisDelta = new string('\t', indent) + "└" + thisDelta;
            }
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    thisDelta += child.ToTreeString(indent + 1);
                }
            }
            return thisDelta;
        }

        public override string ToString()
        {
            var key = "";
            if (Node != null)
            {
                if (Node is BinaryExpression binExp)
                    key = binExp.Key;
                if (Node is NamedBlock namedBlock)
                    key = namedBlock.Key;
                if (Node is AnonymousToken token)
                    key = token.Value;
                if (Node is Comment comment)
                    key = comment.StringRepresentation;
                if (Node is AnonymousBlock block)
                    key = "<block>";
                return "{ " + Kind + ": '" + key + "' }";
            }
            else if (Shadow != null)
            {
                return "{ " + Kind + ": '" + Shadow.StringRepresentation + "' }";
            }
            else
            {
                return "{ " + Kind + " }";
            }
        }

        public void Accept(IDeltaVisitor visitor)
        {
            switch (Kind)
            {
                case DeltaKind.Unchanged:
                    visitor.VisitUnchanged(this);
                    break;
                case DeltaKind.Added:
                    visitor.VisitAdded(this);
                    break;
                case DeltaKind.Deleted:
                    visitor.VisitDeleted(this);
                    break;
                case DeltaKind.Changed:
                    visitor.VisitChanged(this);
                    break;
                case DeltaKind.ChangedInChildren:
                    visitor.VisitChangedInChildren(this);
                    break;
            }
        }
    }
}
