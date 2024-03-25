using Chi.Parsing.Syntax;
using Chi.Parsing.Syntax.Abstract;
using Chi.Runtime.Data;
using Chi.Runtime.Data.Abstract;

namespace Chi.Runtime.Abstract
{
    public interface IInterpreter
    {
        Program Run(ProgramNode program);
        void Reset();

        internal IValueNode Eval(ISyntaxNode? node);
    }
}
