namespace Chi.Parsing.Syntax.Abstract
{
    /// <summary>
    /// Represents a literal value of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class LiteralNode<T> : IExpressionNode
    {
        public readonly T Value;

        public LiteralNode(T value) =>
            Value = value;
    }
}
