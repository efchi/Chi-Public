using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    internal class SetNode : IExpressionNode
    {
        public readonly IExpressionNode LValue;
        public readonly IExpressionNode RValue;

        public SetNode(IExpressionNode lValue, IExpressionNode rValue) =>
            (LValue, RValue) = (lValue, rValue);
    }
}
