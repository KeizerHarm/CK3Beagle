using System.Text.RegularExpressions;

namespace CK3Analyser.Core.Domain.Entities
{
    public class Comment : Node
    {
        private static Regex commentRegex = new Regex("^\\s*#", RegexOptions.Compiled);
        public string RawWithoutHashtag 
        {
            get
            {
                return commentRegex.Replace(StringRepresentation, "").Trim();

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
