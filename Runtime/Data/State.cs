using Chi.Infra;
using Chi.Runtime.Data.Abstract;

namespace Chi.Runtime.Data
{
    // Interpreter.EvalSet() assigns values both in Scopes (root identifier) and States (accessed/indexed subsequent identifiers).
    // Since a State is basically a Dictionary, we turn it into a FixedScope (that is an IScope) so that EvalSet can
    // conveniently assign values to both Scopes and States, working on IScope.
    // See the implementation of EvalSet() for more details.
    public class State : DictionaryScope, IValueNode
    {
    }
}
