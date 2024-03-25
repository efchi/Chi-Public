using Chi.Lexing.Abstract;

namespace Chi.Lexing
{
    /// <summary>
    /// Concrete implementation of a token for our language.
    /// </summary>
    public class Token : IToken<TokenType>
    {
        public Token() =>
            Source = string.Empty;

        #region Token Members

        public string Source { get; set; }
        public TokenType Type { get; set; }
        public int StartIndex { get; set; }
        public int NextIndex { get; set; }

        public int EndIndex => NextIndex - 1;
        public int Length => NextIndex - StartIndex;

        public string Text => NextIndex <= Source.Length ?
            Source.Substring(StartIndex, Length) : string.Empty;

        public void ChangeType(TokenType type) => 
            Type = type;

        #endregion

        #region Print Helpers

        public override string ToString() =>
            ToString(verbose: false);

        public string ToString(bool verbose)
        {
            var kind = (TokenKind)Language.Instance.GetKind(Type);
            var (line, column) = GetLineAndColumn();

            var text = Text
                .Replace(Lexer.NewLine1.ToString(), "\\r")
                .Replace(Lexer.NewLine2.ToString(), "\\n");

            return Type switch
            {
                TokenType.EOF => verbose ? $"[{line},{column}|{StartIndex}+{Length}|EOF]" : "EOF",
                TokenType.Whitespace => verbose ? $"[{line},{column}|{StartIndex}+{Length}|Whitespace|{text}]" : "█",

                _ => kind switch
                {
                    TokenKind.Punctuation or TokenKind.Keyword => 
                        verbose ? $"[{line},{column}|{StartIndex}+{Length}|{Type}]" : text,
                    _ =>
                        verbose ? $"[{line},{column}|{StartIndex}+{Length}|{Type}|{text}]" : text
                }
            };
        }

        public (int line, int column) GetLineAndColumn()
        {
            var line = 1;
            var column = 1;

            for (int i = 0; i < StartIndex; i++)
            {
                // Unix-style newline: \n.
                if (Source[i] == Lexer.NewLine2)
                {
                    line++;
                    column = 1;
                }
                // Windows-style newline: \r\n.
                else if (Source[i] == Lexer.NewLine1
                    && i + 1 < Source.Length && Source[i + 1] == Lexer.NewLine2)
                {
                    i++; // Skip the second character of a newline.
                    line++;
                    column = 1;
                }
                else
                    column++;
            }

            return (line, column);
        }

        #endregion
    }
}
