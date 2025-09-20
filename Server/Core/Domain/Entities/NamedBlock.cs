namespace CK3Analyser.Core.Domain.Entities
{
    public class NamedBlock : Block
    {
        public string Key { get; }

        private BlockType _ownBlockType;
        public BlockType BlockType {
            get {
                if (_ownBlockType != BlockType.None)
                    return _ownBlockType;

                if (Parent is NamedBlock blockParent)
                    _ownBlockType = blockParent.BlockType;

                return _ownBlockType;
            }
        }

        public NamedBlock(string key)
        {
            Key = key;
        }

        public override void Accept(IAnalysisVisitor visitor) => visitor.Visit(this);
        public override string GetLoneIdentifier() => Key;
    }
}
