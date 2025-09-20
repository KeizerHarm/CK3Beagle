using System.Linq;
using System.Text;

namespace CK3Analyser.Core.Domain.Entities
{
    public class Node
    {
        public string Raw { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

        public Node PrevSibling { get; set; }
        public Node NextSibling { get; set; }
        public Node Parent { get; set; }

        public NodeType NodeType { get; set; }

        public virtual void Accept(IDomainVisitor visitor) => visitor.Visit(this);

        public virtual string GetLoneIdentifier() => " ";
        public string GetIdentifier()
        {
            var sb = new StringBuilder(GetLoneIdentifier());
            var parent = Parent;
            while (parent != null)
            {
                sb.Append("->" + parent.GetLoneIdentifier());
                parent = parent.Parent;
            }
            return string.Join("->", sb.ToString().Split("->").Reverse());
        }
    }
}
