using Chi.Lexing.Abstract;

namespace Chi.Lexing
{
    /// <summary>
    /// Implements a concrete lexer for our language.
    /// The lexer matches identifiers with dots in the middle (not the first character) 
    /// as a single token. These identifiers are then split by the post-processor.
    /// </summary>
    public class Lexer : Lexer<Language, Token, TokenType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Lexer"/> class with the specified source code, whitespace handling, and comment handling.
        /// </summary>
        /// <param name="source">The source code to be lexed.</param>
        /// <param name="ignoreWhitespace">Specifies whether to ignore whitespace during lexing.</param>
        /// <param name="ignoreComments">Specifies whether to ignore comments during lexing.</param>
        public Lexer(string source, bool ignoreWhitespace, bool ignoreComments)
            : base(Language.Instance, source, ignoreWhitespace, ignoreComments)
        {
        }

        #region Print Helpers

        public static void Print(IEnumerable<Token> tokens, bool verbose, bool ignoreWhitespace, bool ignoreComments)
        {
            var count = tokens.Count();
            var i = 1;

            foreach (var token in tokens)
            {
                var kind = (TokenKind)Language.Instance.GetKind(token.Type);

                if (kind == TokenKind.Whitespace && ignoreWhitespace == true)
                    continue;

                if (kind == TokenKind.Comment && ignoreComments == true)
                    continue;

                var (foregroundColor, backgroundColor) = kind switch
                {
                    TokenKind.EOF => (ConsoleColor.DarkYellow, ConsoleColor.Black),
                    TokenKind.Whitespace => (ConsoleColor.DarkGray, ConsoleColor.Black),
                    TokenKind.Comment => (ConsoleColor.DarkGreen, ConsoleColor.Black),
                    TokenKind.Punctuation => (ConsoleColor.Green, ConsoleColor.Black),
                    TokenKind.Keyword => (ConsoleColor.Blue, ConsoleColor.Black),
                    TokenKind.Number => (ConsoleColor.Cyan, ConsoleColor.Black),
                    TokenKind.Identifier => (ConsoleColor.White, ConsoleColor.Black),
                    TokenKind.Word => (ConsoleColor.Magenta, ConsoleColor.Black),
                    TokenKind.Unrecognized => (ConsoleColor.Red, ConsoleColor.Black),
                    _ => (ConsoleColor.Gray, ConsoleColor.Black),
                };

                Output.Write(token.ToString(verbose), foregroundColor, backgroundColor);
                Output.Write(i < count ? " " : Environment.NewLine);
                i++;
            }
        }

        #endregion
    }
}
