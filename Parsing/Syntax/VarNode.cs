using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    internal class VarNode : IExpressionNode
    {
        public readonly string Name;
        public readonly IExpressionNode? RValue;

        public VarNode(string name, IExpressionNode? rValue) =>
            (Name, RValue) = (name, rValue);
    }
}
