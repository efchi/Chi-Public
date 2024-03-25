using Chi.Parsing.Syntax;
using Chi.Runtime.Data.Abstract;

namespace Chi.Runtime.Data
{
    public class Definition : IValueNode
    {
        public readonly DefinitionNode Node;

        public Definition(DefinitionNode def) =>
            Node = def;
    }
}
