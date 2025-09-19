namespace CK3Analyser.Core.Domain.Entities
{
    public class NamedBlock : Block
    {
        public string Key { get; }

        public NamedBlock(string key)
        {
            Key = key;
        }

        public override void Accept(IAnalysisVisitor visitor) => visitor.Visit(this);
        public override string GetLoneIdentifier() => Key;
    }
}
