namespace CK3Analyser.Core.Domain
{
    public interface IAnalysisVisitor
    {
        void Visit(Block block);
        void Visit(Comment comment);
        void Visit(Declaration declaration);
        void Visit(KeyValuePair keyValuePair);
        void Visit(NamedBlock namedBlock);
        void Visit(Node node);
        void Visit(ScriptFile scriptFile);
    }
}
