using Chi.Parsing.Data;
using Chi.Runtime.Values.Abstract;

namespace Chi.Runtime.Data
{
    /// <summary>
    /// Abstraction over dynamic scopes (dictionary or stack).
    /// </summary>
    public interface IDynamicScope
    {
        void Bind(Symbol symbol, IValueNode value);
        bool Find(Symbol symbol, out IValueNode? value);
        void Clear();
    }
}
