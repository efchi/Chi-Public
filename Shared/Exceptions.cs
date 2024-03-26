using Chi.Lexing.Abstract;

namespace Chi
{
    class UsageException : Exception
    {
        public UsageException(string message) : base(message)
        {
        }
    }

    class UnreachableException : Exception
    {
        public UnreachableException()
        {
        }

        public UnreachableException(string message) : base(message)
        {
        }
    }

    abstract class SyntaxException : Exception
    {
        public SyntaxException(string message) : base(message)
        {
        }
    }

    class MatchException<Y> : SyntaxException where Y : struct, Enum
    {
        public MatchException(string message, IToken<Y> current)
            : base($"Match: {message}. Current is {current.ToString(verbose: true)}.")
        {
        }
    }

    class PredictException<Y> : SyntaxException where Y : struct, Enum
    {
        public PredictException(string message, IToken<Y> current)
            : base($"Predict: {message}. Current is {current.ToString(verbose: true)}.")
        {
        }
    }

    class LiteralException : SyntaxException
    {
        public LiteralException(Enum expected, string text)
            : base($"Invalid Literal ({expected}). Matched text is '{text}'.")
        {
        }
    }

    class RuntimeException : Exception
    {
        public RuntimeException(string message) : base(message)
        {
        }
    }

    class PrimitiveException : RuntimeException
    {
        public PrimitiveException(string message) : base(message)
        {
        }
    }
}
