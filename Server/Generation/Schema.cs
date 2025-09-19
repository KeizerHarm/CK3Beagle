using System.Collections.Generic;

namespace CK3Analyser.Generation
{
    public class Schema
    {
        public string Key { get; set; }
        public string Home { get; set; }

        public bool IsPlural { get; set; }
        public BlockType BLOCKTYPE { get; set; } = BlockType.Other;
        public List<Schema> Children { get; } = new List<Schema>();
    }
}
