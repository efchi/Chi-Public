using Chi.Infra;
using Chi.Parsing.Data;

namespace Chi.Semanting
{
    /// <summary>
    /// Represents a lexical scope.
    /// </summary>
    public class LexicalScope : List<Declaration>
    {
        public readonly Symbol Alias;                           // Friendly name for this scope: <global>/module/.../A(x).
        public readonly LexicalScopeKind Kind;                  // Singleton (static: global, module) or Instance (local).
        public readonly LexicalScope? Parent;                   // Parent scope.
        public int? GlobalFrameStart;                           // Start of this scope's definitions into the global frame (only for Singletons).

        // Ctor for global scope (called by Context).
        // Sets the global scope start at index 0. This is necessary to correctly address primitives when the interpreter is running for the first time.
        // Note how the global scope is a Singleton scope with no parent.
        public LexicalScope(Symbol alias, LexicalScopeKind kind) =>
            (Alias, Kind, Parent, GlobalFrameStart) = (alias, kind, default, 0);

        // Ctor for module and local scopes (called by Analyzer).
        public LexicalScope(Symbol alias, LexicalScopeKind kind, LexicalScope? parent) =>
            (Alias, Kind, Parent) = (alias, kind, parent);

        public int Append(Declaration declaration)
        {
            if (Lookup(declaration.Symbol, out var _, out var _))
                // This could be reached while merging global scope with current program scope.
                throw new AlreadyDeclaredException($"LexicalScope :: Symbol {declaration.Symbol.String} is already declared in the scope {Alias.String}.");

            Add(declaration);
            return Count - 1;
        }

        // Check if a Symbol is declared in this scope.
        public bool Lookup(Symbol symbol, out Declaration? declaration, out int? index)
        {
            // Important note: FindIndex vs FindLastIndex (first declaration vs last declaration).
            // - We use FindLastIndex in REPL mode to allow redefinitions.
            // - We use FindIndex in file mode to forbid redefinitions.

            // Explanation:
            // When redefining a def (or var), the new declaration is appended into the <program> scope and in the
            // global scope extension, that will be merged into the global scope at the end of the static analysis.
            // This means that, when using FindIndex, the new declaration is available into its program using the usual
            // scoping rules (shadowing), but wont be reachable by subsequent lookups from next programs:
            // the older declaration will prevail. This is a strict and coherent behavior.
            // Instead, when using FindLastIndex, the most recent declaration will also be reachable by subsequent lookups,
            // enabling a more permissive behavior useful when working in REPL mode.

            index = Context.IsREPL ? 
                FindLastIndex(i => Symbol.Equal(i.Symbol, symbol)) :
                FindIndex(i => Symbol.Equal(i.Symbol, symbol));

            declaration = (index >= 0) ? this[index.Value] : default;
            return declaration != default;
        }

        // Check if a Symbol is declared in this scope or in an enclosing Singleton scope.
        public bool RecursiveLookup(Symbol symbol, out Declaration? declaration, out LexicalScope? targetScope, out int? scopeIndex)
        {
            if (Lookup(symbol, out declaration, out scopeIndex))
            {
                targetScope = this;
                return true;
            }

            if (Parent != default)
            {
                // Parent lookup is always performed skipping Instance scopes (we are only interested in locals and globals). 
                // Eventually global scope will be reached, which is a Singleton scope.
                var ancestor = Parent;
                while (ancestor!.Kind == LexicalScopeKind.Instance)
                    ancestor = ancestor.Parent;

                return ancestor.RecursiveLookup(symbol, out declaration, out targetScope, out scopeIndex);
            }

            scopeIndex = default;
            targetScope = default;
            return false;
        }

        public override string ToString() =>
            $"(LexicalScope {Kind} {Alias.String} in {Parent?.Alias.String ?? "<default>"})";
    }
}
