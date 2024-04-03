using Chi.Infra;
using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class CloseNode : IExpressionNode
    {
        public readonly IExpressionNode Expression;
        public readonly IList<(Symbol name, IExpressionNode expression)>? Bindings;

        public CloseNode(IExpressionNode expression, IList<(Symbol name, IExpressionNode expression)>? bindings) =>
            (Expression, Bindings) = (expression, bindings);
    }
}
