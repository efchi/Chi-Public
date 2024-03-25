using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class IndexNode : IExpressionNode
    {
        public readonly IExpressionNode Expression;
        public readonly IExpressionNode Accessor;

        public IndexNode(IExpressionNode expression, IExpressionNode accessor) =>
            (Expression, Accessor) = (expression, accessor);
    }
}
