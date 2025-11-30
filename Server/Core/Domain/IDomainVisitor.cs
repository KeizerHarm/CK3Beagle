using CK3BeagleServer.Core.Domain.Entities;

namespace CK3BeagleServer.Core.Domain
{
    public interface IDomainVisitor
    {
        void Visit(ScriptFile scriptFile);
        void Visit(Declaration declaration);
        void Visit(NamedBlock namedBlock);
        void Visit(AnonymousBlock anonymousBlock);
        void Visit(BinaryExpression binaryExpression);
        void Visit(AnonymousToken token);
        void Visit(Comment comment);
    }
}
