using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class ConditionalNode : IExpressionNode
    {
        public readonly IExpressionNode If;
        public readonly IExpressionNode Then;
        public readonly IExpressionNode? Else;

        public ConditionalNode(IExpressionNode @if, IExpressionNode then, IExpressionNode? @else) =>
            (If, Then, Else) = (@if, then, @else);
    }
}
