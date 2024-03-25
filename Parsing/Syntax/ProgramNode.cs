using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class ProgramNode : ISyntaxNode
    {
        public readonly IList<ISyntaxNode> Instructions;

        public ProgramNode(IList<ISyntaxNode> instructions) =>
            Instructions = instructions;
    }
}
