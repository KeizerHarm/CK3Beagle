using CK3BeagleServer.Core.Generated;

namespace CK3BeagleServer.Core.Domain.Entities
{
    public class Declaration : NamedBlock
    {
        public DeclarationType DeclarationType;

        public Declaration(string key, DeclarationType declarationType) : base(key)
        {
            DeclarationType = declarationType;
        }
        public override void Accept(IDomainVisitor visitor) => visitor.Visit(this);
    }
}
