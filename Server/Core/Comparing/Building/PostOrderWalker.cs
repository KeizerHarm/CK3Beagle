using CK3Analyser.Core.Comparing.Domain;
using CK3Analyser.Core.Domain.Entities;
using System;

namespace CK3Analyser.Core.Comparing.Building
{
    public class PreOrderWalker
    {
        public PreOrderWalker(Func<Node, bool> action)
        {
            Action = action;
        }

        public Func<Node, bool> Action { get; set; }

        public void Walk(Node node)
        {
            var stop = Action(node);

            if (!stop && node is Block block)
            {
                foreach (var child in block.Children)
                {
                    Walk(child);
                }
            }
        }
    }

    public class PostOrderWalker
    {
        public PostOrderWalker(Action<Node> action)
        {
            Action = action;
        }

        public Action<Node> Action { get; set; }

        public void Walk(Node node)
        {
            if (node is Block block)
            {
                foreach (var child in block.Children)
                {
                    Walk(child);
                }
            }

            Action(node);
        }
    }

    public class PostOrderShallowWalker
    {
        public PostOrderShallowWalker(Action<ShallowNode> action)
        {
            Action = action;
        }

        public Action<ShallowNode> Action { get; set; }

        public void Walk(ShallowNode node)
        {
            foreach (var child in node.Children)
            {
                Walk(child);
            }

            Action(node);
        }
    }
}
