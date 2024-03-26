using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    internal class VarNode : IExpressionNode
    {
        public readonly int Symbol;
        public readonly IExpressionNode? RValue;

        public VarNode(int symbol, IExpressionNode? rValue) =>
            (Symbol, RValue) = (symbol, rValue);
    }
}
