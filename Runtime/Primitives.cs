using Chi.Parsing.Syntax.Abstract;
using Chi.Runtime.Abstract;
using Chi.Runtime.Data;
using Chi.Runtime.Data.Abstract;
using Chi.Runtime.Infra;

namespace Chi.Runtime
{
    public static class Primitives
    {
        public static void Register(FixedScope scope)
        {
            // Arithmetic Operators
            scope[Sum.Instance.Signature] = Sum.Instance;
            scope[Subtract.Instance.Signature] = Subtract.Instance;
            scope[Multiply.Instance.Signature] = Multiply.Instance;
            scope[Divide.Instance.Signature] = Divide.Instance;
            scope[Modulo.Instance.Signature] = Modulo.Instance;
            scope[Negative.Instance.Signature] = Negative.Instance;
            scope[Increment.Instance.Signature] = Increment.Instance;
            scope[Decrement.Instance.Signature] = Decrement.Instance;

            // Comparison Operators
            scope[Equal.Instance.Signature] = Equal.Instance;
            scope[NotEqual.Instance.Signature] = NotEqual.Instance;
            scope[GreaterThan.Instance.Signature] = GreaterThan.Instance;
            scope[LessThan.Instance.Signature] = LessThan.Instance;
            scope[GreaterThanEqual.Instance.Signature] = GreaterThanEqual.Instance;
            scope[LessThanEqual.Instance.Signature] = LessThanEqual.Instance;

            // Logical Operators
            scope[And.Instance.Signature] = And.Instance;
            scope[Or.Instance.Signature] = Or.Instance;
            scope[Not.Instance.Signature] = Not.Instance;
            scope[Xor.Instance.Signature] = Xor.Instance;

            // State Operators
            scope[New.Instance.Signature] = New.Instance;
            
            // Debug Operators
            scope[StructuralEquivalence.Instance.Signature] = StructuralEquivalence.Instance;
            scope[Print.Instance.Signature] = Print.Instance;
        }

        #region Arithmetic Operators

        public class Sum : IPrimitive
        {
            public static IPrimitive Instance => new Sum();
            public string Signature => "+(2)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if (arguments?.Count != 2)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                var value1 = ((Number)interpreter.Eval(arguments[0])).Value!;
                var value2 = ((Number)interpreter.Eval(arguments[1])).Value!;
                return new Number(value1 + value2);
            }
        }

        public class Subtract : IPrimitive
        {
            public static IPrimitive Instance => new Subtract();
            public string Signature => "-(2)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if (arguments?.Count != 2)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                var value1 = ((Number)interpreter.Eval(arguments[0])).Value!;
                var value2 = ((Number)interpreter.Eval(arguments[1])).Value!;
                return new Number(value1 - value2);
            }
        }

        public class Multiply : IPrimitive
        {
            public static IPrimitive Instance => new Multiply();
            public string Signature => "mul(2)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if (arguments?.Count != 2)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                var value1 = ((Number)interpreter.Eval(arguments[0])).Value!;
                var value2 = ((Number)interpreter.Eval(arguments[1])).Value!;
                return new Number(value1 * value2);
            }
        }

        public class Divide : IPrimitive
        {
            public static IPrimitive Instance => new Divide();
            public string Signature => "div(2)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if (arguments?.Count != 2)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                var value1 = ((Number)interpreter.Eval(arguments[0])).Value!;
                var value2 = ((Number)interpreter.Eval(arguments[1])).Value!;
                return new Number(value1 / value2);
            }
        }

        public class Modulo : IPrimitive
        {
            public static IPrimitive Instance => new Modulo();
            public string Signature => "mod(2)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if (arguments?.Count != 2)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                var value1 = ((Number)interpreter.Eval(arguments[0])).Value!;
                var value2 = ((Number)interpreter.Eval(arguments[1])).Value!;
                return new Number(value1 % value2);
            }
        }

        public class Negative : IPrimitive
        {
            public static IPrimitive Instance => new Negative();
            public string Signature => "-(1)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if (arguments?.Count != 1)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                var value = ((Number)interpreter.Eval(arguments[0])).Value!;
                return new Number(-value);
            }
        }

        public class Increment : IPrimitive
        {
            public static IPrimitive Instance => new Increment();
            public string Signature => "++(1)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if (arguments?.Count != 1)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                var value = ((Number)interpreter.Eval(arguments[0])).Value!;
                return new Number(value + 1);
            }
        }

        public class Decrement : IPrimitive
        {
            public static IPrimitive Instance => new Decrement();
            public string Signature => "--(1)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if (arguments?.Count != 1)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                var value = ((Number)interpreter.Eval(arguments[0])).Value!;
                return new Number(value - 1);
            }
        }

        #endregion

        #region Comparison Operators

        public class Equal : IPrimitive
        {
            public static IPrimitive Instance => new Equal();
            public string Signature => "eq(2)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if (arguments?.Count != 2)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                var value1 = ((Number)interpreter.Eval(arguments[0])).Value!;
                var value2 = ((Number)interpreter.Eval(arguments[1])).Value!;
                return new Number(value1 == value2 ? 1 : 0);
            }
        }

        public class NotEqual : IPrimitive
        {
            public static IPrimitive Instance => new NotEqual();
            public string Signature => "neq(2)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if (arguments?.Count != 2)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                var value1 = ((Number)interpreter.Eval(arguments[0])).Value!;
                var value2 = ((Number)interpreter.Eval(arguments[1])).Value!;
                return new Number(value1 != value2 ? 1 : 0);
            }
        }

        public class GreaterThan : IPrimitive
        {
            public static IPrimitive Instance => new GreaterThan();
            public string Signature => "gt(2)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if (arguments?.Count != 2)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                var value1 = ((Number)interpreter.Eval(arguments[0])).Value!;
                var value2 = ((Number)interpreter.Eval(arguments[1])).Value!;
                return new Number(value1 > value2 ? 1 : 0);
            }
        }

        public class LessThan : IPrimitive
        {
            public static IPrimitive Instance => new LessThan();
            public string Signature => "lt(2)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if (arguments?.Count != 2)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                var value1 = ((Number)interpreter.Eval(arguments[0])).Value!;
                var value2 = ((Number)interpreter.Eval(arguments[1])).Value!;
                return new Number(value1 < value2 ? 1 : 0);
            }
        }

        public class GreaterThanEqual : IPrimitive
        {
            public static IPrimitive Instance => new GreaterThanEqual();
            public string Signature => "gte(2)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if (arguments?.Count != 2)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                var value1 = ((Number)interpreter.Eval(arguments[0])).Value!;
                var value2 = ((Number)interpreter.Eval(arguments[1])).Value!;
                return new Number(value1 >= value2 ? 1 : 0);
            }
        }

        public class LessThanEqual : IPrimitive
        {
            public static IPrimitive Instance => new LessThanEqual();
            public string Signature => "lte(2)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if (arguments?.Count != 2)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                var value1 = ((Number)interpreter.Eval(arguments[0])).Value!;
                var value2 = ((Number)interpreter.Eval(arguments[1])).Value!;
                return new Number(value1 <= value2 ? 1 : 0);
            }
        }

        #endregion

        #region Logical Operators

        public class And : IPrimitive
        {
            public static IPrimitive Instance => new And();
            public string Signature => "and(2)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if (arguments?.Count != 2)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                var value1 = ((Number)interpreter.Eval(arguments[0])).Value!;
                var value2 = ((Number)interpreter.Eval(arguments[1])).Value!;
                return new Number((value1 != 0 && value2 != 0) ? 1 : 0);
            }
        }

        public class Or : IPrimitive
        {
            public static IPrimitive Instance => new Or();
            public string Signature => "or(2)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if (arguments?.Count != 2)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                var value1 = ((Number)interpreter.Eval(arguments[0])).Value!;
                var value2 = ((Number)interpreter.Eval(arguments[1])).Value!;
                return new Number((value1 != 0 || value2 != 0) ? 1 : 0);
            }
        }

        public class Not : IPrimitive
        {
            public static IPrimitive Instance => new Not();
            public string Signature => "not(1)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if (arguments?.Count != 1)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                var value = ((Number)interpreter.Eval(arguments[0])).Value!;
                return new Number(value == 0 ? 1 : 0);
            }
        }

        public class Xor : IPrimitive
        {
            public static IPrimitive Instance => new Xor();
            public string Signature => "xor(2)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if (arguments?.Count != 2)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                var value1 = ((Number)interpreter.Eval(arguments[0])).Value!;
                var value2 = ((Number)interpreter.Eval(arguments[1])).Value!;
                return new Number((value1 != 0 ^ value2 != 0) ? 1 : 0);
            }
        }

        #endregion

        #region State Operators

        // Todo: initialize state from named tuple + syntactical new (new:{tuple}).
        public class New : IPrimitive
        {
            public static IPrimitive Instance => new New();
            public string Signature => "new(0)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if ((arguments?.Count ?? 0) != 0)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                return new State();
            }
        }

        #endregion

        #region Debug Operators

        // Structural equivalence operator used for test purposes.
        public class StructuralEquivalence : IPrimitive
        {
            public static IPrimitive Instance => new StructuralEquivalence();
            public string Signature => "steq(2)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if (arguments?.Count != 2)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                var value1 = interpreter.Eval(arguments[0]);
                var value2 = interpreter.Eval(arguments[1]);
                var equivalent = CheckEquivalenceRecursive(value1, value2);
                return new Number(equivalent ? 1 : 0);
            }

            static bool CheckEquivalenceRecursive(IValueNode? value1, IValueNode? value2)
            {
                if (value1 is Entry pro1 && value2 is Entry pro2)
                    throw new NotSupportedException($"{nameof(CheckEquivalenceRecursive)}: Should never happen (Program).");

                if (value1 is IPrimitive pri1 && value2 is IPrimitive pri2)
                    throw new NotSupportedException($"{nameof(CheckEquivalenceRecursive)}: Should never happen (Primitive).");

                if (value1 is Definition def1 && value2 is Definition def2)
                    throw new NotSupportedException($"{nameof(CheckEquivalenceRecursive)}: Should never happen (Definition).");

                value1 ??= Nil.Instance;
                value2 ??= Nil.Instance;

                if (value1!.GetType() != value2!.GetType())
                    return false; // base case; different types.

                if (value1 is Nil && value2 is Nil)
                    return true; // base case; nil.

                if (value1 is Number number1 && value2 is Number number2)
                    return number1.Value == number2.Value;

                if (value1 is Open open1 && value2 is Open open2)
                    return open1.Value == open2.Value;

                if (value1 is State state1 && value2 is State state2)
                {
                    if (state1.Count != state2.Count)
                        return false;

                    foreach (var key in state1.Keys)
                    {
                        if (!state2.ContainsKey(key))
                            return false;

                        if (!CheckEquivalenceRecursive(state1[key], state2[key]))
                            return false;
                    }

                    return true;
                }

                if (value1 is Sequence seq1 && value2 is Sequence seq2)
                {
                    if (seq1.Count != seq2.Count)
                        return false;

                    for (var i = 0; i < seq1.Count; i++)
                    {
                        if (!CheckEquivalenceRecursive(seq1[i], seq2[i]))
                            return false;
                    }

                    return true;
                }

                if (value1 is Data.Tuple tup1 && value2 is Data.Tuple tup2)
                {
                    if (tup1.Count != tup2.Count)
                        return false;

                    for (var i = 0; i < tup1.Count; i++)
                    {
                        if (!CheckEquivalenceRecursive(tup1[i], tup2[i]))
                            return false;
                    }

                    return true;
                }

                throw new NotSupportedException($"{nameof(CheckEquivalenceRecursive)}: Should never happen.");
            }
        }

        public class Print : IPrimitive
        {
            public static IPrimitive Instance => new Print();
            public string Signature => "print(1)";

            public IValueNode Apply(IInterpreter interpreter, IList<IExpressionNode>? arguments)
            {
                if (arguments?.Count != 1)
                    throw new PrimitiveException($"Primitive {GetType().Name}: invalid arguments.");

                var value = interpreter.Eval(arguments[0]);
                var serialized = Serializer.Serialize(value, verbose: true);

                Output.WriteLine($"Print > {serialized}", ConsoleColor.White);
                return Nil.Instance;
            }
        }

        #endregion
    }
}
