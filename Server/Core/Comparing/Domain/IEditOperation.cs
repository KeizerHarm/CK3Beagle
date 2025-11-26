using CK3Analyser.Core.Domain.Entities;

namespace CK3Analyser.Core.Comparing.Domain
{
    public interface IEditOperation
    {
        
    }
    public readonly struct InsertOperation(Node insertedNode, Block newParent, int newPosition) : IEditOperation
    {
        public readonly Node InsertedNode = insertedNode;
        public readonly Block NewParent = newParent;
        public readonly int NewPosition = newPosition;
    }
    public readonly struct DeleteOperation(Node node) : IEditOperation
    {
        public readonly Node DeletedNode = node;
    }
    public readonly struct UpdateOperation(Node updatedNode, string newValue) : IEditOperation
    {
        public readonly Node UpdatedNode = updatedNode;
        public readonly string NewValue = newValue;
    }
    public readonly struct MoveOperation(Node movedNode, Block newParent, int newPosition) : IEditOperation
    {
        public readonly Node MovedNode = movedNode;
        public readonly Block NewParent = newParent;
        public readonly int NewPosition = newPosition;
    }
}
