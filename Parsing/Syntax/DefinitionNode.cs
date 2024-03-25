using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class DefinitionNode : IExpressionNode
    {
        public readonly string Name;
        public readonly IList<string>? Parameters;
        public readonly IExpressionNode? Expression;

        public DefinitionNode(string name, IList<string>? parameters, IExpressionNode? expression) =>
            (Name, Parameters, Expression) = (name, parameters, expression);
    }
}
