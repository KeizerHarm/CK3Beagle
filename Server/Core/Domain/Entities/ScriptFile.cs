using CK3Analyser.Core.Generated;
using System;
using System.Collections.Generic;

namespace CK3Analyser.Core.Domain.Entities
{
    public class ScriptFile : Block
    {
        public Context Context;
        public string RelativePath;
        public string AbsolutePath;
        private string rawFileContents { get; set; }
        public OrderedDictionary<string, Declaration> Declarations = [];

        public DeclarationType ExpectedDeclarationType;

        public string GetContentSelectionString(int absoluteStartIndex, int absoluteEndIndex)
        {
            return rawFileContents[absoluteStartIndex..absoluteEndIndex];
        }

        public ReadOnlySpan<char> GetContentSelectionSpan(int absoluteStartIndex, int absoluteEndIndex)
        {
            return rawFileContents.AsSpan()[absoluteStartIndex..absoluteEndIndex];
        }

        public ScriptFile(Context context, string relativePath, DeclarationType expectedDeclarationType, string contents = null)
        {
            Context = context;
            RelativePath = relativePath;
            ExpectedDeclarationType = expectedDeclarationType;
            rawFileContents = contents;
            Start = new Position(0, 0, 0);
            End = new Position(0, 0, contents.Length);
        }

        public bool ContentsAndLocationMatch(ScriptFile other)
        {
            return RelativePath == other.RelativePath && StringRepresentation == other.StringRepresentation;
        }

        public void RegisterDeclaration(Declaration declaration, bool addText = false)
        {
            //AddChild(declaration);
            Declarations.Add(declaration.Key, declaration);

            //if (addText)
            //{
            //    Raw += declaration.Raw;
            //}
        }

        public void InsertDeclaration(Declaration newDecl, Declaration prevSibling)
        {
            if (!Declarations.ContainsKey(prevSibling.Key))
            {
                throw new Exception("Prev sibling not found upon insertion in ScriptFile!");
            }

            if (prevSibling.NextSibling != null) { 
                var oldNextSibling = prevSibling.NextSibling;
                oldNextSibling.PrevSibling = newDecl;
            }

            prevSibling.NextSibling = newDecl;
            //Raw = Raw.Replace(prevSibling.Raw, prevSibling.Raw + newDecl.Raw);
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

            //Raw = Raw.Replace(original.Raw, replacement.Raw);
        }

        public ScriptFile Clone()
        {
            return new ScriptFile(Context, RelativePath, ExpectedDeclarationType, StringRepresentation)
            {
                Declarations = new OrderedDictionary<string, Declaration>(Declarations)
            };
        }
        public override void Accept(IDomainVisitor visitor) => visitor.Visit(this);

        public override string GetLoneIdentifier() => RelativePath;



        #region hashing
        private int _duplicationCheckingHash;
        public override int GetDuplicationCheckingHash()
        {
            if (_duplicationCheckingHash == 0)
            {
                var hashCode = new HashCode();
                hashCode.Add(RelativePath);
                Children.ForEach(x =>
                {
                    var code = x.GetDuplicationCheckingHash();
                    if (code != 0)
                        hashCode.Add(x.GetDuplicationCheckingHash());
                });
                _duplicationCheckingHash = hashCode.ToHashCode();
            }

            return _duplicationCheckingHash;
        }
        private int _trueHash;
        public override int GetTrueHash()
        {
            if (_trueHash == 0)
            {
                var hashCode = new HashCode();
                hashCode.Add(RelativePath);
                Children.ForEach(x =>
                {
                    var code = x.GetTrueHash();
                    if (code != 0)
                        hashCode.Add(x.GetTrueHash());
                });
                _trueHash = hashCode.ToHashCode();
            }

            return _trueHash;
        }
        #endregion
    }
}
