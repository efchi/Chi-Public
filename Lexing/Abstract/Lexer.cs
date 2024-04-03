namespace Chi.Lexing.Abstract
{
    /// <summary>
    /// Represents an abstract lexer for an essential scripting language, supporting 
    /// whitespace, single-line comments, punctuation symbols, keywords, numbers, 
    /// identifiers and unrecognized symbols (in this order of precedence).
    /// </summary>
    /// <typeparam name="L">The language description type.</typeparam>
    /// <typeparam name="T">The token type.</typeparam>
    /// <typeparam name="Y">The enumeration representing a token type.</typeparam>
    public abstract class Lexer<L, T, Y>
        where L : ILanguage<T, Y>
        where T : IToken<Y>, new()
        where Y : struct, Enum
    {
        #region Constants

        /// <summary>
        /// Represents the end-of-file character.
        /// </summary>
        public const char EOF = '\0';

        /// <summary>
        /// Represents the first character of a new line in Windows-style newline: \r\n.
        /// </summary>
        public const char NewLine1 = '\r';

        /// <summary>
        /// Represents the second character of a new line in Windows-style newline: \r\n.
        /// </summary>
        public const char NewLine2 = '\n';

        #endregion

        #region Constructor & Properties

        /// <summary>
        /// Initializes a new instance of the <see cref="Lexer{L, T, Y}"/> class.
        /// </summary>
        /// <param name="language">The language description type.</param>
        public Lexer(L language) =>
            Language = language;

        /// <summary>
        /// Gets the language description type associated with the lexer.
        /// </summary>
        readonly L Language;

        /// <summary>
        /// Gets the source code being lexed.
        /// </summary>
        string Source;

        /// <summary>
        /// Gets a flag indicating whether to ignore whitespace tokens.
        /// </summary>
        bool IgnoreWhitespace;

        /// <summary>
        /// Gets a flag indicating whether to ignore comment tokens.
        /// </summary>
        bool IgnoreComments;

        /// <summary>
        /// Gets or sets the current index in the source code.
        /// </summary>
        protected int Index;

        #endregion

        #region Lexer Logic

        /// <summary>
        /// Runs the lexer and returns a list of tokens.
        /// </summary>
        /// <param name="source">The source code to lex.</param>
        /// <param name="ignoreWhitespace">A flag indicating whether to ignore whitespace tokens.</param>
        /// <param name="ignoreComments">A flag indicating whether to ignore comment tokens.</param>
        /// <returns>A list of tokens.</returns>
        public IList<T> Run(string source, bool ignoreWhitespace, bool ignoreComments)
        {
            Source = source;
            IgnoreWhitespace = ignoreWhitespace;
            IgnoreComments = ignoreComments;

            Reset();
            var tokens = new List<T>();

            T token;
            do
            {
                token = Match();
                tokens.Add(token);
            }
            while (!Language.IsEOF(token.Type));

            return tokens;
        }

        /// <summary>
        /// Resets the lexer to the beginning of the source code.
        /// </summary>
        void Reset() =>
            Index = 0;

        /// <summary>
        /// Moves the lexer forward by the specified number of steps.
        /// </summary>
        /// <param name="step">The number of steps to move forward.</param>
        protected void Forward(int step) =>
            Index += step;

        /// <summary>
        /// Gets the current character (at the current index) in the source code.
        /// </summary>
        protected char Current =>
            Get(Index);

        /// <summary>
        /// Gets the next character (after the current index) in the source code.
        /// </summary>
        protected char Next =>
            Get(Index + 1);

        /// <summary>
        /// Gets the character at the specified index in the source code.
        /// </summary>
        /// <param name="index">The index of the character.</param>
        /// <returns>The character at the specified index, or the end-of-file character if the index is out of range.</returns>
        protected char Get(int index) =>
            index >= 0 && index < Source.Length ? Source[index] : EOF;

        /// <summary>
        /// Matches the next token in the source code.
        /// </summary>
        /// <returns>The matched token.</returns>
        protected virtual T Match()
        {
            if (MatchWhitespace(out var token))
                return !IgnoreWhitespace ? token! : Match();

            if (MatchComment(out token))
                return !IgnoreComments ? token! : Match();

            if (Language.MatchPunctuation(Current, out var tokenType))
            {
                token = NewToken(tokenType!.Value);
                Forward(token.Length);
                return token;
            }

            if (MatchWord(out token))
            {
                var text = token!.Text;

                if (Language.MatchKeyword(text, out var newTokenType))
                    token.ChangeType(newTokenType!.Value);

                else if (Language.MatchNumber(text, out newTokenType))
                    token.ChangeType(newTokenType!.Value);

                else if (Language.MatchIdentifier(text, out newTokenType))
                    token.ChangeType(newTokenType!.Value);

                return token!;
            }

            if (MatchEOF(out token))
                return token!;

            MatchUnrecognized(out token);
            return token!;
        }

        /// <summary>
        /// Matches a whitespace token.
        /// </summary>
        /// <param name="token">The matched token.</param>
        /// <returns><c>true</c> if a whitespace token is matched; otherwise, <c>false</c>.</returns>
        protected virtual bool MatchWhitespace(out T? token)
        {
            if (!char.IsWhiteSpace(Current))
            {
                token = default;
                return false;
            }

            // Whitespace detected. Matching Whitespace block.

            var startIndex = Index;
            do Forward(1);
            while (char.IsWhiteSpace(Current));

            token = NewToken(Language.Whitespace, startIndex, Index);
            return true;
        }

        /// <summary>
        /// Matches a comment token.
        /// </summary>
        /// <param name="token">The matched token.</param>
        /// <returns><c>true</c> if a comment token is matched; otherwise, <c>false</c>.</returns>
        protected virtual bool MatchComment(out T? token)
        {
            if (!Language.IsCommentChar(Current))
            {
                token = default;
                return false;
            }

            // Comment detected. Matching single-line comment.

            var startIndex = Index;
            do Forward(1);
            while (Current != NewLine1 && Current != NewLine2 && Current != EOF);

            // If comment is on last line.
            if (Current == EOF)
            {
                token = NewToken(Language.Comment, startIndex, Index);
                return true;
            }

            // Unix-style newline: \n.
            if (Current == NewLine2)
            {
                Forward(1);
                token = NewToken(Language.Comment, startIndex, Index);
                return true;
            }

            // Windows-style newline: \r\n.
            if (Current == NewLine1 && Next == NewLine2)
            {
                Forward(2);
                token = NewToken(Language.Comment, startIndex, Index);
                return true;
            }

            // Should never reach this point:
            // program shall always end with EOF.
            throw new UnreachableException();
        }

        /// <summary>
        /// Matches a word token. A word is a generic token that could be a keyword, number or identifier.
        /// It's up to the implementation to determine the actual token type (see <see cref="Match"/>).
        /// </summary>
        /// <param name="token">The matched token.</param>
        /// <returns><c>true</c> if a word token is matched; otherwise, <c>false</c>.</returns>
        protected virtual bool MatchWord(out T? token)
        {
            if (!Language.IsWordChar(Current))
            {
                token = default;
                return false;
            }

            var startIndex = Index;
            do Forward(1);
            while (Language.IsWordChar(Current));

            token = NewToken(Language.Word, startIndex, Index);
            return true;
        }

        /// <summary>
        /// Matches a terminator token.
        /// </summary>
        /// <param name="token">The matched token.</param>
        /// <returns><c>true</c> if a terminator token is matched; otherwise, <c>false</c>.</returns>
        protected virtual bool MatchEOF(out T? token)
        {
            if (Current != EOF)
            {
                token = default;
                return false;
            }

            token = NewToken(Language.EOF);
            return true;
        }

        /// <summary>
        /// Matches an unrecognized token.
        /// </summary>
        /// <param name="token">The matched token.</param>
        /// <returns><c>true</c> if an unrecognized token is matched; otherwise, <c>false</c>.</returns>
        protected virtual bool MatchUnrecognized(out T token)
        {
            token = NewToken(Language.Unrecognized);
            Forward(1);
            return true;
        }

        #endregion

        #region Lexing Helpers

        /// <summary>
        /// Creates a new token with the specified token type at the current index.
        /// </summary>
        /// <param name="tokenType">The token type.</param>
        /// <returns>The new token.</returns>
        protected T NewToken(Y tokenType) =>
            NewToken(tokenType, Index);

        /// <summary>
        /// Creates a new token with the specified token type at the given index.
        /// </summary>
        /// <param name="tokenType">The token type.</param>
        /// <param name="startIndex">The start index of the token.</param>
        /// <returns>The new token.</returns>
        protected T NewToken(Y tokenType, int startIndex) =>
            NewToken(tokenType, startIndex, startIndex + 1);

        /// <summary>
        /// Creates a new token with the specified token type at the given start index, specifying the index of the next token.
        /// </summary>
        /// <param name="tokenType">The token type.</param>
        /// <param name="startIndex">The start index of the token.</param>
        /// <param name="nextIndex">The index of the next token.</param>
        /// <returns>The new token.</returns>
        protected T NewToken(Y tokenType, int startIndex, int nextIndex) => new()
        {
            Type = tokenType,
            Source = Source,
            StartIndex = startIndex,
            NextIndex = nextIndex
        };

        #endregion
    }
}
