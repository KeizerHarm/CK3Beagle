using System;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Core.Domain.Entities
{
    public class AnonymousBlock : Block
    {
        public override void Accept(IDomainVisitor visitor) => visitor.Visit(this);

        public override int GetHashCode()
        {
            return HashCode.Combine(Children.Where(x => x.GetType() != typeof(Comment)));
        }

        public override string GetLoneIdentifier() => "<anonymous block>";


    }
}
