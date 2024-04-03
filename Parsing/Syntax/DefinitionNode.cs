using Chi.Infra;
using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class DefinitionNode : IExpressionNode
    {
        public readonly Symbol Name;
        public readonly IList<Symbol>? Parameters;
        public readonly IExpressionNode? Expression;

        public DefinitionNode(Symbol name, IList<Symbol>? parameters, IExpressionNode? expression) =>
            (Name, Parameters, Expression) = (name, parameters, expression);
    }
}
