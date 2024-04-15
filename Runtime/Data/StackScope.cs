using Chi.Parsing.Data;
using Chi.Runtime.Values.Abstract;

namespace Chi.Runtime.Data
{
    /// <summary>
    /// A concrete dynamic scope implementation as a dictionary.
    /// Previously used for local scope, now it's only used for dynamic scope.
    /// Keeping int as key (not Symbol) for performance reasons.
    /// </summary>
    public class StackScope : Stack<(int symbolCode, IValueNode value)>, IDynamicScope
    {
        public void Bind(Symbol symbol, IValueNode value) =>
            Push((symbol.Code, value));

        public bool Find(Symbol symbol, out IValueNode? value)
        {
            foreach (var binding in this)
            {
                if (binding.symbolCode == symbol.Code)
                {
                    value = binding.value;
                    return true;
                }
            }
            value = default;
            return false;
        }
    }
}
