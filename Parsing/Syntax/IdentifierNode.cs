using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class IdentifierNode : LiteralNode<int>
    {
        public IdentifierNode(int symbol) : base(symbol)
        {
        }
    }
}
