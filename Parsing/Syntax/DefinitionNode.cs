using Chi.Infra;
using Chi.Parsing.Data;
using Chi.Parsing.Syntax.Abstract;
using Chi.Runtime.Data;
using Chi.Runtime.Values;
using Chi.Semanting;

namespace Chi.Parsing.Syntax
{
    public class DefinitionNode : IExpressionNode, IScopedNode
    {
        public readonly Symbol Name;
        public readonly IList<Symbol>? Parameters;
        public readonly IExpressionNode? Expression;

        public DefinitionNode(Symbol name, IList<Symbol>? parameters, IExpressionNode? expression)
        {
            Name = name;
            Parameters = parameters;
            Expression = expression;
        }

        #region Analyzer Fields and Methods

        // Singleton definition value.
        public Definition Value { get; internal set; }

        // Static definition signature.
        public Symbol Signature { get; internal set; }

        // Lexical scope of this definition.
        public LexicalScope Scope { get; set; }

        // Activation frame prototype: array of local values.
        // The frame prototype is cloned to create a new frame on each call (see MemoryFrame ctor).
        internal MemoryFrame? FramePrototype { get; private set; }

        internal void BuildFramePrototype() =>
            FramePrototype = new MemoryFrame(Scope);

        #endregion

        #region Private Helpers

        public string GetDefinitionSignature() =>
            $"{Name.String}({Parameters?.Count ?? 0})";

        #endregion
    }
}