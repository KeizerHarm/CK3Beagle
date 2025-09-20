using CK3Analyser.Core.Domain.Entities;

namespace CK3Analyser.Core.Domain
{
    public interface IDomainVisitor
    {
        void Visit(Block block);
        void Visit(Comment comment);
        void Visit(Declaration declaration);
        void Visit(BinaryExpression binaryExpression);
        void Visit(NamedBlock namedBlock);
        void Visit(Node node);
        void Visit(ScriptFile scriptFile);
        void Visit(AnonymousBlock anonymousBlock);
    }
}
