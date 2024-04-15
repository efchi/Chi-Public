using System.Text;
using Chi.Parsing.Data;
using Chi.Parsing.Syntax;
using Chi.Runtime.Values;
using Chi.Runtime.Values.Abstract;
using Chi.Semanting;

namespace Chi.Runtime
{
    public static class Serializer
    {
        public static string Serialize(SymbolTable symbols, IValueNode? atom, bool verbose)
        {
            var builder = new StringBuilder();
            SerializeRecursive(atom, verbose, builder);
            return builder.ToString();

            void SerializeRecursive(IValueNode? atom, bool verbose, StringBuilder builder)
            {
                int i;
                int count;

                switch (atom)
                {
                    case null:
                        throw new NotSupportedException();

                    case Nil:
                        builder.Append("nil");
                        return;

                    case Number number:
                        builder.Append((verbose ? "num:" : "") + number.Value);
                        return;

                    case Open open:
                        builder.Append((verbose ? "opn:" : "") + open.Value.String);
                        return;

                    case Definition definition:
                        builder.Append((verbose ? "def:" : "") + definition.Node.Name.String);
                        return;

                    case Module module:
                        builder.Append((verbose ? "mod:" : "") + module.Node.Name.Value.String);
                        if (verbose)
                        { 
                            builder.Append('{');
                            i = 0;
                            count = module.Node.Scope.Count;
                            foreach (var item in module.Node.Scope)
                            {
                                if (i++ == 0)
                                    continue; // Skipping self-references.

                                switch(item.Kind)
                                {
                                    case DeclarationKind.Def:
                                        builder.Append("def:" + ((DefinitionNode)item.Node).Signature.String);
                                        break;
                                    case DeclarationKind.Module:
                                        SerializeRecursive(((ModuleNode)item.Node).Value, verbose, builder);
                                        break;
                                    case DeclarationKind.Var:
                                        builder.Append("var:" + item.Symbol.String);
                                        break;
                                    default:
                                        throw new NotImplementedException();
                                }
                                
                                if (i++ < count - 1)
                                    builder.Append(',');
                            }
                            builder.Append('}');
                        }
                        return;

                    case IPrimitive primitive:
                        builder.Append((verbose ? "pri:" : "") + primitive.Signature);
                        return;

                    case Sequence sequence:
                        if (verbose)
                            builder.Append('(');
                        i = 0;
                        count = sequence.Count;
                        foreach (var item in sequence)
                        {
                            SerializeRecursive(item, verbose, builder);
                            if (i++ < count - 1)
                                builder.Append(' ');
                        }
                        if (verbose)
                            builder.Append(')');
                        return;

                    case Values.Tuple tuple:
                        builder.Append('{');
                        i = 0;
                        count = tuple.Count;
                        foreach (var item in tuple)
                        {
                            SerializeRecursive(item, verbose, builder);
                            if (i++ < count - 1)
                                builder.Append(',');
                        }
                        builder.Append('}');
                        return;

                    case Program program:
                        if (verbose)
                            builder.Append('[');
                        i = 0;
                        count = program.Count;
                        foreach (var item in program)
                        {
                            SerializeRecursive(item, verbose, builder);
                            if (i++ < count - 1)
                                builder.Append(';');
                        }
                        if (verbose)
                            builder.Append(']');
                        return;

                    case State state:
                        builder.Append('<');
                        i = 0;
                        count = state.Count;
                        foreach (var (symbolCode, value) in state)
                        {
                            builder.Append(symbols.GetByCode(symbolCode).String);
                            builder.Append('=');
                            SerializeRecursive(value, verbose, builder);
                            if (i++ < count - 1)
                                builder.Append(',');
                        }
                        builder.Append('>');
                        return;

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
