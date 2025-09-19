using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK3Analyser.Core.Domain
{
    public class AnonymousBlock : Block
    {
        public override void Accept(IAnalysisVisitor visitor) => visitor.Visit(this);
        public override string GetLoneIdentifier() => "<anonymous block>";
    }
}
