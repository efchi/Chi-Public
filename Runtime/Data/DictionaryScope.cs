using Chi.Parsing.Data;
using Chi.Runtime.Values.Abstract;

namespace Chi.Runtime.Data
{
    /// <summary>
    /// A concrete dynamic scope implementation as a dictionary.
    /// Previously used for global scope, now it's only used for states.
    /// Keeping int as key (not Symbol) for performance reasons.
    /// </summary>
    public class DictionaryScope : Dictionary<int, IValueNode>, IDynamicScope
    {
        public void Bind(Symbol symbol, IValueNode value) =>
            this[symbol.Code] = value;

        public bool Find(Symbol symbol, out IValueNode? value) =>
            TryGetValue(symbol.Code, out value);
    }
}
