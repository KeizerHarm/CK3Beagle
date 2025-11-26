using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Core.Domain.Entities
{
    public abstract class Block : Node
    {
        public List<Node> Children = new List<Node>();
        public List<Node> ChildrenFlattened =>
            [.. Children.SelectMany(x => {
                if (x is Block block) return block.ChildrenFlattened; return []; 
            }).Union(Children)];

        public void AddChild(Node node, bool addText = false)
        {
            node.Parent = this;
            if (Children.Count > 0)
            {
                var lastChild = Children.Last();
                lastChild.NextSibling = node;
                node.PrevSibling = lastChild;
            }

            Children.Add(node);

            //if (addText)
            //{
            //    Raw += node.Raw;
            //}
        }

        public void InsertChild(Node newNode, Node prevSibling)
        {
            newNode.Parent = this;
            if (!Children.Contains(prevSibling))
            {
                throw new System.Exception("Prev sibling not found upon insertion!");
            }

            if (prevSibling.NextSibling != null)
            {
                var oldNextSibling = prevSibling.NextSibling;
                oldNextSibling.PrevSibling = newNode;
            }

            prevSibling.NextSibling = newNode;
            //Raw = Raw.Replace(prevSibling.Raw, prevSibling.Raw + newNode.Raw);
        }

        public void ReplaceChild(Node original, Node replacement)
        {
            replacement.Parent = this;
            if (original.PrevSibling != null)
            {
                original.PrevSibling.NextSibling = replacement;
            }

            if (original.NextSibling != null)
            {
                original.NextSibling.PrevSibling = replacement;
            }

            var oldIndex = Children.IndexOf(original);
            Children.Remove(original);
            Children.Insert(oldIndex, replacement);
        }

        private int _size = -1;
        public override int GetSize()
        {
            if (_size == -1)
            {
                var tempSize = 0;
                if (NodeType != NodeType.NonStatement)
                    tempSize += 1;

                tempSize += Children.Sum(x => x.GetSize());
                _size = tempSize;
            }
            return _size;
        }
    }
}
