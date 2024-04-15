namespace Chi.Lexing
{
    /// <summary>
    /// Describes the token types of our language.
    /// </summary>
    public enum TokenType
    {
        // EOF Token.
        EOF = 0,

        // Whitespace Token.
        Whitespace,

        // Comment Token.
        Comment,

        // Punctuation Tokens.
        BraceOpen,
        BraceClose,
        ParenOpen,
        ParenClose,
        BrackOpen,
        BrackClose,
        AngularOpen,
        AngularClose,
        Dot,
        Colon,
        Comma,
        Semicolon,
        Equal,
        Underscore,
        Dollar,

        // Word Token (should not be used un practice).
        Word,

        // Keyword Tokens.
        Nil,
        Def,
        If,
        Then,
        Else,
        Close,
        With,
        Var,
        Set,
        Test,
        Module,

        // Number Tokens.
        Integer,
        Decimal,

        // Identifier Token.
        Identifier,

        // Unrecognized Token.
        Unrecognized
    }
}
