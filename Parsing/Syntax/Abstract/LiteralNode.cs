namespace Chi.Parsing.Syntax.Abstract
{
    public abstract class LiteralNode<T> : IExpressionNode
    {
        public readonly T Value;

        public LiteralNode(T value) =>
            Value = value;
    }
}
