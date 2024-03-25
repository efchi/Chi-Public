using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class SequenceNode : IExpressionNode
    {
        public readonly IExpressionNode Expression;
        public readonly IExpressionNode Continuation;

        public SequenceNode(IExpressionNode expression, IExpressionNode continuation) =>
            (Expression, Continuation) = (expression, continuation);
    }
}
