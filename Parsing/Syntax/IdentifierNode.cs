using Chi.Parsing.Data;
using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public enum IdentifierLocation
    {
        // The Unresolved value is used to indicate that the identifier has not been
        // resolved yet by the Analyzer. In this case, Index will be null.
        Unresolved = 0,

        Local,      // Identifier was resolved in the local scope (local declaration or parameter).
        Global,     // Identifier was resolved in the global scope (global declaration).
        Dynamic     // Identifier was *not* resolved in local or global scope (dynamic scope lookup).
    }

    public class IdentifierNode : LiteralNode<Symbol>
    {
        public IdentifierNode(Symbol symbol) : base(symbol)
        {
        }

        #region Analyzer Fields and Methods

        internal IdentifierLocation Location { get; private set; }
        internal int? Index { get; private set; }

        internal void ResolveToLocal(int index) =>
            (Location, Index) = (IdentifierLocation.Local, index);

        internal void ResolveToGlobal(int index) =>
            (Location, Index) = (IdentifierLocation.Global, index);

        internal void ResolveToDynamic() =>
            (Location, Index) = (IdentifierLocation.Dynamic, default);

        #endregion
    }
}
