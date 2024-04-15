using Chi.Parsing.Data;
using Chi.Parsing.Syntax.Abstract;

namespace Chi.Semanting
{
    /// <summary>
    /// Represents a declaration in a lexical scope.
    /// </summary>
    public class Declaration
    {
        public readonly DeclarationKind Kind;
        public readonly Symbol Symbol;
        public readonly LexicalScope Scope;
        public readonly ISyntaxNode Node;

        public Declaration(DeclarationKind kind, Symbol symbol, LexicalScope scope, ISyntaxNode node) =>
            (Kind, Symbol, Scope, Node) = (kind, symbol, scope, node);

        public override string ToString() =>
            $"(Declaration {Kind} {Symbol.String} in {Scope.Alias.String})";
    }
}
