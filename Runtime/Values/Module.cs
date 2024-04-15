using Chi.Parsing.Syntax;
using Chi.Runtime.Values.Abstract;

namespace Chi.Runtime.Values
{
    public class Module : IValueNode
    {
        internal readonly ModuleNode Node;

        internal Module(ModuleNode moduleNode) =>
            Node = moduleNode;
    }
}
