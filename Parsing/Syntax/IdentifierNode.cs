using Chi.Infra;
using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class IdentifierNode : LiteralNode<Symbol>
    {
        public IdentifierNode(Symbol symbol) : base(symbol)
        {
        }
    }
}
