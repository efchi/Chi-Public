using Chi.Runtime.Data.Abstract;

namespace Chi.Runtime.Infra
{
    // A concrete Scope implementation as a stack.
    // Local and Dynamic scopes are ChangingScopes.
    public class ChangingScope : Stack<(int symbol, IValueNode value)>, IScope
    {
        public void Bind(int symbol, IValueNode value) =>
            Push((symbol, value));

        public bool Find(int symbol, out IValueNode? value)
        {
            foreach (var binding in this)
            {
                if (binding.symbol == symbol)
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
