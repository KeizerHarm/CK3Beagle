using System.Text;

namespace CK3Analyser.Core.Domain.Entities
{
    public abstract class Node
    {
        public string Raw { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

        public Node PrevSibling { get; set; }
        public Node PrevNonCommentSibling {
            get
            {
                if (PrevSibling == null)
                    return null;

                if (PrevSibling.GetType() == typeof(Comment))
                {
                    return PrevSibling.PrevNonCommentSibling;
                }

                return PrevSibling;
            }
        }
        public Node PrevStatementOrLinkerSibling {
            get
            {
                if (PrevSibling == null)
                    return null;

                if (PrevSibling.NodeType == NodeType.Other)
                {
                    return PrevSibling.PrevStatementOrLinkerSibling;
                }

                return PrevSibling;
            }
        }
        public Node NextSibling { get; set; }
        public Node NextNonCommentSibling 
        {
            get
            {
                if (NextSibling == null)
                    return null;

                if (NextSibling.GetType() == typeof(Comment))
                {
                    return NextSibling.NextNonCommentSibling;
                }

                return NextSibling;
            }
        }
        public Node NextStatementOrLinkerSibling 
        {
            get
            {
                if (NextSibling == null)
                    return null;

                if (NextSibling.NodeType == NodeType.Other)
                {
                    return NextSibling.NextStatementOrLinkerSibling;
                }

                return NextSibling;
            }
        }
        public Block Parent { get; set; }

        public NodeType NodeType { get; set; }

        public abstract void Accept(IDomainVisitor visitor);

        public virtual string GetLoneIdentifier() => " ";
        public string GetIdentifier()
        {
            var sb = new StringBuilder(GetLoneIdentifier());
            var parent = Parent;
            while (parent != null)
            {
                sb.Insert(0, parent.GetLoneIdentifier() + "->");
                parent = parent.Parent;
            }
            return sb.ToString();
        }

        public abstract int GetLooseHashCode();
        public abstract int GetStrictHashCode();

        public virtual int GetSize() => NodeType == NodeType.Other ? 0 : 1;
    }
}
