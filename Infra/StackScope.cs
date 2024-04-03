using Chi.Runtime.Data.Abstract;

namespace Chi.Infra
{
    // A concrete scope implementation as a stack.
    // Used for dynamic scope and local scopes.
    // Keeping int as key and not Symbol for performance reasons.
    public class StackScope : Stack<(int symbolCode, IValueNode value)>, IRuntimeScope
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
