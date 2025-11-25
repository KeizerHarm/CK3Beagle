using CK3Analyser.Core.Domain.Entities;
using System;

namespace CK3Analyser.Analysing.Comparing.Building
{
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
}
