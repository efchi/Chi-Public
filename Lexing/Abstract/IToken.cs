namespace Chi.Lexing.Abstract
{
    /// <summary>
    /// Represents a token in the lexing process.
    /// </summary>
    /// <typeparam name="Y">The type of the token.</typeparam>
    public interface IToken<Y> : IToken where Y : struct, Enum
    {
        /// <summary>
        /// Gets the type of the token.
        /// </summary>
        public Y Type { get; set; }
        /// <summary>
        /// Changes the type of the token.
        /// </summary>
        /// <param name="type">The new type for the token.</param>
        public void ChangeType(Y type);
    }

    /// <summary>
    /// Represents a token in the lexing process.
    /// </summary>
    public interface IToken 
    {
        /// <summary>
        /// Gets the source string from which the token was extracted.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets the start index of the token in the source string.
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// Gets the index of the next token in the source string.
        /// </summary>
        public int NextIndex { get; set; }

        /// <summary>
        /// Gets the end index of the token in the source string.
        /// </summary>
        public int EndIndex { get; }

        /// <summary>
        /// Gets the length of the token.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the text content of the token.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Returns a string representation of the token.
        /// </summary>
        public string ToString(bool verbose);
    }
}
