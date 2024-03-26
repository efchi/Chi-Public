using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class DefinitionNode : IExpressionNode
    {
        public readonly int Symbol;
        public readonly IList<int>? Parameters;
        public readonly IExpressionNode? Expression;

        public DefinitionNode(int symbol, IList<int>? parameters, IExpressionNode? expression) =>
            (Symbol, Parameters, Expression) = (symbol, parameters, expression);
    }
}
