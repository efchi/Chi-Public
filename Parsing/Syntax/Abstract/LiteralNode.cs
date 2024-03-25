namespace Chi.Parsing.Syntax.Abstract
{
    public abstract class LiteralNode : IExpressionNode
    {
        public readonly string Value;

        public LiteralNode(string value) =>
            Value = value;
    }
}
