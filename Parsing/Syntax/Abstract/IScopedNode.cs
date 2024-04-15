using Chi.Semanting;

namespace Chi.Parsing.Syntax.Abstract
{
    /// <summary>
    /// Represents a syntax node bound to a lexical scope (program, module, def, etc).
    /// </summary>
    public interface IScopedNode : ISyntaxNode
    {
        LexicalScope Scope { get; set; }
    }
}
