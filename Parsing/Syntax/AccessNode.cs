using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class AccessNode : IExpressionNode
    {
        public readonly IExpressionNode Expression;
        public readonly int Symbol;

        public AccessNode(IExpressionNode expression, int symbol) =>
            (Expression, Symbol) = (expression, symbol);
    }
}
