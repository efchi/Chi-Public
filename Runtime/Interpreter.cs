﻿using Chi.Parsing.Syntax;
using Chi.Parsing.Syntax.Abstract;
using Chi.Runtime.Abstract;
using Chi.Runtime.Data;
using Chi.Runtime.Data.Abstract;
using Chi.Runtime.Infra;

namespace Chi.Runtime
{
    public class Interpreter : IInterpreter
    {
        internal readonly bool REPL;
        internal IValueNode LastResult = Nil.Instance;

        readonly FixedScope Global = new();
        readonly ChangingScope Dynamic = new();
        readonly Stack<IScope> Locals = new();
        IScope Current => Locals.Peek();
        bool IsCurrentGlobal => Current == Global;

        // This option is used to turn sequence optimization on and off for testing purposes.
        const bool OptimizeSequences = true;

        public Interpreter(bool repl)
        {
            REPL = repl;
            Reset();
        }

        public void Reset()
        {
            // Clear state and invoke garbage collector,
            // then register primitives and run startup scripts.

            Global.Clear();
            Dynamic.Clear();
            Locals.Clear();
            
            // Note: this call is slow and will impact test results.
            GC.Collect();

            // Global scope is always the first in the chain.
            Locals.Push(Global);

            // Register primitives in global scope.
            Primitives.Register((FixedScope)Global);

            foreach (var (_, ast) in Startup.Files)
                Run(ast);

            LastResult = Nil.Instance;
        }

        public Program Run(ProgramNode program)
        {
            var result = (Program)Eval(program);
            LastResult = result;
            return result;
        }

        #region Evaluation

        // Here the public modifier is required by Primitives
        // which refer to the Interpreter through the IInterpreter interface
        // (C# interfaces dont support internal members yet).
        public IValueNode Eval(ISyntaxNode? node)
        {
            return Normalized(node switch
            {
                // We use null as a synonym for Nil, for practical reasons.
                // Null is used in the AST to represent missing values, while Nil is used by the runtime.
                null or NilNode => Nil.Instance,

                // Wildcard node is supported only in REPL.
                WildcardNode => REPL ? LastResult : throw new NotSupportedException(),

                ProgramNode programNode => EvalProgram(programNode),
                TestNode testNode => EvalTest(testNode),
                IntegerNode integerNode => EvalInteger(integerNode),
                DecimalNode decimalNode => EvalDecimal(decimalNode),
                IdentifierNode identifierNode => EvalIdentifier(identifierNode),
                SequenceNode sequenceNode => EvalSequence(sequenceNode),
                TupleNode tupleNode => EvalTuple(tupleNode),
                DefinitionNode definitionNode => EvalDefinition(definitionNode),
                CloseNode closeNode => EvalClose(closeNode),
                ApplyNode applyNode => EvalApply(applyNode),
                AccessNode accessNode => EvalAccess(accessNode),
                IndexNode indexNode => EvalIndex(indexNode),
                ConditionalNode conditionalNode => EvalConditional(conditionalNode),
                VarNode varNode => EvalVar(varNode),
                SetNode setNode => EvalSet(setNode),

                // A node that has been already evaluated.
                IValueNode valueNode => valueNode,

                _ => throw new NotSupportedException($"{nameof(Eval)}: Unsupported node type {node.GetType().Name}."),
            });
        }

        IValueNode EvalProgram(ProgramNode programNode)
        {
            var results = new Program();

            foreach (var child in programNode.Instructions)
            {
                var result = Eval(child);
                results.Add(result);
            }
            return results;
        }

        static int testLevel = 1;
        IValueNode EvalTest(TestNode testNode)
        {
            // A test case is a named program which contains a list of instructions (definitions, statements and expressions).
            // Expressions shall evaluate to an Open OK / KO, which comes from calling check() to test for structural equivalence.
            // A test case is fully passed if it contains only definitions, nils (eg. for sets) and open OKs.
            // Tests can be nested, thus test results are recursive.

            string indent = new(' ', testLevel * 2);
            Output.WriteLine($"{indent}Testing {testNode.Name}", ConsoleColor.DarkYellow);

            testLevel++;
            var testResults = (Program)Eval(testNode.Program);
            testLevel--;

            var passed = testResults.All(r => r is Nil || r is Definition || (r is Open open && open.Value == "OK"));

            if (passed)
                Output.WriteLine($"{indent}{testNode.Name} OK", ConsoleColor.Green);
            else
                Output.WriteLine($"{indent}{testNode.Name} KO", ConsoleColor.Red);

            return passed ? new Open("OK") : new Open("KO");
        }

        static IValueNode EvalInteger(IntegerNode integerNode)
        {
            if (!int.TryParse(integerNode.Value, out var integerValue))
                throw new RuntimeException($"Invalid integer number '{integerNode.Value}'.");

            return new Number(integerValue);
        }

        static IValueNode EvalDecimal(DecimalNode decimalNode)
        {
            if (!decimal.TryParse(decimalNode.Value, out var decimalValue))
                throw new RuntimeException($"Invalid decimal number '{decimalNode.Value}'.");

            return new Number(decimalValue);
        }

        IValueNode EvalSequence(SequenceNode sequenceNode)
        {
            var sequence = new Sequence();
            EvalSequenceRecursive(sequenceNode, ref sequence);
            return sequence;

            void EvalSequenceRecursive(SequenceNode sequenceNode, ref Sequence sequence)
            {
                // Evaluate the current sequence item and append it to the value list.
                var result = Eval(sequenceNode.Expression);

                if (!OptimizeSequences)
                {
                    // Sequence Optimization is off: just append anything.
                    // The sequence will eventually be normalized by the Normalized method.
                    sequence.Add(result);
                }
                else
                {
                    // Sequence Optimization is on: skip Nil values and flatten sequences.

                    if (result is Nil)
                    {
                        // Skip Nil values.
                        goto continuation;
                    }
                    else if (result is Sequence resultSequence)
                    {
                        // Flatten sequences, skipping empty sequences.
                        if (resultSequence.Count > 0)
                            sequence.AddRange(resultSequence);
                    }
                    else
                        sequence.Add(result);
                }

            continuation:

                // If the sequence has no continuation, we just return (end of sequence).
                if (sequenceNode.Continuation is null)
                    return;

                // If the sequence continuation is a sequence node, we continue to evaluate it recursively.
                if (sequenceNode.Continuation is SequenceNode continuationSequence)
                {
                    EvalSequenceRecursive(continuationSequence, ref sequence);
                    return;
                }

                // If the sequence continuation is not a sequence node, we evaluate it and append it to the value list
                // (this should be true if and only if the continuation node is the last element of the sequence).

                // Evaluate the continuation (last sequence value) and append it to the value list.
                var resultContinuation = Eval(sequenceNode.Continuation);

                if (!OptimizeSequences)
                {
                    // Sequence Optimization is off: just append anything.
                    // The sequence will eventually be normalized by the Normalized method.
                    sequence.Add(result);
                }
                else
                {
                    // Sequence Optimization is on: skip Nil values and flatten sequences.

                    if (resultContinuation is Nil)
                    {
                        // Skip Nil values.
                    }
                    else if (resultContinuation is Sequence resultContinationSequence)
                    {
                        // Flatten sequences, skipping empty sequences.
                        if (resultContinationSequence.Count > 0)
                            sequence.AddRange(resultContinationSequence);
                    }
                    else
                        sequence.Add(resultContinuation);
                }
            }
        }

        IValueNode EvalTuple(TupleNode tupleNode)
        {
            var tuple = new Data.Tuple();

            if (tupleNode.Items is null)
                return tuple;

            foreach (var item in tupleNode.Items)
            {
                var result = Eval(item);
                tuple.Add(result);
            }
            return tuple;
        }

        IValueNode EvalIdentifier(IdentifierNode identifierNode)
        {
            // Note: this method is used only for resolving var/lets,
            // not calls, which have a signature (like F(0)), see Apply.

            // Search for identifier in current scope.
            if (Current.Find(identifierNode.Value, out var value))
                return value!;

            // Search for identifier in global scope (when Curent != Global).
            if (!IsCurrentGlobal && Global.Find(identifierNode.Value, out value))
                return value!;

            // Search for identifier in dynamic scope.
            if (Dynamic.Find(identifierNode.Value, out value))
                return value!;

            // if Identifier not found, it's an open identifier.
            return new Open(identifierNode.Value);
        }

        IValueNode EvalDefinition(DefinitionNode definitionNode)
        {
            var definitionSignature = GetDefinitionSignature(definitionNode);
            Definition definition;

            // The behavior is the same for local and global scope,
            // except for redefinition, which is allowed in global scope and REPL mode only.

            var found = Current.Find(definitionSignature, out var _);

            if (found && !IsCurrentGlobal)
                throw new RuntimeException($"Redefinition of '{definitionNode.Name}' in local scope is not allowed.");
            
            if (found && IsCurrentGlobal && !REPL)
                throw new RuntimeException($"Redefinition of '{definitionNode.Name}' in global scope is only allowed in REPL mode.");

            // Binding new definition in current scope.
            definition = new Definition(definitionNode);
            Current.Bind(definitionSignature, definition);

            return definition;
        }

        IValueNode EvalApply(ApplyNode applyNode)
        {
            // Cases like chaining application ("()()()") or member access ("x.y()") are still not supported.
            if (applyNode.Expression is not IdentifierNode && applyNode.Expression is not Open)
                throw new RuntimeException($"Invalid apply expression: expected IdentifierNode or Open but got {applyNode.Expression.GetType().Name}.");

            var (applySignature, applyTarget) = GetApplyTarget(applyNode);

            // For now we support apply only for definitions and primitives.
            if (applyTarget is Definition definition)
            {
                var definitionNode = definition.Node;
                var evaluationTarget = definitionNode.Expression;

                // Create new local scope.
                ChangingScope localScope = new();
                
                // Passing parameters into new local scope.
                if (applyNode.Arguments is not null)
                {
                    for (int i = 0; i < applyNode.Arguments.Count; i++)
                    {
                        var parameter = definitionNode.Parameters![i];
                        var argument = applyNode.Arguments[i];
                        var value = Eval(argument);
                        localScope.Bind(parameter, value);
                    }
                }

                // Push local scope.
                Locals.Push(localScope);

                // Eval expression.
                var result = Eval(evaluationTarget);

                // Pop local scope.
                Locals.Pop();

                // Return result.
                return result;
            }
            else if (applyTarget is IPrimitive primitive)
            {
                return primitive.Apply(this, applyNode.Arguments);
            }
            else
                throw new RuntimeException($"'{applySignature}' is not a definition nor a primitive.");

            // Apply Helpers.

            // Searches signature in local and global scope.
            bool FindSignature(string signature, out IValueNode? target)
            {
                if (Current.Find(signature, out target))
                    return true;

                if (!IsCurrentGlobal && Global.Find(signature, out target))
                    return true;

                // Todo: it would be possible to search also in dynamic scope.
                // Uncertain semantics (Close-With + def).

                return false;
            }

            // Eg: (ApplyNode, "F") -> Looks for "F(0)" or F(*).
            bool FindSignatureByName(ApplyNode node, string name, out string? signature, out IValueNode? target)
            {
                signature = GetApplySignature(node, name); // F(0).

                if (FindSignature(signature, out target))
                    return true;

                signature = GetApplySignatureGeneric(name);

                if (FindSignature(signature, out target))
                    return true;

                return false;
            }

            (string signature, IValueNode target) GetApplyTarget(ApplyNode node)
            {
                string formalName;

                if (node.Expression is IdentifierNode identifier1)
                    formalName = identifier1.Value;
                else if (node.Expression is Open open1)
                    formalName = open1.Value;
                else
                    // Here is where to support apply to different node types (eg: (fun(x) => x)();)
                    throw new NotImplementedException($"{nameof(GetApplyTarget)}: Apply Expression is not an IdentifierNode or Open ({node.Expression.GetType().Name} found).");

                if (Current.Find(formalName, out var target))
                {
                    // formalName is found in the current scope: resolve it.
                    string actualName;

                    if (target is IdentifierNode identifier2)
                        actualName = identifier2.Value;
                    else if (target is Open open2)
                        actualName = open2.Value;
                    else
                        // Here is where to support passing functions as arguments (eg. a = fun(x) => x).
                        throw new RuntimeException($"{nameof(GetApplyTarget)}: Actual Identifier '{formalName}' is not an IdentifierNode or Open ({target!.GetType().Name} found).");

                    if (FindSignatureByName(node, actualName, out var signature, out target))
                        return (signature!, target!);
                    else
                        throw new RuntimeException($"{nameof(GetApplyTarget)}: No overload found for '{actualName}'.");
                }
                else
                {
                    // formalName is not found in the current scope: interpret it literally.
                    // Note: this code has a double-check for Current.Find in FindSignatureByName and should be optimizable.

                    if (FindSignatureByName(node, formalName, out var signature, out target))
                        return (signature!, target!);
                    else
                        throw new RuntimeException($"{nameof(GetApplyTarget)}: Formal Identifier '{formalName}' not found in local or global scope.");
                }
            }
        }

        IValueNode EvalClose(CloseNode closeNode)
        {
            if (closeNode.Bindings is null)
                return Eval(closeNode.Expression);

            // Computing Close substitutions.
            var bindings = new List<(string name, IValueNode value)>();
            int bindingsCount = 0;
            
            foreach (var (name, expression) in closeNode.Bindings)
            {
                // Support for shielded identifiers.
                // Subsitution is without $, and they wont be replaced by nested substitutions.

                var shielded = name.StartsWith("$");

                var substitutionName = !shielded ? 
                    ((Open)EvalIdentifier(new IdentifierNode(name))).Value : // Ugly! We may refactor, replacing string with identifier.
                    name[1..];
                
                bindings.Add((substitutionName, Eval(expression)));
                bindingsCount++;
            }

            // Push.
            foreach (var (name, value) in bindings)
                Dynamic.Bind(name, value);

            var result = Eval(closeNode.Expression);

            // Pop Close substitutions.
            for (var i = 0; i < bindingsCount; i++)
                Dynamic.Pop();
            
            return result;
        }

        IValueNode EvalConditional(ConditionalNode conditionalNode)
        {
            var condition = Eval(conditionalNode.If);

            if (condition is not Number number)
                throw new RuntimeException($"Invalid conditional type: expected Number but got {condition.GetType().Name}.");

            // See Library class for boolean semantics.
            if (number.Value != 0)
                return Eval(conditionalNode.Then);

            if (conditionalNode.Else is not null)
                return Eval(conditionalNode.Else);

            return Nil.Instance;
        }

        IValueNode EvalAccess(AccessNode accessNode)
        {
            var state = (State)Eval(accessNode.Expression);
            var key = accessNode.Member.Value;
            return state[key];
        }

        IValueNode EvalIndex(IndexNode accessNode)
        {
            var state = (State)Eval(accessNode.Expression);
            var key = (Open)Eval(accessNode.Accessor);
            return state[key.Value];
        }

        IValueNode EvalVar(VarNode varNode)
        {
            // Similar to Defintion.
            var varSignature = varNode.Name;

            // The behaior is the same for local and global scope,
            // except for redefinition, which is allowed in global scope and REPL mode only.

            var found = Current.Find(varSignature, out var _);

            if (found && !IsCurrentGlobal)
                throw new RuntimeException($"Redefinition of '{varSignature}' in local scope is not allowed.");

            if (found && IsCurrentGlobal && !REPL)
                throw new RuntimeException($"Redefinition of '{varSignature}' in global scope is only allowed in REPL mode.");

            // Binding new var in current scope.

            var rValue = varNode.RValue is null ?
                new State() : // var(x) === var(x = new())
                Eval(varNode.RValue);

            Current.Bind(varSignature, rValue); 
            return Nil.Instance;
        }

        IValueNode EvalSet(SetNode setNode)
        {
            var rValue = Eval(setNode.RValue);
            
            if (setNode.LValue is IdentifierNode or Open)
            {
                // Simple Set. (set x = 1)
                // x must be defined in local or global scope.

                string name;
                if (setNode.LValue is IdentifierNode identifierNode)
                    name = identifierNode.Value;
                else if (setNode.LValue is Open open)
                    name = open.Value;
                else
                    throw new NotSupportedException();

                if (Current.Find(name, out var _))
                    Current.Bind(name, rValue);
                else if (!IsCurrentGlobal && Global.Find(name, out _))
                    Global.Bind(name, rValue);
                else
                    throw new RuntimeException($"Set: Identifier '{name}' not found in local or global scope.");
            }
            else if (setNode.LValue is AccessNode accessNode)
            {
                // Complex Set. (set x(...).y = 1)
                // x must be defined in local or global scope *and* must be a state.

                var targetState = GetTargetState(accessNode.Expression);
                var targetName = accessNode.Member;
                targetState[targetName.Value] = rValue;
            }
            else if (setNode.LValue is IndexNode indexNode)
            {
                // Complex Set. (set x(...).[y]... = 1)
                // x must be defined in local or global scope *and* must be a state.

                var targetState = GetTargetState(indexNode.Expression);
                var targetName = (Open)Eval(indexNode.Accessor);
                targetState[targetName.Value] = rValue;
            }
            else
                throw new UnreachableException();

            return Nil.Instance;

            // Set Helpers.

            State GetTargetState(IExpressionNode lValue)
            {
                if (lValue is IdentifierNode or Open)
                {
                    string name;
                    if (lValue is IdentifierNode identifierNode)
                        name = identifierNode.Value;
                    else if (lValue is Open open)
                        name = open.Value;
                    else
                        throw new NotSupportedException();

                    var found = Current.Find(name, out var value);

                    if (!found && !IsCurrentGlobal)
                        found = Global.Find(name, out value);

                    if (!found)
                        throw new RuntimeException($"Set: Identifier '{name}' not found in local or global scope.");

                    if (value is not State state)
                        throw new RuntimeException($"Set: Identifier '{name}' is not a State.");

                    return state;
                }
                else if (lValue is AccessNode accessNode)
                {
                    var nextState = GetTargetState(accessNode.Expression);
                    var key = accessNode.Member;

                    if (!nextState.ContainsKey(key.Value))
                    {
                        // Auto-create nested states.
                        var newState = new State();
                        nextState[key.Value] = newState;
                        return newState;
                    }
                    else
                    {
                        if (nextState[key.Value] is State state)
                            return state;

                        throw new RuntimeException($"Set: Access: Identifier '{key}' is not a State.");    
                    }
                }
                else if (lValue is IndexNode indexNode)
                {
                    var nextState = GetTargetState(indexNode.Expression);
                    var key = (Open)Eval(indexNode.Accessor);

                    if (!nextState.ContainsKey(key.Value))
                    {
                        // Auto-create nested states.
                        var newState = new State();
                        nextState[key.Value] = newState;
                        return newState;
                    }
                    else
                    {
                        if (nextState[key.Value] is State state)
                            return state;

                        throw new RuntimeException($"Set: Index: Identifier '{key.Value}' is not a State.");    
                    }
                }
                else
                    throw new UnreachableException();
            }
        }

        #endregion

        #region Processing Helpers

        // Sequence normalization method.
        // This method is called on every Eval result and ensures that any resulting sequence is "flattened"
        // (that is, nested sequences are merged into one) and that Nil values are removed from the sequence.
        // This enforces the semantics of Nil as an "empty value" and simplifies the computation of structural equivalence.
        static IValueNode Normalized(IValueNode atom)
        {
            switch (atom)
            {
                case null:
                    throw new NotSupportedException();

                case Program program:

                    // Don't skip Nil values in top-level expressions: we generally want to
                    // preserve the program structure (expression-result order).
                    for (var i = 0; i < program.Count; i++)
                    {
                        var item = program[i];
                        program[i] = Normalized(item);
                    }
                    return program;

                case Data.Tuple tuple:

                    // Tuples preserve structure, so we don't skip Nils inside them.
                    for (var i = 0; i < tuple.Count; i++)
                    {
                        var item = tuple[i];
                        tuple[i] = Normalized(item);
                    }
                    return tuple;

                case Sequence sequence:

                    // In sequences, we flatten nested sequences and remove Nil values.
                    for (var i = 0; i < sequence.Count; i++)
                    {
                        var item = sequence[i];
                        item = Normalized(item);

                        if (item is Nil)
                        {
                            sequence.RemoveAt(i);
                            i--;
                        }
                        else if (item is Sequence sequence2)
                        {
                            sequence.RemoveAt(i);
                            sequence.InsertRange(i, sequence2);
                            i--;
                        }
                    }

                    if (sequence.Count == 0)
                        return Nil.Instance;
                    else if (sequence.Count == 1)
                        return sequence[0];
                    else
                        return sequence;

                default:
                    return atom;
            }
        }

        #endregion

        #region Other Helpers

        static string GetDefinitionSignature(DefinitionNode definition) =>
            ((definition.Parameters?.Count ?? 0) == 0) ?
                $"{definition.Name}(0)" :
                $"{definition.Name}({definition.Parameters!.Count})";


        static string GetApplySignature(ApplyNode applyNode, string actualApplyName) =>
            ((applyNode.Arguments?.Count ?? 0) == 0) ?
                $"{actualApplyName}(0)" :
                $"{actualApplyName}({applyNode.Arguments!.Count})";

        // Signature for generic apply with variable params number,
        // used for some primitive functions (get, set).
        static string GetApplySignatureGeneric(string actualApplyName) =>
            $"{actualApplyName}(*)";

        #endregion
    }
}