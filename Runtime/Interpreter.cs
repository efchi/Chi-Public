using Chi.Infra;
using Chi.Parsing.Data;
using Chi.Parsing.Syntax;
using Chi.Parsing.Syntax.Abstract;
using Chi.Runtime.Abstract;
using Chi.Runtime.Data;
using Chi.Runtime.Values;
using Chi.Runtime.Values.Abstract;
using Chi.Semanting;

namespace Chi.Runtime
{
    /// <summary>
    /// Interpreter implementation for the Chi language.
    /// This interpreter is a stack-based interpreter supporting lexical scope 
    /// (local and global lookup in O(1)) and dynamic scope.
    /// </summary>
    public class Interpreter : IInterpreter
    {
        readonly StackScope DynamicScope = new();
        readonly Stack<MemoryFrame> LocalFrames = new();
        MemoryFrame LocalFrame => LocalFrames.Peek();

        public Program Run(ProgramNode program)
        {
            var result = (Program)Eval(program);
            return result;
        }

        #region Evaluation

        // The public modifier is required by Primitives.Register, which needs to access the Eval method.
        public IValueNode Eval(ISyntaxNode? node)
        {
            return Normalized(node switch
            {
                // We use null as a synonym for Nil, for practical reasons.
                // Null is used in the AST to represent missing values, while Nil is used by the runtime.
                null or NilNode => Nil.Instance,

                // Wildcard node is supported only in REPL.
                WildcardNode => Context.IsREPL ? Workflow.CurrentResult : throw new NotSupportedException(),

                ProgramNode programNode => EvalProgram(programNode),
                TestNode testNode => EvalTest(testNode),
                ModuleNode moduleNode => EvalModule(moduleNode),
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

                _ => throw new NotSupportedException($"Interpreter :: Eval :: Unsupported node type {node.GetType().Name}."),
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
            // A test case is a named module which contains a list of instructions (definitions, statements and expressions).
            // Expressions shall evaluate to an Open OK / KO, which comes from calling check() to test for structural equivalence.
            // A test case is fully passed if it contains only definitions, nils (eg. for sets) and open OKs, and at least one OK.
            // Tests can be nested, thus test results are recursive.

            string indent = new(' ', testLevel * 2);
            testLevel++;

            try
            {
                var testResults = new List<IValueNode>();

                foreach (var instruction in testNode.Instructions)
                    testResults.Add(Eval(instruction));

                var passed = Workflow.IsTestPassed(testResults);

                if (passed)
                    Output.WriteLine($"{indent}{testNode.Name.Value.String} OK", testLevel == 1 ? ConsoleColor.Green : ConsoleColor.DarkGreen);
                else
                    Output.WriteLine($"{indent}{testNode.Name.Value.String} KO", testLevel == 1 ? ConsoleColor.Red : ConsoleColor.DarkRed);

                return passed ? Context.OpenOK : Context.OpenKO;
            }
            catch (Exception ex)
            {
                Output.WriteLine($"{indent}{testNode.Name.Value.String} KO with exception :: {ex.Message}", ConsoleColor.DarkRed);
                return Context.OpenKO;
            }
            finally
            {
                testLevel--;
            }
        }

        static IValueNode EvalInteger(IntegerNode integerNode) =>
            new Number(integerNode.Value);

        static IValueNode EvalDecimal(DecimalNode decimalNode) =>
            new Number(decimalNode.Value);

        static IValueNode EvalDefinition(DefinitionNode definitionNode) =>
            definitionNode.Value;

        static IValueNode EvalModule(ModuleNode moduleNode) =>
            moduleNode.Value;

        IValueNode EvalSequence(SequenceNode sequenceNode)
        {
            var sequence = new Sequence();
            EvalSequenceRecursive(sequenceNode, ref sequence);
            return sequence;

            void EvalSequenceRecursive(SequenceNode sequenceNode, ref Sequence sequence)
            {
                // Evaluate the current sequence item and append it to the value list.
                var result = Eval(sequenceNode.Expression);

                if (!Context.Options.OptimizeSequences)
                {
                    // Sequence Optimization is off: just append anything.
                    // The sequence will eventually be normalized by the Normalized method.
#pragma warning disable CS0162 // Unreachable code detected
                    sequence.Add(result);
#pragma warning restore CS0162 // Unreachable code detected
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

                if (!Context.Options.OptimizeSequences)
                {
                    // Sequence Optimization is off: just append anything.
                    // The sequence will eventually be normalized by the Normalized method.
#pragma warning disable CS0162 // Unreachable code detected
                    sequence.Add(result);
#pragma warning restore CS0162 // Unreachable code detected
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
            var tuple = new Values.Tuple();

            if (tupleNode.Items is null)
                return tuple;

            foreach (var item in tupleNode.Items)
            {
                var result = Eval(item);
                tuple.Add(result);
            }
            return tuple;
        }

        IValueNode EvalIdentifier(IdentifierNode identifierNode) => identifierNode.Location switch
        {
            IdentifierLocation.Unresolved => 
                throw new UnreachableException($"Interpreter :: EvalIdentifier :: Unresolved identifier node '{identifierNode.Value.String}'."),
            
            IdentifierLocation.Global => 
                Context.GlobalFrame.Read(identifierNode.Index!.Value),
            
            IdentifierLocation.Local => 
                LocalFrame.Read(identifierNode.Index!.Value),
            
            IdentifierLocation.Dynamic =>
                DynamicScope.Find(identifierNode.Value, out var value) ? value! : new Open(identifierNode.Value),
            
            _ => throw new UnreachableException($"Interpreter :: EvalIdentifier :: Unsupported identifier location {identifierNode.Location}."),
        };

        IValueNode EvalApply(ApplyNode applyNode)
        {
            // Cases like chaining application ("()()()") or member access ("x.y()") are still unsupported.
            if (applyNode.Expression is not IdentifierNode && applyNode.Expression is not Open)
                throw new RuntimeException($"Interpreter :: EvalApply :: Invalid Apply Expression. Expected IdentifierNode or Open but got {applyNode.Expression.GetType().Name}.");

            var (applySignature, applyTarget) = GetApplyTarget(applyNode);

            // This function gets the evaluation target of the apply node based on its lexical scope.
            (Symbol signatureSymbol, IValueNode target) GetApplyTarget(ApplyNode node)
            {
                switch (node.Location)
                {
                    case ApplyKind.Unresolved:
                        throw new UnreachableException($"Interpreter :: EvalApply :: GetApplyTarget :: Unresolved apply node '{node.Signature.String}'");

                    case ApplyKind.Local:
                        // Target was resolved in local scope.
                        return (node.Signature, LocalFrame.Read(node.Index!.Value));

                    case ApplyKind.Global:
                        // Target was resolved in global scope.
                        return (node.Signature, Context.GlobalFrame.Read(node.Index!.Value));

                    case ApplyKind.Dynamic:
                        // Target was not resolved during analysis.
                        // "Open calls" are not yet supported.
                        throw new RuntimeException($"Interpreter :: EvalApply :: GetApplyTarget :: Apply target '{node.Signature.String}' not found.");  

                    case ApplyKind.Parametric:
                        // Target was resolved as a local parameter.
                        // For now, we assume that passed argument is always an open name.
                        var argument = LocalFrame.Read(node.Index!.Value);
                        if (argument is not Open)
                            throw new RuntimeException($"Interpreter :: EvalApply :: GetApplyTarget :: Argument '{node.Signature.String}' is not an Open.");

                        // Getting the apply dynamic signature.
                        var dynamicSignature = Context.SymbolTable.GetOrCreate(
                            node.GetDynamicApplySignature((Open)argument));

                        // Looking for the dynamic signature in the scope chain.
                        if (!LocalFrame.Scope.RecursiveLookup(dynamicSignature, out var declaration, out var targetScope, out var scopeIndex))
                            throw new RuntimeException($"Interpreter :: EvalApply :: GetApplyTarget :: 'Signature {dynamicSignature.String}' not found in local or global scope.");
                        
                        // For now, only definitions and primitives are supported.
                        else if (declaration!.Kind != DeclarationKind.Def && declaration.Kind != DeclarationKind.Primitive)
                            throw new RuntimeException($"Interpreter :: EvalApply :: GetApplyTarget :: 'Signature {dynamicSignature.String}' is not a definition nor a primitive.");

                        return targetScope!.Kind switch
                        {
                            LexicalScopeKind.Singleton =>
                                // Dynamic target was found in global scope.
                                (dynamicSignature, Context.GlobalFrame.Read(targetScope.GlobalFrameStart!.Value + scopeIndex!.Value)),
                            
                            LexicalScopeKind.Instance => 
                                // Dynamic target was found in local scope.
                                (dynamicSignature, LocalFrame.Read(scopeIndex!.Value)),
                            
                            _ => throw new UnreachableException($"Interpreter :: EvalApply :: GetApplyTarget :: Unsupported lexical scope kind {targetScope.Kind}."),
                        };

                    default:
                        throw new UnreachableException($"Interpreter :: EvalApply :: GetApplyTarget :: Unsupported apply node location {node.Location}.");
                }
            }

            if (applyTarget is Definition definition)
            {
                var definitionNode = definition.Node;
                var evaluationTarget = definitionNode.Expression;
    
                // Create new local frame.
                MemoryFrame localScope = new(definitionNode.FramePrototype!);

                // Passing parameters into the new local frame.
                // Note that order matters. For now, we only have positional parameters (not named parameters).
                if (applyNode.Arguments is not null)
                {
                    for (int i = 0; i < applyNode.Arguments.Count; i++)
                    {
                        var parameter = definitionNode.Parameters![i];
                        var argument = applyNode.Arguments[i];
                        var value = Eval(argument);

                        // Note that the first item in the local scope is the target itself (eg. definition).
                        localScope.Write(i + 1, value);
                    }
                }

                // Push the local frame.
                LocalFrames.Push(localScope);

                // Recurse.
                var result = Eval(evaluationTarget);

                // Pop local frame.
                LocalFrames.Pop();

                // Return result.
                return result;
            }
            else if (applyTarget is IPrimitive primitive)
            {
                // Primitive function application.
                return primitive.Apply(this, applyNode.Arguments);
            }
            else
                throw new RuntimeException($"Interpreter :: EvalApply :: GetApplyTarget :: 'Signature {applySignature}' is not a definition nor a primitive.");
        }

        IValueNode EvalClose(CloseNode closeNode)
        {
            if (closeNode.Bindings is null)
                return Eval(closeNode.Expression);

            // Evaluating close bindings.
            var bindings = new List<(IdentifierNode name, IValueNode value)>();
            int bindingsCount = 0;

            foreach (var (name, expression) in closeNode.Bindings)
            {
                // Support for shielded identifiers.
                // If the identifier starts with $, it wont be evaluated in its scope, but always treated as an open symbol.

                var identifier = name.Value.String;
                var shielded = identifier.StartsWith("$");

                Symbol argumentSymbol;

                if (shielded)
                {
                    // Shielded: remove the $ and dont evaluate the identifier.
                    argumentSymbol = Context.SymbolTable.GetOrCreate(identifier[1..]);
                }
                else
                {
                    // Unshielded: evaluate the identifier as an Open symbol.
                    var argumentValue = EvalIdentifier(name);
                    if (argumentValue is not Open open)
                        throw new RuntimeException($"Interpreter :: EvalClose :: Invalid binding argument: expected Open but got {argumentValue.GetType().Name}.");

                    argumentSymbol = ((Open)EvalIdentifier(name)).Value;
                }

                var bindingIdentifier = new IdentifierNode(argumentSymbol);
                bindingIdentifier.ResolveToDynamic();

                bindings.Add((bindingIdentifier, Eval(expression)));
                bindingsCount++;
            }

            // Push bindings.
            foreach (var (name, value) in bindings)
                DynamicScope.Bind(name.Value, value);

            // Recurse.
            var result = Eval(closeNode.Expression);

            // Pop bindings.
            for (var i = 0; i < bindingsCount; i++)
                DynamicScope.Pop();

            // Return result.
            return result;
        }

        IValueNode EvalConditional(ConditionalNode conditionalNode)
        {
            var condition = Eval(conditionalNode.If);

            if (condition is not Number number)
                throw new RuntimeException($"Interpreter :: EvalConditional :: Invalid conditional type: expected Number but got {condition.GetType().Name}.");

            // See Primitive class for boolean semantics.
            if (number.Value != 0)
                return Eval(conditionalNode.Then);

            if (conditionalNode.Else is not null)
                return Eval(conditionalNode.Else);

            return Nil.Instance;
        }

        IValueNode EvalAccess(AccessNode accessNode)
        {
            var state = (State)Eval(accessNode.Expression);
            var key = accessNode.Member;
            return state[key.Value.Code];
        }

        IValueNode EvalIndex(IndexNode accessNode)
        {
            var state = (State)Eval(accessNode.Expression);
            var key = (Open)Eval(accessNode.Accessor);
            return state[key.Value.Code];
        }

        IValueNode EvalVar(VarNode varNode)
        {
            // Computing Var r-value.
            // Note: var(x) === var(x = new())

            var rValue = varNode.RValue is not null ? 
                Eval(varNode.RValue) : 
                new State();

            // The Var semantics is similar to the identifier case,
            // except that we write instead of reading into the target frame.
            
            switch (varNode.Name.Location)
            {
                case IdentifierLocation.Unresolved:
                    throw new UnreachableException($"Interpreter :: EvalVar :: Unresolved identifier '{varNode.Name.Value.String}'.");

                case IdentifierLocation.Dynamic:
                    // A var is always resolved in local or global scope.
                    throw new UnreachableException($"Interpreter :: EvalVar :: Inconsistent identifier location (Dynamic).");

                case IdentifierLocation.Global:
                    Context.GlobalFrame.Write(varNode.Name.Index!.Value, rValue);
                    break;

                case IdentifierLocation.Local:
                    LocalFrame.Write(varNode.Name.Index!.Value, rValue);
                    break;

                default:
                    throw new UnreachableException($"Interpreter :: EvalVar :: Unsupported identifier location {varNode.Name.Location}.");
            }

            return Nil.Instance;
        }

        IValueNode EvalSet(SetNode setNode)
        {
            var rValue = Eval(setNode.RValue);

            if (setNode.LValue is IdentifierNode identifierNode)
            {
                // Simple set (set x = 1).
                // x must be defined in local or global scope.

                switch (identifierNode.Location)
                {
                    case IdentifierLocation.Global:
                        Context.GlobalFrame.Write(identifierNode.Index!.Value, rValue);
                        break;
                    
                    case IdentifierLocation.Local:
                        LocalFrame.Write(identifierNode.Index!.Value, rValue);
                        break;

                    case IdentifierLocation.Dynamic:
                        throw new RuntimeException($"Interpreter :: EvalSet :: Identifier '{identifierNode.Value.String}' not found in local or global scope.");                 
                            
                    default: 
                        throw new UnreachableException($"Interpreter :: EvalSet :: Unsupported identifier location {identifierNode.Location}");
                }
            }
            else if (setNode.LValue is AccessNode accessNode)
            {
                // Complex Set (set x(...).y = 1).
                // x must be defined in local or global scope *and* must be a state.

                var targetState = GetTargetState(accessNode.Expression);
                var targetName = accessNode.Member;
                targetState[targetName.Value.Code] = rValue;
            }
            else if (setNode.LValue is IndexNode indexNode)
            {
                // Complex Set (set x(...).[y]... = 1).
                // x must be defined in local or global scope *and* must be a state.

                var targetState = GetTargetState(indexNode.Expression);

                var accessorValue = Eval(indexNode.Accessor);
                if (accessorValue is not Open targetName)
                    throw new RuntimeException($"Interpreter :: EvalSet :: Invalid index accessor: expected Open but got {accessorValue.GetType().Name}.");

                targetState[targetName.Value.Code] = rValue;
            }
            else
                throw new UnreachableException($"Interpreter :: EvalSet :: Unsupported node type {setNode.GetType().Name}");

            return Nil.Instance;

            // This methods gets the target state of a set node based on its lexical scope.
            State GetTargetState(IExpressionNode lValue)
            {
                if (lValue is IdentifierNode identifierNode2) 
                {
                    // Base case: getting root state.
                    var value = identifierNode2.Location switch
                    {
                        IdentifierLocation.Global =>
                            Context.GlobalFrame.Read(identifierNode2.Index!.Value),
                    
                        IdentifierLocation.Local =>
                            LocalFrame.Read(identifierNode2.Index!.Value),
                    
                        IdentifierLocation.Dynamic =>
                            throw new RuntimeException($"Interpreter :: EvalSet :: GetTargetState :: Identifier '{identifierNode2.Value.String}' not found in local or global scope."),

                        _ => throw new UnreachableException($"Interpreter :: EvalSet :: GetTargetState :: Unsupported identifier location {identifierNode2.Location}")
                    };

                    if (value is not State state)
                        throw new RuntimeException($"Interpreter :: EvalSet :: GetTargetState ::: Identifier '{identifierNode2.Value.String}' is not a State.");
                    
                    return state;
                }
                else if (lValue is AccessNode accessNode)
                {
                    var nextState = GetTargetState(accessNode.Expression);
                    var key = accessNode.Member;

                    if (!nextState.ContainsKey(key.Value.Code))
                    {
                        // Auto-create nested states.
                        var newState = new State();
                        nextState[key.Value.Code] = newState;
                        return newState;
                    }
                    else
                    {
                        if (nextState[key.Value.Code] is State state)
                            return state;

                        throw new RuntimeException($"Interpreter :: EvalSet :: GetTargetState :: Access identifier '{key.Value.String}' is not a State.");
                    }
                }
                else if (lValue is IndexNode indexNode)
                {
                    var nextState = GetTargetState(indexNode.Expression);
                    var key = (Open)Eval(indexNode.Accessor);

                    if (!nextState.ContainsKey(key.Value.Code))
                    {
                        // Auto-create nested states.
                        var newState = new State();
                        nextState[key.Value.Code] = newState;
                        return newState;
                    }
                    else
                    {
                        if (nextState[key.Value.Code] is State state)
                            return state;

                        throw new RuntimeException($"Interpreter :: EvalSet :: GetTargetState :: Index identifier '{key.Value.String}' is not a State.");
                    }
                }
                else
                    throw new UnreachableException($"Interpreter :: EvalSet :: GetTargetState :: Unsupported node type {lValue.GetType().Name}.");
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

                case Values.Tuple tuple:

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
    }
}
