using System;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Core.Domain.Entities
{
    public class ScriptFile : Block
    {
        public Context Context { get; }
        public string RelativePath { get; set; }
        public OrderedDictionary<string, Declaration> Declarations { get; private set; } = [];

        public DeclarationType ExpectedDeclarationType { get; }

        public ScriptFile(Context context, string relativePath, DeclarationType expectedDeclarationType, string rawContent = null)
        {
            Context = context;
            RelativePath = relativePath;
            Raw = rawContent;
            ExpectedDeclarationType = expectedDeclarationType;
        }

        public bool ContentsAndLocationMatch(ScriptFile other)
        {
            return RelativePath == other.RelativePath && Raw == other.Raw;
        }

        public void AddDeclaration(Declaration declaration, bool addText = false)
        {
            AddChild(declaration);
            Declarations.Add(declaration.Key, declaration);

            if (addText)
            {
                Raw += declaration.Raw;
            }
        }

        public void InsertDeclaration(Declaration newDecl, Declaration prevSibling)
        {
            if (!Declarations.ContainsKey(prevSibling.Key))
            {
                throw new System.Exception("Prev sibling not found upon insertion in ScriptFile!");
            }

            if (prevSibling.NextSibling != null) { 
                var oldNextSibling = prevSibling.NextSibling;
                oldNextSibling.PrevSibling = newDecl;
            }

            prevSibling.NextSibling = newDecl;
            Raw = Raw.Replace(prevSibling.Raw, prevSibling.Raw + newDecl.Raw);
        }

        public void ReplaceDeclaration(Declaration replacement)
        {
            if (!Declarations.TryGetValue(replacement.Key, out Declaration original))
                return;
            
            if (original.PrevSibling != null)
            {
                original.PrevSibling.NextSibling = replacement;
            }
            
            if (original.NextSibling != null)
            {
                original.NextSibling.PrevSibling = replacement;
            }

            var oldIndex = Declarations.IndexOf(original.Key);
            Declarations.Remove(original.Key);
            Declarations.Insert(oldIndex, replacement.Key, replacement);

            Raw = Raw.Replace(original.Raw, replacement.Raw);
        }

        public ScriptFile Clone()
        {
            return new ScriptFile(Context, RelativePath, ExpectedDeclarationType, Raw)
            {
                Declarations = new OrderedDictionary<string, Declaration>(Declarations)
            };
        }
        public override void Accept(IDomainVisitor visitor) => visitor.Visit(this);

        public override string GetLoneIdentifier() => RelativePath;


        public override int GetHashCode()
        {
            return HashCode.Combine(RelativePath, Children.Where(x => x.GetType() != typeof(Comment)));
        }
    }
}
