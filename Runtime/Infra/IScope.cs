using Chi.Runtime.Data.Abstract;

namespace Chi.Runtime.Infra
{
    // A Scope is a collection of (name, value) bindings.
    // IScope is a Scope abstraction: some scopes are implemented as
    // dictionaries (eg. Global), others as stacks (eg. Local, Dynamic).
    public interface IScope
    {
        void Bind(string name, IValueNode value);
        bool Find(string name, out IValueNode? value);
        void Clear();
    }
}
