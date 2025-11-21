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

        public override int GetStrictHashCode()
        {
            //Comments don't count
            return 0;
        }
    }
}
