using Chi.Runtime.Data.Abstract;

namespace Chi.Infra
{
    // A Scope is a collection of (name, value) bindings.
    // IScope is a Scope abstraction: some scopes are implemented as
    // dictionaries (eg. Global), others as stacks (eg. Local, Dynamic).
    public interface IRuntimeScope
    {
        void Bind(Symbol symbol, IValueNode value);
        bool Find(Symbol symbol, out IValueNode? value);
        void Clear();
    }
}
