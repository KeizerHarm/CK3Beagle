namespace CK3Analyser.Core.Domain.Entities
{
    public class Comment : Node
    {
        public string RawWithoutHashtag 
        {
            get
            {
                return StringRepresentation.Split('#')[1].Trim();

            }
        }
        public override void Accept(IDomainVisitor visitor) => visitor.Visit(this);

        public override int GetLooseHashCode()
        {
            //Should never be called for comments
            return 1;
        }

        public override int GetStrictHashCode()
        {
            //Should never be called for comments
            throw new System.InvalidOperationException();
        }
    }
}
