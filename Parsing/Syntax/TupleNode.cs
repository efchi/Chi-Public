using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class TupleNode : IExpressionNode
    {
        public readonly IList<IExpressionNode>? Items;

        public TupleNode(IList<IExpressionNode>? items) =>
            Items = items;
    }
}
