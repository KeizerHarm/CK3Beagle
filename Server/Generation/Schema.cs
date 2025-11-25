using System;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Generation
{
    public enum BlockContext
    {
        None,
        Trigger,
        Effect
    }

    public class Schema
    {
        protected Schema Parent;
        private List<Schema> _schemasInTree;
        public List<Schema> SchemasInTree
        {
            get {
                if (Parent == null)
                {
                    _schemasInTree ??= [];
                    return _schemasInTree;
                }
                return Parent.SchemasInTree;
            }
        }

        public string FullCodeName { get; set; }
        public string LocalCodeName => SnakeToPascal(ScriptKey);
        public string ScriptKey { get; set; }
        public string Home { get; set; }

        public bool IsPlural { get; set; }
        public BlockContext BLOCKTYPE { get; set; } = BlockContext.None;
        public List<Schema> Children { get; } = new List<Schema>();

        public void AddChild(Schema child)
        {
            child.FullCodeName = FullCodeName + "_" + child.LocalCodeName;
            child.Parent = this;
            SchemasInTree.Add(child);
            Children.Add(child);
        }

        private static string SnakeToPascal(string input)
        {
            return string.Concat(
                input.Split(['_'], StringSplitOptions.RemoveEmptyEntries)
                     .Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1))
            );
        }
    }
}
