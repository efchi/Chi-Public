using Chi.Runtime.Data.Abstract;

namespace Chi.Runtime.Infra
{
    // A Scope is a collection of (name, value) bindings.
    // IScope is a Scope abstraction: some scopes are implemented as
    // dictionaries (eg. Global), others as stacks (eg. Local, Dynamic).
    public interface IScope
    {
        void Bind(int symbol, IValueNode value);
        bool Find(int symbol, out IValueNode? value);
        void Clear();
    }
}
