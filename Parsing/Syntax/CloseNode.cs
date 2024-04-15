using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class CloseNode : IExpressionNode
    {
        public readonly IExpressionNode Expression;
        public readonly IList<(IdentifierNode name, IExpressionNode expression)>? Bindings;

        public CloseNode(IExpressionNode expression, IList<(IdentifierNode name, IExpressionNode expression)>? bindings) =>
            (Expression, Bindings) = (expression, bindings);
    }
}
