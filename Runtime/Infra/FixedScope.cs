using Chi.Runtime.Data.Abstract;

namespace Chi.Runtime.Infra
{
    // A concrete Scope implementation as a dictionary.
    // Global scope is a FixedScope.
    public class FixedScope : Dictionary<string, IValueNode>, IScope
    {
        public void Bind(string name, IValueNode value) =>
            this[name] = value;

        public bool Find(string name, out IValueNode? value) =>
            TryGetValue(name, out value);
    }
}
