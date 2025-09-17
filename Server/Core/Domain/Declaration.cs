namespace CK3Analyser.Core.Domain
{
    public class Declaration : NamedBlock
    {
        public EntityType EntityType { get; set; }

        public Declaration(string key, EntityType entityType) : base(key)
        {
            EntityType = entityType;
        }
        public override void Accept(IAnalysisVisitor visitor) => visitor.Visit(this);
    }
}
