using Chi.Parsing.Syntax;
using Chi.Runtime.Values.Abstract;

namespace Chi.Runtime.Values
{
    public class Definition : IValueNode
    {
        internal readonly DefinitionNode Node;

        internal Definition(DefinitionNode definitionNode) =>
            Node = definitionNode;
    }
}
