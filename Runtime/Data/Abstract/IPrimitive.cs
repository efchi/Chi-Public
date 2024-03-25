using Chi.Parsing.Syntax.Abstract;
using Chi.Runtime.Abstract;

namespace Chi.Runtime.Data.Abstract
{
    public interface IPrimitive : IValueNode
    {
        public string Signature { get; }
        public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments);
    }
}
