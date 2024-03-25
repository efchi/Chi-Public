namespace Chi.Lexing.Abstract
{
    /// <summary>
    /// Represents an abstract language description used for lexing and parsing.
    /// </summary>
    /// <typeparam name="T">The type of the token.</typeparam>
    /// <typeparam name="Y">The enumeration representing the token types.</typeparam>
    public interface ILanguage<T, Y> 
        where T : IToken<Y>
        where Y : struct, Enum
    {
        /// <summary>
        /// Gets the token type for whitespace.
        /// </summary>
        public Y Whitespace { get; }

        /// <summary>
        /// Gets the token type for comments.
        /// </summary>
        public Y Comment { get; }

        /// <summary>
        /// Gets the token type for words.
        /// </summary>
        public Y Word { get; }

        /// <summary>
        /// Gets the token type for unrecognized tokens.
        /// </summary>
        public Y Unrecognized { get; }

        /// <summary>
        /// Gets the token type for EOF.
        /// </summary>
        public Y EOF { get; }

        /// <summary>
        /// Determines whether the specified token type is a terminator.
        /// </summary>
        /// <param name="tokenType">The token type to check.</param>
        /// <returns><c>true</c> if the token type is a terminator; otherwise, <c>false</c>.</returns>
        public bool IsEOF(Y tokenType);

        /// <summary>
        /// Determines whether the specified token type is whitespace.
        /// </summary>
        /// <param name="tokenType">The token type to check.</param>
        /// <returns><c>true</c> if the token type is whitespace; otherwise, <c>false</c>.</returns>
        public bool IsWhitespace(Y tokenType);

        /// <summary>
        /// Determines whether the specified token type is a comment.
        /// </summary>
        /// <param name="tokenType">The token type to check.</param>
        /// <returns><c>true</c> if the token type is a comment; otherwise, <c>false</c>.</returns>
        public bool IsComment(Y tokenType);

        /// <summary>
        /// Determines whether the specified character is a comment character.
        /// </summary>
        /// <param name="char">The character to check.</param>
        /// <returns><c>true</c> if the character is a comment character; otherwise, <c>false</c>.</returns>
        public bool IsCommentChar(char @char);

        /// <summary>
        /// Determines whether the specified character is a word character.
        /// </summary>
        /// <param name="char">The character to check.</param>
        /// <returns><c>true</c> if the character is a word character; otherwise, <c>false</c>.</returns>
        public bool IsWordChar(char @char);

        /// <summary>
        /// Matches the specified punctuation character and returns the corresponding token type.
        /// </summary>
        /// <param name="current">The current character to match.</param>
        /// <param name="tokenType">When this method returns, contains the token type of the matched punctuation character, if successful; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if the punctuation character is matched; otherwise, <c>false</c>.</returns>
        public bool MatchPunctuation(char current, out Y? tokenType);

        /// <summary>
        /// Matches the specified keyword and returns the corresponding token type.
        /// </summary>
        /// <param name="text">The keyword to match.</param>
        /// <param name="newTokenType">When this method returns, contains the token type of the matched keyword, if successful; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if the keyword is matched; otherwise, <c>false</c>.</returns>
        public bool MatchKeyword(string text, out Y? newTokenType);

        /// <summary>
        /// Matches the specified number and returns the corresponding token type.
        /// </summary>
        /// <param name="text">The number to match.</param>
        /// <param name="newTokenType">When this method returns, contains the token type of the matched number, if successful; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if the number is matched; otherwise, <c>false</c>.</returns>
        public bool MatchNumber(string text, out Y? newTokenType);

        /// <summary>
        /// Matches the specified identifier and returns the corresponding token type.
        /// </summary>
        /// <param name="text">The identifier to match.</param>
        /// <param name="newTokenType">When this method returns, contains the token type of the matched identifier, if successful; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if the identifier is matched; otherwise, <c>false</c>.</returns>
        public bool MatchIdentifier(string text, out Y? newTokenType);

        /// <summary>
        /// Gets the kind of the specified token type.
        /// </summary>
        /// <param name="tokenType">The token type.</param>
        /// <returns>The kind of the token type.</returns>
        public Enum GetKind(Y tokenType);
    }
}
