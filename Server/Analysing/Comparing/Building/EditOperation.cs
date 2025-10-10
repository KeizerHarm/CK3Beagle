using CK3Analyser.Core.Domain.Entities;

namespace CK3Analyser.Analysis.Comparing.Building
{
    public class EditOperation
    {
        
    }
    public class InsertOperation : EditOperation
    {
        public Node InsertedNode { get; set; }
        public Block NewParent { get; set; }
        public int NewPosition { get; set; }
    }
    public class DeleteOperation : EditOperation
    {
        public Node DeletedNode { get; set; }
    }
    public class UpdateOperation : EditOperation
    {
        public Node UpdatedNode { get; set; }
        public string NewValue { get; set; }
    }
    public class MoveOperation : EditOperation
    {
        public Node MovedNode { get; set; }
        public Block NewParent { get; set; }
        public int NewPosition { get; set; }
    }
}
