using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    /// <summary>
    /// Represents a test node, that is, a module whose instructions 
    /// return OK / KO for language testing purposes.
    /// </summary>
    public class TestNode : ModuleNode
    {
        public TestNode(IdentifierNode name, IList<ISyntaxNode> instructions) 
            : base(name, instructions) 
        {
        }
    }
}