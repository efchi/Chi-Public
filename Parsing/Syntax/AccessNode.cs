using Chi.Infra;
using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class AccessNode : IExpressionNode
    {
        public readonly IExpressionNode Expression;
        public readonly Symbol Member;

        public AccessNode(IExpressionNode expression, Symbol member) =>
            (Expression, Member) = (expression, member);
    }
}
