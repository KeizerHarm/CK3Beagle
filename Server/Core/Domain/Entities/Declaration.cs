using CK3Analyser.Core.Generated;
using System;
using System.Linq;

namespace CK3Analyser.Core.Domain.Entities
{
    public class Declaration : NamedBlock
    {
        public DeclarationType DeclarationType;

        public Declaration(string key, DeclarationType declarationType) : base(key)
        {
            DeclarationType = declarationType;
        }
        public override void Accept(IDomainVisitor visitor) => visitor.Visit(this);


        public override int GetHashCode()
        {
            return HashCode.Combine(Key, DeclarationType, Children.Where(x => x.GetType() != typeof(Comment)));
        }
    }
}
