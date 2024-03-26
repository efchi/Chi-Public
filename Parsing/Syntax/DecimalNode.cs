using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class DecimalNode : LiteralNode<decimal>
    {
        public DecimalNode(decimal value) : base(value)
        {
        }
    }
}
