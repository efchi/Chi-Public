using Chi.Parsing.Syntax;
using Chi.Parsing.Syntax.Abstract;
using Chi.Runtime.Values;
using Chi.Runtime.Values.Abstract;

namespace Chi.Runtime.Abstract
{
    /// <summary>
    /// Interpreter abtraction, used to test different interpreter implementations.
    /// </summary>
    public interface IInterpreter
    {
        Program Run(ProgramNode program);
        internal IValueNode Eval(ISyntaxNode? node);
    }
}
