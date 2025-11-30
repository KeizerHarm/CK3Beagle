using CK3BeagleServer.Core.Domain.Entities;

namespace CK3BeagleServer.Core.Domain
{
    /// <summary>
    /// Standard implementation of IDomainVisitor that visits every node
    /// </summary>
    public class BaseDomainVisitor : IDomainVisitor
    {
        public virtual void Visit(AnonymousBlock anonymousBlock)
        {
            foreach (var child in anonymousBlock.Children)
            {
                child.Accept(this);
            }
        }

        public virtual void Visit(Comment comment)
        {
        }

        public virtual void Visit(Declaration declaration)
        {
            foreach (var child in declaration.Children)
            {
                child.Accept(this);
            }
        }

        public virtual void Visit(BinaryExpression binaryExpression)
        {
        }

        public virtual void Visit(NamedBlock namedBlock)
        {
            foreach (var child in namedBlock.Children)
            {
                child.Accept(this);
            }
        }

        public virtual void Visit(AnonymousToken anonymousToken)
        {
        }

        public virtual void Visit(ScriptFile scriptFile)
        {
            foreach (var child in scriptFile.Children)
            {
                child.Accept(this);
            }
        }
    }
}
