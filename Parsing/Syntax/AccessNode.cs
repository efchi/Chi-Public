using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class AccessNode : IExpressionNode
    {
        public readonly IExpressionNode Expression;
        public readonly IdentifierNode Member;

        public AccessNode(IExpressionNode expression, IdentifierNode member) =>
            (Expression, Member) = (expression, member);
    }
}
