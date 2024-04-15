using Chi.Parsing.Data;
using Chi.Parsing.Syntax.Abstract;
using Chi.Runtime.Values;
using Chi.Semanting;

namespace Chi.Parsing.Syntax
{
    /// <summary>
    /// Represents a module in the source AST.
    /// A module is a list of insutrctions with a name and a lexical scope. 
    /// </summary>
    public class ModuleNode : IScopedNode
    {
        public LexicalScope Scope { get; set; }

        public readonly IdentifierNode Name;
        public readonly IList<ISyntaxNode> Instructions;

        public ModuleNode(IdentifierNode name, IList<ISyntaxNode> instructions) =>
            (Name, Instructions) = (name, instructions);

        #region Analyzer Fields and Methods

        // Singleton definition value.
        public Module Value { get; internal set; }

        // Static definition signature.
        public Symbol Signature { get; internal set; }

        #endregion
    }
}
