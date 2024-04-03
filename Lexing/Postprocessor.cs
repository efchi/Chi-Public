using Chi.Lexing.Abstract;

namespace Chi.Lexing
{
    /// <summary>
    /// Represents a postprocessor for tokenizing the source code.
    /// </summary>
    /// <typeparam name="Language">The concrete language description type.</typeparam>
    /// <typeparam name="TokenType">The concrete enumeration token type.</typeparam>
    public class Postprocessor : Postprocessor<Language, Token, TokenType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Postprocessor"/> class with the specified language and source code.
        /// </summary>
        public Postprocessor() : base(Language.Instance)
        {
        }

        /// <summary>
        /// Runs the postprocessor on the list of tokens.
        /// This method splits compound identifiers (like "Namespace.Class.Method") and removes whitespace and comments.
        /// </summary>
        /// <param name="tokens">The list of tokens.</param>
        /// <returns>The processed list of tokens.</returns>
        public override IList<Token> Run(string source, IList<Token> tokens)
        {
            tokens = Clean(tokens);

            // Splitting compound identifiers (like "Namespace.Class.Method").

            var count = tokens.Count;

            for (int i = 0; i < count; i++)
            {
                var token = tokens.ElementAt(i);

                if (token.Type != TokenType.Identifier)
                    continue;

                var text = token.Text;

                // If compound identifier found.
                if (text.Contains(Punctuations.Dot))
                {
                    var split = text.Split(Punctuations.Dot, StringSplitOptions.None);

                    var newTokens = new List<Token>();
                    var startIndex = token.StartIndex;

                    var s = 0;
                    foreach (var subIdentifier in split)
                    {
                        if (subIdentifier != string.Empty)
                        {
                            var newToken = new Token() 
                            {
                                Type = TokenType.Identifier,
                                Source = source,
                                StartIndex = startIndex,
                                NextIndex = startIndex + subIdentifier.Length,
                            };

                            if (Language.MatchKeyword(subIdentifier, out var newTokenType))
                                newToken.ChangeType(newTokenType!.Value);

                            if (Language.MatchNumber(subIdentifier, out newTokenType))
                                newToken.ChangeType(newTokenType!.Value);

                            newTokens.Add(newToken);
                            startIndex += subIdentifier.Length;
                        }

                        if (s == split.Length - 1)
                            break; // Last token: don't append a dot.

                        newTokens.Add(new Token() 
                        {
                            Type = TokenType.Dot,
                            Source = source,
                            StartIndex = startIndex,
                            NextIndex = startIndex + 1,
                        });

                        startIndex++;
                        s++;
                    }

                    // Removing original identifier.
                    tokens.RemoveAt(i);

                    // Inserting found tokens at the right positions.
                    var n = 0;
                    foreach (var newToken in newTokens)
                        tokens.Insert(i + n++, newToken);

                    // Keeping track of inserted tokens, minus the removed identifier.
                    count += newTokens.Count - 1;

                    // Skipping inserted tokens.
                    i += n - 1;
                }
            }

            return tokens;
        }
    }
}
