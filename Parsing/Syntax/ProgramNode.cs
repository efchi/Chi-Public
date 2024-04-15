using Chi.Infra;
using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    /// <summary>
    /// Represent a program node, that is, the root of the source AST.
    /// A program is an implicit module named with the generic identifier &lt;program&gt;.
    /// </summary>
    public class ProgramNode : ModuleNode
    {
        // Indicates whether the program has been analyzed.
        public bool Analyzed { get; internal set; }

        public ProgramNode(IList<ISyntaxNode> instructions) 
            : base(Context.IdentifierProgram, instructions)
        { 
        }
    }
}