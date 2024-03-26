using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class IntegerNode : LiteralNode<int>
    {
        public IntegerNode(int value) : base(value)
        {
        }
    }
}
