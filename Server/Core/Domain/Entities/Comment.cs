using SharpCompress.Common;
using System;
using System.Text.RegularExpressions;

namespace CK3BeagleServer.Core.Domain.Entities
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

        public override int GetDuplicationCheckingHash()
        {
            //Comments don't count
            return 0;
        }

        private int _trueHash;
        public override int GetTrueHash()
        {
            if (_trueHash == 0)
            {
                var hashCode = new HashCode();
                hashCode.Add(RawWithoutHashtag);
                _trueHash = hashCode.ToHashCode();
            }

            return _trueHash;
        }
    }
}
