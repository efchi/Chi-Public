using Chi.Parsing.Data;
using Chi.Parsing.Syntax.Abstract;
using Chi.Runtime.Values;

namespace Chi.Parsing.Syntax
{
    public enum ApplyKind
    {
        // The Unresolved value is used to indicate that the application has not been
        // resolved yet by the Analyzer. In this case, Index will be null.
        Unresolved = 0,

        Parametric,    // Expression is an IdentifierNode resolved as a local parameter.
        Local,         // Expression is an IdentifierNode resolved in the local scope (local def or recursive def name).
        Global,        // Expression is an IdentifierNode resolved in the global scope (global def).
        Dynamic        // Expression is an IdentifierNode *not* resolved in local or global scope ("open call": not yet supported). 
    }

    public class ApplyNode : IExpressionNode
    {
        public readonly IExpressionNode Expression;
        public readonly IList<IExpressionNode>? Arguments;

        public ApplyNode(IExpressionNode expression, IList<IExpressionNode>? arguments) =>
            (Expression, Arguments) = (expression, arguments);

        #region Analyzer Fields and Methods

        // Dynamic apply signature (see Analyzer).
        internal Symbol Signature { get; set; }

        internal ApplyKind Location { get; private set; }
        internal int? Index { get; private set; }

        internal void ResolveToParametric(int index) =>
            (Location, Index) = (ApplyKind.Parametric, index);

        internal void ResolveToLocal(int index) =>
            (Location, Index) = (ApplyKind.Local, index);

        internal void ResolveToGlobal(int index) =>
            (Location, Index) = (ApplyKind.Global, index);

        internal void ResolveToDynamic() =>
            (Location, Index) = (ApplyKind.Dynamic, default);

        internal static string GetApplySignature(Symbol name, IList<IExpressionNode>? arguments) =>
            $"{name.String}({arguments?.Count ?? 0})";

        internal string GetDynamicApplySignature(Open argument) =>
            $"{argument.Value.String}({Arguments?.Count ?? 0})";

        #endregion
    }
}