using Chi.Parsing.Syntax.Abstract;
using Chi.Runtime.Abstract;

namespace Chi.Runtime.Values.Abstract
{
    /// <summary>
    /// Primitive definition abstraction, used to implement built-in functions.
    /// </summary>
    public interface IPrimitive : IValueNode,
        ISyntaxNode // This is necessary to embed primitives into a MemoryFrame (usually used for global scope).
    {
        public string Signature { get; }
        public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments);
    }
}
