using Chi.Runtime.Data.Abstract;

namespace Chi.Runtime.Infra
{
    // A concrete Scope implementation as a dictionary.
    // Global scope is a FixedScope.
    public class FixedScope : Dictionary<int, IValueNode>, IScope
    {
        public void Bind(int symbol, IValueNode value) =>
            this[symbol] = value;

        public bool Find(int symbol, out IValueNode? value) =>
            TryGetValue(symbol, out value);
    }
}
