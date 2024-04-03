using Chi.Runtime.Data.Abstract;

namespace Chi.Infra
{
    // A concrete scope implementation as a dictionary.
    // Used for global scope (more convenient redefinition logic; better performance) and states (State is a scope: see EvalSet behavior).
    // Keeping int as key and not Symbol for performance reasons.
    public class DictionaryScope : Dictionary<int, IValueNode>, IRuntimeScope
    {
        public void Bind(Symbol symbol, IValueNode value) =>
            this[symbol.Code] = value;

        public bool Find(Symbol symbol, out IValueNode? value) =>
            TryGetValue(symbol.Code, out value);
    }
}
