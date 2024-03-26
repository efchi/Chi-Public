using Chi.Parsing.Syntax;
using Chi.Parsing.Syntax.Abstract;
using Chi.Runtime.Data;
using Chi.Runtime.Data.Abstract;
using Chi.Shared;

namespace Chi.Runtime.Abstract
{
    public interface IInterpreter
    {
        Program Run(ProgramNode program);
        void Reset();
        SymbolTable Symbols { get; }
        int OkSymbol { get; }
        int KoSymbol { get; }

        internal IValueNode Eval(ISyntaxNode? node);
    }
}
