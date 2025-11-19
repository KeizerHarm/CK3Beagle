using CK3Analyser.Core.Domain.Entities;
using System;
using System.IO;

namespace CK3Analyser.Core.Parsing.Fast
{
    public class FastParser : ICk3Parser
    {
        public void ParseFile(ScriptFile file)
        {
            var input = File.ReadAllText(file.StringRepresentation);
            GatherDeclarations(file);
        }

        private static void GatherDeclarations(ScriptFile file)
        {
            int bracketDepth = 0;
            bool isInQuotes = false;
            bool isComment = false;
            int currentDeclarationStartIndex = 0;
            int prevDeclarationEndIndex = 0;
            string currentDeclarationToken = null;
            string text = file.StringRepresentation;
            int length = text.Length - 1;

            for (int index = 0; index <= length; index++)
            {
                char character = text[index];
                if (character == '"' && !isComment)
                {
                    isInQuotes = !isInQuotes;
                }
                if (character == '\n')
                {
                    isComment = false;
                }
                if (!isInQuotes)
                {
                    if (character == '#')
                    {
                        isComment = true;
                    }
                    if (character == '{' && !isComment)
                    {
                        bracketDepth++;
                        if (bracketDepth == 1)
                        {
                            (currentDeclarationToken, currentDeclarationStartIndex) = FindStart(text, index);
                        }
                    }
                    if (character == '}' && !isComment)
                    {
                        bracketDepth--;
                        if (bracketDepth == 0)
                        {
                            var raw = text.Substring(currentDeclarationStartIndex, index - currentDeclarationStartIndex + 1);
                            var preambleLength = Math.Max(0, currentDeclarationStartIndex - prevDeclarationEndIndex - 1);
                            var preamble = text.Substring(prevDeclarationEndIndex + 1, preambleLength);
                            var decl = new Declaration(currentDeclarationToken, file.ExpectedDeclarationType)
                            {
                                //Raw = preamble + raw
                            };
                            prevDeclarationEndIndex = index;
                            file.AddDeclaration(decl);
                            currentDeclarationToken = null;
                        }
                    }
                }
            }

            if (currentDeclarationToken != null)
            {
                var raw = text.Substring(currentDeclarationStartIndex, length - currentDeclarationStartIndex - 1);
                var preambleLength = Math.Max(0, currentDeclarationStartIndex - prevDeclarationEndIndex - 1);
                var preamble = text.Substring(prevDeclarationEndIndex + 1, preambleLength);
                var decl = new Declaration(currentDeclarationToken, file.ExpectedDeclarationType);
                file.AddDeclaration(decl);
            }
        }

        private static (string token, int startIndex) FindStart(string file, int searchStartsAtIndex)
        {
            bool hasEnteredToken = false;
            int tokenStartIndex = searchStartsAtIndex;
            int tokenEndIndex = searchStartsAtIndex;

            for (int i = searchStartsAtIndex; i >= 0; i--)
            {
                tokenStartIndex = i;
                char character = file[i];
                if (!char.IsWhiteSpace(character) && character != '=' && character != '{')
                {
                    if (!hasEnteredToken)
                    {
                        hasEnteredToken = true;
                        tokenEndIndex = i + 1; //Because we're going backwards
                    }
                }

                if (char.IsWhiteSpace(character) && hasEnteredToken)
                {
                    tokenStartIndex += 1; //Last non-whitespace character
                    break;
                }
            }
            var token = file.Substring(tokenStartIndex, tokenEndIndex - tokenStartIndex);
            return (token, tokenStartIndex);
        }
    }
}
