namespace Chi.Lexing
{
    /// <summary>
    /// Describes the token kinds (groups) of our language.
    /// </summary>
    public enum TokenKind
    {
        EOF = 0,
        Whitespace,
        Comment,
        Punctuation,
        Word,
        Keyword,
        Number,
        Identifier,
        Unrecognized
    }
}
