using CK3Analyser.Core.Generated;
using System;
using System.Text;

namespace CK3Analyser.Core.Domain.Entities
{
    public readonly struct Position(int line, int column, int offset)
    {
        //0-based line number
        public readonly int Line = line;
        //0-based character index in line
        public readonly int Column = column;
        //0-based absolute index in file
        public readonly int Offset = offset;

        public override string ToString() => this.GenericToString();
    }

    public abstract class Node
    {
        public string StringRepresentation
        {
            get
            {
                return File.GetContentSelectionString(Start.Offset, End.Offset);
            }
        }
        public ReadOnlySpan<char> SpanRepresentation 
        {
            get
            {
                return File.GetContentSelectionSpan(Start.Offset, End.Offset);
            }
        }
        public Position Start;
        public Position End;

        public Node PrevSibling;
        public Node PrevNonCommentSibling {
            get
            {
                if (PrevSibling == null)
                    return null;

                if (PrevSibling.GetType() == typeof(Comment))
                {
                    return PrevSibling.PrevNonCommentSibling;
                }

                return PrevSibling;
            }
        }
        public Node PrevStatementOrLinkerSibling {
            get
            {
                if (PrevSibling == null)
                    return null;

                if (PrevSibling.NodeType == NodeType.NonStatement)
                {
                    return PrevSibling.PrevStatementOrLinkerSibling;
                }

                return PrevSibling;
            }
        }
        public Node NextSibling;
        public Node NextNonCommentSibling 
        {
            get
            {
                if (NextSibling == null)
                    return null;

                if (NextSibling.GetType() == typeof(Comment))
                {
                    return NextSibling.NextNonCommentSibling;
                }

                return NextSibling;
            }
        }
        public Node NextStatementOrLinkerSibling 
        {
            get
            {
                if (NextSibling == null)
                    return null;

                if (NextSibling.NodeType == NodeType.NonStatement)
                {
                    return NextSibling.NextStatementOrLinkerSibling;
                }

                return NextSibling;
            }
        }
        public Block Parent;
        public ScriptFile File
        {
            get
            {
                if (GetType() == typeof(ScriptFile))
                    return (ScriptFile)this;

                if (Parent == null)
                    return null;

                return Parent.File;
            }
        }

        public NodeType NodeType = NodeType.NonStatement;
        public int SemanticId = -1;
        public SymbolType SymbolType = SymbolType.Undefined;

        public abstract void Accept(IDomainVisitor visitor);

        public abstract int GetDuplicationCheckingHash();
        public abstract int GetTrueHash();

        public virtual int GetSize() => NodeType == NodeType.NonStatement ? 0 : 1;
    }
}
