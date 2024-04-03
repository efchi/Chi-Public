using Chi.Infra;
using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class TestNode : ISyntaxNode
    {
        public readonly Symbol Name;
        public readonly IList<ISyntaxNode> Instructions;

        public TestNode(Symbol name, IList<ISyntaxNode> instructions) =>
            (Name, Instructions) = (name, instructions);
    }
}
