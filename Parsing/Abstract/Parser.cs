using Chi.Lexing.Abstract;

namespace Chi.Parsing.Abstract
{
    /// <summary>
    /// Represents an abstract parser.
    /// </summary>
    /// <typeparam name="L">The type of the language.</typeparam>
    /// <typeparam name="T">The type of the token.</typeparam>
    /// <typeparam name="Y">The type of the token type enumeration.</typeparam>
    public abstract class Parser<L, T, Y>
        where L : ILanguage<T, Y>
        where T : IToken<Y>, new()
        where Y : struct, Enum
    {
        #region Constructor & Properties

        readonly L Language;
        protected T[] Source;
        int Index;

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser{L, T, Y}"/> class.
        /// </summary>
        /// <param name="language">The language.</param>
        public Parser(L language) =>
            Language = language;

        #endregion

        #region Parser Primitives

        /// <summary>
        /// Resets the parser to the beginning of the source tokens.
        /// </summary>
        protected void Reset() =>
            Index = 0;

        /// <summary>
        /// Gets the current token.
        /// </summary>
        protected T Current =>
            Get(Index);

        /// <summary>
        /// Gets the next token.
        /// </summary>
        protected T Next =>
            Get(Index + 1);

        /// <summary>
        /// Gets the token at the specified index.
        /// </summary>
        /// <param name="index">The index of the token.</param>
        /// <returns>The token at the specified index, or a new token with EOF type if the index is out of range.</returns>
        protected T Get(int index) =>
            index >= 0 && index < Source.Length ? Source[index] : new T { Type = Language.EOF };

        /// <summary>
        /// Moves the parser forward by the specified number of steps.
        /// </summary>
        /// <param name="step">The number of steps to move forward. Default is 1.</param>
        protected void Forward(int step = 1) =>
            Index += step;

        /// <summary>
        /// Matches the current token with the specified type.
        /// </summary>
        /// <param name="type">The expected token type.</param>
        /// <returns>The current token if it matches the expected type.</returns>
        /// <exception cref="MatchException{Y}">Thrown if the current token does not match the expected type.</exception>
        protected T Match(Y type)
        {
            if (!EqualityComparer<Y>.Default.Equals(Current.Type, type))
                throw new MatchException<Y>($"Expected [{type}]", Current);

            var current = Current;
            Forward();
            return current;
        }

        /// <summary>
        /// Creates a predict exception with the specified message.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <returns>A new instance of <see cref="PredictException{Y}"/> with the specified message.</returns>
        protected Exception Predict(string message) =>
            new PredictException<Y>($"Expected {message}", Current);

        #endregion
    }
}
