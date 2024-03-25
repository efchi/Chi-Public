using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class CastNode : IExpressionNode
    {
        public readonly IExpressionNode Expression;
        public readonly IExpressionNode Type;

        public CastNode(IExpressionNode expression, IExpressionNode type) =>
            (Expression, Type) = (expression, type);
    }
}
