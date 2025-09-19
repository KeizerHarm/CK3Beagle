namespace CK3Analyser.Core.Domain.Entities
{
    public class Comment : Node
    {
        public string RawWithoutHashtag { get; set; }
        public override void Accept(IAnalysisVisitor visitor) => visitor.Visit(this);
    }
}
