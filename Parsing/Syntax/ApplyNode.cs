using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class ApplyNode : IExpressionNode
    {
        public readonly IExpressionNode Expression;
        public readonly IList<IExpressionNode>? Arguments;

        public ApplyNode(IExpressionNode expression, IList<IExpressionNode>? arguments) =>
            (Expression, Arguments) = (expression, arguments);
    }
}
