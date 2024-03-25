using System.Text.RegularExpressions;
using Chi.Lexing.Abstract;

namespace Chi.Lexing
{
    /// <summary>
    /// Implements a language description used for lexing and parsing. 
    /// This is our language's concrete specification, indicating what is a keyword, 
    /// number, punctuation symbol, comment and so on.
    /// </summary>
    public class Language : ILanguage<Token, TokenType>
    {
        #region Singleton & Constants

        public static readonly Language Instance = new();

        static readonly Regex IntegerRegex = new(@"^[-+]?[0-9]+$", RegexOptions.Compiled);
        static readonly Regex DecimalRegex = new(@"^[-+]?[0-9]+\.[0-9]+$", RegexOptions.Compiled);

        #endregion

        #region ILanguage<TokenType> Implementation

        public TokenType EOF => TokenType.EOF;
        public TokenType Whitespace => TokenType.Whitespace;
        public TokenType Comment => TokenType.Comment;
        public TokenType Word => TokenType.Word;
        public TokenType Unrecognized => TokenType.Unrecognized;
        
        public bool IsEOF(TokenType tokenType) => tokenType == EOF;
        public bool IsWhitespace(TokenType tokenType) => tokenType == Whitespace;
        public bool IsComment(TokenType tokenType) => tokenType == Comment;
        public bool IsCommentChar(char c) => c == Punctuations.Hash;
        
        public bool IsWordChar(char c) => char.IsLetter(c) || char.IsDigit(c)
            || c == '-' || c == '+' || c == '*' || c == '/' || c == '!' || c == '?'
            // Dot is allowed in the middle of a word (not the first character) to match floating point numbers and
            // compound identifiers (like Namespace.Class.Method). Such identifiers are then splitted by the post-processor.
            || c == Punctuations.Dot;

        public bool MatchPunctuation(char current, out TokenType? tokenType)
        {
            tokenType = current switch
            {
                Punctuations.BraceOpen => TokenType.BraceOpen,
                Punctuations.BraceClose => TokenType.BraceClose,
                Punctuations.ParenOpen => TokenType.ParenOpen,
                Punctuations.ParenClose => TokenType.ParenClose,
                Punctuations.BrackOpen => TokenType.BrackOpen,
                Punctuations.BrackClose => TokenType.BrackClose,
                Punctuations.AngularOpen => TokenType.AngularOpen,
                Punctuations.AngularClose => TokenType.AngularClose,
                Punctuations.Dot => TokenType.Dot,
                Punctuations.Colon => TokenType.Colon,
                Punctuations.Comma => TokenType.Comma,
                Punctuations.Semicolon => TokenType.Semicolon,
                Punctuations.Equal => TokenType.Equal,
                Punctuations.Underscore => TokenType.Underscore,
                Punctuations.Dollar => TokenType.Dollar,
                _ => default(TokenType?),
            };

            return tokenType != default;
        }

        public bool MatchKeyword(string text, out TokenType? newTokenType)
        {
            newTokenType = text switch
            {
                Keywords.Nil => TokenType.Nil,
                Keywords.Def => TokenType.Def,
                Keywords.If => TokenType.If,
                Keywords.Then => TokenType.Then,
                Keywords.Else => TokenType.Else,
                Keywords.Close => TokenType.Close,
                Keywords.With => TokenType.With,
                Keywords.Var => TokenType.Var,
                Keywords.Set => TokenType.Set,
                Keywords.Test => TokenType.Test,
                _ => default(TokenType?)
            };

            return newTokenType != default;
        }

        public bool MatchNumber(string text, out TokenType? newTokenType)
        {
            newTokenType = default;

            if (IntegerRegex.IsMatch(text))
                newTokenType = TokenType.Integer;

            else if (DecimalRegex.IsMatch(text))
                newTokenType = TokenType.Decimal;

            return newTokenType != default;
        }

        public bool MatchIdentifier(string text, out TokenType? newTokenType)
        {
            // Match anything that is not a keyword or number (see Abstract.Lexer).
            newTokenType = TokenType.Identifier;
            return true;
        }

        public Enum GetKind(TokenType tokenType) => tokenType switch
        {
            TokenType.EOF => TokenKind.EOF,
            TokenType.Whitespace => TokenKind.Whitespace,
            TokenType.Comment => TokenKind.Comment,
            TokenType.BraceOpen => TokenKind.Punctuation,
            TokenType.BraceClose => TokenKind.Punctuation,
            TokenType.ParenOpen => TokenKind.Punctuation,
            TokenType.ParenClose => TokenKind.Punctuation,
            TokenType.BrackOpen => TokenKind.Punctuation,
            TokenType.BrackClose => TokenKind.Punctuation,
            TokenType.AngularOpen => TokenKind.Punctuation,
            TokenType.AngularClose => TokenKind.Punctuation,
            TokenType.Dot => TokenKind.Punctuation,
            TokenType.Colon => TokenKind.Punctuation,
            TokenType.Comma => TokenKind.Punctuation,
            TokenType.Semicolon => TokenKind.Punctuation,
            TokenType.Equal => TokenKind.Punctuation,
            TokenType.Underscore => TokenKind.Punctuation,
            TokenType.Dollar => TokenKind.Punctuation,
            TokenType.Word => TokenKind.Word,
            TokenType.Nil => TokenKind.Keyword,
            TokenType.Def => TokenKind.Keyword,
            TokenType.If => TokenKind.Keyword,
            TokenType.Then => TokenKind.Keyword,
            TokenType.Else => TokenKind.Keyword,
            TokenType.Close => TokenKind.Keyword,
            TokenType.With => TokenKind.Keyword,
            TokenType.Var => TokenKind.Keyword,
            TokenType.Set => TokenKind.Keyword,
            TokenType.Test => TokenKind.Keyword,
            TokenType.Integer => TokenKind.Number,
            TokenType.Decimal => TokenKind.Number,
            TokenType.Identifier => TokenKind.Identifier,
            TokenType.Unrecognized => TokenKind.Unrecognized,
            _ => throw new NotSupportedException()
        };

        #endregion
    }
}
