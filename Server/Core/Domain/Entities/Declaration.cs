namespace CK3Analyser.Core.Domain.Entities
{
    public class Declaration : NamedBlock
    {
        public DeclarationType DeclarationType { get; set; }

        public Declaration(string key, DeclarationType declarationType) : base(key)
        {
            DeclarationType = declarationType;
        }
        public override void Accept(IDomainVisitor visitor) => visitor.Visit(this);
    }
}
