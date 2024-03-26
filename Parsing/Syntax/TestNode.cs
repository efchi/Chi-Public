using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class TestNode : ISyntaxNode
    {
        public readonly int Symbol;
        public readonly ProgramNode Program;

        public TestNode(int symbol, ProgramNode program) =>
            (Symbol, Program) = (symbol, program);
    }
}
