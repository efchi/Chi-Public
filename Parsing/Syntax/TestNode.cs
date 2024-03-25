using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class TestNode : ISyntaxNode
    {
        public readonly string Name;
        public readonly ProgramNode Program;

        public TestNode(string name, ProgramNode program) =>
            (Name, Program) = (name, program);
    }
}
