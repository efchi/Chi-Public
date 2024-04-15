using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    internal class VarNode : IExpressionNode
    {
        public readonly IdentifierNode Name;
        public readonly IExpressionNode? RValue;

        public VarNode(IdentifierNode name, IExpressionNode? rValue) =>
            (Name, RValue) = (name, rValue);
    }
}
