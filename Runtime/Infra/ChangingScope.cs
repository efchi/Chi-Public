using Chi.Runtime.Data.Abstract;

namespace Chi.Runtime.Infra
{
    // A concrete Scope implementation as a stack.
    // Local and Dynamic scopes are ChangingScopes.
    public class ChangingScope : Stack<(string name, IValueNode value)>, IScope
    {
        public void Bind(string name, IValueNode value) =>
            Push((name, value));

        public bool Find(string name, out IValueNode? value)
        {
            foreach (var binding in this)
            {
                if (binding.name == name)
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
