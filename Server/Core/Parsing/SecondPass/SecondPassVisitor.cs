using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK3Analyser.Core.Parsing.SecondPass
{
    public class SecondPassVisitor : IDomainVisitor
    {
        public void Visit(Block block)
        {
            throw new NotImplementedException();
        }

        public void Visit(Comment comment)
        {
            throw new NotImplementedException();
        }

        public void Visit(Declaration declaration)
        {
            throw new NotImplementedException();
        }

        public void Visit(BinaryExpression binaryExpression)
        {
            throw new NotImplementedException();
        }

        public void Visit(NamedBlock namedBlock)
        {
            throw new NotImplementedException();
        }

        public void Visit(Node node)
        {
            throw new NotImplementedException();
        }

        public void Visit(ScriptFile scriptFile)
        {
            throw new NotImplementedException();
        }

        public void Visit(AnonymousBlock anonymousBlock)
        {
            throw new NotImplementedException();
        }
    }
}
