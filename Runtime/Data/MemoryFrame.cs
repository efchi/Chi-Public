using Chi.Parsing.Data;
using Chi.Parsing.Syntax;
using Chi.Runtime.Values;
using Chi.Runtime.Values.Abstract;
using Chi.Semanting;

namespace Chi.Runtime.Data
{
    public class MemoryFrame
    {
        public readonly LexicalScope Scope;
        IValueNode[] Values;

        static IValueNode GetDeclarationValue(Declaration declaration) => declaration.Kind switch
        {
            DeclarationKind.Primitive => (IPrimitive)declaration.Node,
            DeclarationKind.Def => ((DefinitionNode)declaration.Node).Value,
            DeclarationKind.Module => ((ModuleNode)declaration.Node).Value,
            DeclarationKind.Param => Nil.Instance,
            DeclarationKind.Var => Nil.Instance,
            _ => throw new NotImplementedException()
        };

        // Create a new frame from a lexical scope.
        // User for global scope and definition frame prototype.
        public MemoryFrame(LexicalScope scope)
        {
            Scope = scope;
            Values = new IValueNode[scope.Count];

            for (var i = 0; i < scope.Count; i++)
                Values[i] = GetDeclarationValue(scope[i]);
        }

        // Create a new frame from a prototype.
        // Used by Interpreter.EvalApply to create local frames.
        public MemoryFrame(MemoryFrame prototype)
        {
            Scope = prototype.Scope;
            Values = new IValueNode[prototype.Values.Length];
            Array.Copy(prototype.Values, Values, prototype.Values.Length);
        }

        // Extend scope, used by Analyzer (Pass 4) to extend global scope.
        public void Extend(LexicalScope globalScopeExtension)
        {
            var oldSize = Values.Length;
            var extensionSize = globalScopeExtension.Count;
            var newSize = oldSize + extensionSize;
            Array.Resize(ref Values, newSize);

            for (var i = 0; i < extensionSize; i++)
                Values[oldSize + i] = GetDeclarationValue(globalScopeExtension[i]);
        }

        public IValueNode Read(int index) =>
            Values[index];

        public void Write(int index, IValueNode value) =>
            Values[index] = value;

        public override string ToString() =>
            $"(MemoryFrame of {Scope.Kind} {Scope.Alias.String})";
    }
}
