using Chi.Infra;
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
        SymbolTable Symbols { get; }
        Symbol OkSymbol { get; }
        Symbol KoSymbol { get; }

        internal IValueNode Eval(ISyntaxNode? node);
    }
}
