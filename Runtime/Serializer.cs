using System.Text;
using Chi.Runtime.Data;
using Chi.Runtime.Data.Abstract;
using Chi.Shared;

namespace Chi.Runtime
{
    public static class Serializer
    {
        public static string Serialize(SymbolTable symbols, IValueNode? atom, bool verbose)
        {
            var builder = new StringBuilder();
            SerializeRecursive(symbols, atom, verbose, builder);
            return builder.ToString();
        }

        static void SerializeRecursive(SymbolTable symbols, IValueNode? atom, bool verbose, StringBuilder builder)
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
                    builder.Append((verbose ? "opn:" : "") + symbols.Get(open.Symbol));
                    return;
                
                case Definition definition:
                    builder.Append((verbose ? "def:" : "") + symbols.Get(definition.Node.Symbol));
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
                        SerializeRecursive(symbols, item, verbose, builder);
                        if (i++ < count - 1)
                            builder.Append(' ');
                    }
                    if (verbose)
                        builder.Append(')');
                    return;

                case Data.Tuple tuple:
                    builder.Append('{');
                    i = 0;
                    count = tuple.Count;
                    foreach (var item in tuple)
                    {
                        SerializeRecursive(symbols, item, verbose, builder);
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
                        SerializeRecursive(symbols, item, verbose, builder);
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
                    foreach (var (symbol, value) in state)
                    {
                        builder.Append(symbols.Get(symbol));
                        builder.Append('=');
                        SerializeRecursive(symbols, value, verbose, builder);
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
