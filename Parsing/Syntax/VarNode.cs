using Chi.Infra;
using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    internal class VarNode : IExpressionNode
    {
        public readonly Symbol Name;
        public readonly IExpressionNode? RValue;

        public VarNode(Symbol name, IExpressionNode? rValue) =>
            (Name, RValue) = (name, rValue);
    }
}
