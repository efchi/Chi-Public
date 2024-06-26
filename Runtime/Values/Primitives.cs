﻿using Chi.Infra;
using Chi.Parsing.Data;
using Chi.Parsing.Syntax.Abstract;
using Chi.Runtime.Abstract;
using Chi.Runtime.Values.Abstract;
using Chi.Semanting;

namespace Chi.Runtime.Values
{
    /// <summary>
    /// Primitive functions of the language.
    /// </summary>
    public static class Primitives
    {
        /// <summary>
        /// Registers all primitive functions into the symbol table and the global scope.
        /// </summary>
        public static void Register(SymbolTable symbolTable, LexicalScope globalScope)
        {
            void BindPrimitive(IPrimitive primitive)
            {
                var signatureSymbol = symbolTable.GetOrCreate(primitive.Signature);
                var declaration = new Declaration(DeclarationKind.Primitive, signatureSymbol, globalScope, primitive);
                globalScope.Append(declaration);
            }

            // Arithmetic Operators
            BindPrimitive(Sum.Instance);
            BindPrimitive(Subtract.Instance);
            BindPrimitive(Multiply.Instance);
            BindPrimitive(Divide.Instance);
            BindPrimitive(Modulo.Instance);
            BindPrimitive(Negative.Instance);
            BindPrimitive(Increment.Instance);
            BindPrimitive(Decrement.Instance);

            // Comparison Operators
            BindPrimitive(Equal.Instance);
            BindPrimitive(NotEqual.Instance);
            BindPrimitive(GreaterThan.Instance);
            BindPrimitive(LessThan.Instance);
            BindPrimitive(GreaterThanEqual.Instance);
            BindPrimitive(LessThanEqual.Instance);

            // Logical Operators
            BindPrimitive(And.Instance);
            BindPrimitive(Or.Instance);
            BindPrimitive(Not.Instance);
            BindPrimitive(Xor.Instance);

            // State Operators
            BindPrimitive(New.Instance);

            // Debug Operators
            BindPrimitive(StructuralEquivalence.Instance);
            BindPrimitive(Print.Instance);
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
                return new Number(value1 != 0 && value2 != 0 ? 1 : 0);
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
                return new Number(value1 != 0 || value2 != 0 ? 1 : 0);
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
                return new Number(value1 != 0 ^ value2 != 0 ? 1 : 0);
            }
        }

        #endregion

        #region State Operators

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
                    return false; // Base case; different types.

                if (value1 is Nil && value2 is Nil)
                    return true; // Base case; nil.

                if (value1 is Number number1 && value2 is Number number2)
                    return number1.Value == number2.Value;

                if (value1 is Open open1 && value2 is Open open2)
                    return open1.Value.Code == open2.Value.Code;

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

                if (value1 is Tuple tup1 && value2 is Tuple tup2)
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
                var serialized = Serializer.Serialize(Context.SymbolTable, value, verbose: true);

                Output.WriteLine($"Print > {serialized}", ConsoleColor.White);
                return Nil.Instance;
            }
        }

        #endregion
    }
}
