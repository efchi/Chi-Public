using Chi.Infra;
using Chi.Parsing.Data;
using Chi.Parsing.Syntax;
using Chi.Parsing.Syntax.Abstract;
using Chi.Runtime.Data;
using Chi.Runtime.Values;

namespace Chi.Semanting
{
    /// <summary>
    /// Static analyzer for Chi programs.
    /// The analyzer recurses over the AST and build lexical scopes, 
    /// resolving identifiers and function calls.
    /// </summary>
    public class Analyzer
    {
        SymbolTable Symbols { get; set; }                                       // Global symbol table, shared across programs.
        LexicalScope GlobalScope { get; set; }                                  // Global scope, shared across programs (Global <= Program <= Module <= Definition).
        MemoryFrame GlobalFrame { get; set; }                                   // Global memory frame, shared across programs.
        
        Stack<(Symbol Name, LexicalScope Scope)> Scopes { get; set; }           // Scope chain.
        LexicalScope CurrentScope => Scopes.Peek().Scope;                       // Current scope (<global>, module1, module2, A(x), ...).

        string CurrentScopeAlias => 
            string.Join('/', Scopes.Select(i => i.Name.String).Reverse());      // Current scope alias (eg. <global>/module1/module2/A(x)).

        public void Run(SymbolTable symbols, LexicalScope globalScope, MemoryFrame globalFrame, ProgramNode programNode)
        {
            Symbols = symbols;
            GlobalScope = globalScope;
            GlobalFrame = globalFrame;
            
            Pass1_CreateLexicalScopes(programNode);
            Pass2_ComputeGlobalExtensions(programNode);
            Pass3_ResolveIdentifiers(programNode);
            Pass4_Finalize(programNode);
        }

        #region Pass 1: Create Lexical Scopes

        /// <summary>
        /// Pass 1. This pass creates lexical scopes for the program, modules and definitions,
        /// populating them with declarations and local parameters.
        /// </summary>
        void Pass1_CreateLexicalScopes(ProgramNode programNode)
        {
            Scopes = new();

            // Push global scope (<global> => GlobalScope).
            // Global scope is shared and injected from the outside.
            Scopes.Push((GlobalScope.Alias, GlobalScope));

            Pass1_Recurse(programNode);

            // Pop global scope.
            Scopes.Pop();
        }

        void Pass1_Recurse(ISyntaxNode node)
        {
            switch (node)
            {
                // Significative cases.
                case ProgramNode programNode: Pass1_RecurseProgram(programNode); break;
                case ModuleNode moduleNode: Pass1_RecurseModule(moduleNode); break;
                case DefinitionNode definitionNode: Pass1_RecurseDefinition(definitionNode); break;
                case VarNode varNode: Pass1_RecurseVar(varNode); break;

                // Trivial cases.
                case AccessNode: break;
                case ApplyNode: break;
                case CastNode: break;
                case CloseNode: break;
                case ConditionalNode: break;
                case IndexNode: break;
                case SetNode: break;
                case TupleNode: break;
                case NilNode: break;
                case DecimalNode: break;
                case IntegerNode: break;
                case WildcardNode: break;
                case IdentifierNode: break;

                // Warning: var inside sequences is not supported. Todo fix grammar.
                case SequenceNode: break; 

                default: 
                    throw new NotSupportedException($"Analyzer :: Pass 1 :: Node type {node.GetType().Name} is not supported.");
            };

            void Pass1_RecurseProgram(ProgramNode programNode)
            {
                // Create a new program scope.
                programNode.Scope = new LexicalScope(Context.SymbolProgram, LexicalScopeKind.Singleton, GlobalScope);

                // Push program scope (<program> => ProgramScope).
                Scopes.Push((programNode.Scope.Alias, programNode.Scope));

                foreach (var instruction in programNode.Instructions)
                    Pass1_Recurse(instruction);

                // Pop program scope.
                Scopes.Pop();
            }

            // Todo: allow for redefinition/extension of modules ("partial" or namespace-like).
            void Pass1_RecurseModule(ModuleNode moduleNode)
            {
                // Create a new module signature and value.
                moduleNode.Signature = moduleNode.Name.Value;
                moduleNode.Value = new Module(moduleNode);

                // Create a new Singleton lesical scope for the module.
                var scopeAlias = Symbols.GetOrCreate($"{CurrentScopeAlias}/{moduleNode.Signature.String}");
                var moduleScope = new LexicalScope(scopeAlias, LexicalScopeKind.Singleton, CurrentScope);
                moduleNode.Scope = moduleScope;

                // Create a new declaration for the module.
                var declaration = new Declaration(DeclarationKind.Module, moduleNode.Signature, CurrentScope, moduleNode);

                // Bind the declaration to the parent scope.
                CurrentScope.Append(declaration);

                // Bind the declaration to the module scope as first item, to allow self-references.
                moduleScope.Append(declaration);

                // Push module scope.
                Scopes.Push((moduleNode.Signature, moduleScope));

                // Recurse.
                foreach (var instruction in moduleNode.Instructions)
                    Pass1_Recurse(instruction);

                // Pop module scope.
                Scopes.Pop();
            }

            // Todo: allow for redefinition in REPL mode (currently the first definition wins).
            void Pass1_RecurseDefinition(DefinitionNode definitionNode)
            {
                var definitionSignature = Context.SymbolTable.GetOrCreate(definitionNode.GetDefinitionSignature());

                if (CurrentScope.Lookup(definitionSignature, out var _, out var _))
                    throw new AlreadyDeclaredException($"Analyzer :: Pass 1 :: Symbol '{definitionSignature.String}' is already declared in the current scope '{CurrentScope.Alias.String}'.");

                // Create a new definition singature and value.
                definitionNode.Signature = definitionSignature;
                definitionNode.Value = new Definition(definitionNode);

                // Create a new Instance lexical scope for the definition.
                var scopeAlias = Symbols.GetOrCreate(CurrentScopeAlias + "/" + definitionNode.Signature.String);
                var localScope = new LexicalScope(scopeAlias, LexicalScopeKind.Instance, CurrentScope);
                definitionNode.Scope = localScope;

                // Create a new declaration for the definition.
                var declaration = new Declaration(DeclarationKind.Def, definitionNode.Signature, CurrentScope, definitionNode);

                // Bind the declaration to the parent scope.
                CurrentScope.Append(declaration);

                // Bind the declaration to the local scope as first item, to allow recursive calls.
                localScope.Append(declaration);

                // Append parameters to the local scope.
                if (definitionNode.Parameters != default)
                    foreach (var parameter in definitionNode.Parameters)
                    {
                        var parameterDeclaration = new Declaration(DeclarationKind.Param, parameter, localScope, definitionNode);
                        localScope.Append(parameterDeclaration);
                    }

                // Push the local scope.
                Scopes.Push((definitionNode.Signature, localScope));

                // Recurse.
                if (definitionNode.Expression != default)
                    Pass1_Recurse(definitionNode.Expression);

                // Pop the local scope.
                Scopes.Pop();

                // Build the runtime frame prototype.
                definitionNode.BuildFramePrototype();
            }

            // Todo: allow for redefinition in REPL mode (currently the first definition wins).
            void Pass1_RecurseVar(VarNode varNode)
            {
                if (CurrentScope.Lookup(varNode.Name.Value, out var _, out var _))
                    throw new AlreadyDeclaredException($"Analyzer :: Pass 1 :: Symbol '{varNode.Name.Value.String}' is already declared in the current scope '{CurrentScope.Alias.String}'.");

                // Create a declaration for the variable.
                var declaration = new Declaration(DeclarationKind.Var, varNode.Name.Value, CurrentScope, varNode);

                // Bind the declaration to the current scope.
                CurrentScope.Append(declaration);
            }
        }

        #endregion

        #region Pass 2: Compute Global Extensions

        // Here, local defintions are already mapped to a runtime frame (see BuildFramePrototype in pass 1).
        // Now we want to unify global/program and module scopes into a single, global array, to be accessible by index in O(1).
        // So we are only interested in Singleton scopes, which are mapped to a range into the global array (start, end).
        // The global array is expanded with every scope, and the start/end indices are stored in the scope.
        // At the end of the analysis phase (pass 4: Finalize), the global scope is expanded with the current program scope.

        LexicalScope GlobalScopeExtension;

        /// <summary>
        /// Pass 2. This pass computes the extension for the global scope. 
        /// Each module (including the program itself) is seen as an extension of the global scope,
        /// that will be merged into the global scope at the end of the analysis phase.
        /// </summary>
        void Pass2_ComputeGlobalExtensions(ProgramNode programNode)
        {
            // The scope extension exists only at analysis time.
            // By convention we use the Null symbol as an alias, the Singleton kind and no parent.
            GlobalScopeExtension = new LexicalScope(Context.SymbolNull, LexicalScopeKind.Singleton, default);
            Pass2_Recurse(programNode);
        }

        void Pass2_Recurse(ISyntaxNode node)
        {
            switch (node)
            {
                // Significative cases.
                case ModuleNode moduleNode:
                    // Includes ProgramNode and TestNode.
                    Pass2_RecurseModule(moduleNode); 
                    break;
                    
                // Trivial cases.
                case DefinitionNode: break;
                case VarNode: break;
                case AccessNode: break;
                case ApplyNode: break;
                case CastNode: break;
                case CloseNode: break;
                case ConditionalNode: break;
                case IndexNode: break;
                case SequenceNode: break;
                case SetNode: break;
                case TupleNode: break;
                case NilNode: break;
                case DecimalNode: break;
                case IntegerNode: break;
                case WildcardNode: break;
                case IdentifierNode: break;

                default:
                    throw new NotSupportedException($"Analyzer :: Pass 2 :: Node type {node.GetType().Name} is not supported.");
            };

            void Pass2_RecurseModule(ModuleNode moduleNode)
            {
                ExtendGlobalScope(moduleNode.Scope);

                foreach (var instruction in moduleNode.Instructions)
                    Pass2_Recurse(instruction);
            }

            /// <summary>
            /// This method extends the global scope with the extension scope.
            /// </summary>
            void ExtendGlobalScope(LexicalScope singletonScope)
            {
                if (singletonScope.Kind != LexicalScopeKind.Singleton)
                    throw new UnreachableException($"Analyzer :: Pass 2 :: Scope '{singletonScope.Alias.String}' is not a Singleton scope.");

                if (singletonScope.Count == 0)
                    return;

                // Note: the second operand (GlobalScopeExtension.Count) is necessary because the analyzer
                // may have added other modules to GlobalScopeExtension previously (multiple modules into a single file).
                var frameStart = GlobalScope.Count + GlobalScopeExtension.Count;
                singletonScope.GlobalFrameStart = frameStart;

                GlobalScopeExtension.AddRange(singletonScope);
            }
        }

        #endregion

        #region Pass 3: Resolve Identifiers and Calls

        void Pass3_ResolveIdentifiers(ProgramNode programNode)
        {
            // Push global scope.
            Scopes.Clear();
            Scopes.Push((GlobalScope.Alias, GlobalScope));

            // Recurse.
            Pass3_Recurse(programNode);
            
            // Pop global scope.
            Scopes.Pop();
        }

        void Pass3_Recurse(ISyntaxNode node)
        {
            switch (node)
            {
                // Significative cases.
                case IdentifierNode identifierNode: Pass3_RecurseIdentifier(identifierNode); break;
                case ApplyNode applyNode: Pass3_RecurseApply(applyNode); break;

                // Recursive cases.
                case ProgramNode programNode: Pass3_RecurseProgram(programNode); break;
                case ModuleNode moduleNode: Pass3_RecurseModule(moduleNode); break; // Includes TestNode.
                case DefinitionNode definitionNode: Pass3_RecurseDefinition(definitionNode); break;
                case VarNode varNode: Pass3_RecurseVar(varNode); break;
                case AccessNode accessNode: Pass3_RecurseAccess(accessNode); break;
                case CastNode castNode: Pass3_RecurseCast(castNode); break;
                case CloseNode closeNode: Pass3_RecurseClose(closeNode); break;
                case ConditionalNode conditionalNode: Pass3_RecurseConditional(conditionalNode); break;
                case IndexNode indexNode: Pass3_RecurseIndex(indexNode); break;
                case SequenceNode sequenceNode: Pass3_RecurseSequence(sequenceNode); break;
                case SetNode setNode: Pass3_RecurseSet(setNode); break;
                case TupleNode tupleNode: Pass3_RecurseTuple(tupleNode); break;

                // Trivial cases.
                case NilNode: break;
                case DecimalNode: break;
                case IntegerNode: break;
                case WildcardNode: break;
                
                default:
                    throw new NotSupportedException($"Analyzer :: Pass 3 :: Node type {node.GetType().Name} is not supported.");
            };

            void Pass3_RecurseIdentifier(IdentifierNode identifierNode)
            {
                if (!CurrentScope.RecursiveLookup(identifierNode.Value, out var _, out var targetScope, out var index))
                    // Identifier not found in the current scope: dynamic lookup (could be an open identifier).
                    identifierNode.ResolveToDynamic();
                else if (targetScope!.Kind == LexicalScopeKind.Instance)
                    // Identifier found in the current, local scope.
                    identifierNode.ResolveToLocal(index!.Value);
                else if (targetScope!.Kind == LexicalScopeKind.Singleton)
                    // Identifier found in the global scope.
                    identifierNode.ResolveToGlobal(targetScope.GlobalFrameStart!.Value + index!.Value);
                else 
                    throw new UnreachableException($"Analyzer :: Pass 3 :: Unsupported scope kind {targetScope!.Kind}");
            }

            void Pass3_RecurseApply(ApplyNode applyNode)
            {
                Pass3_Recurse(applyNode.Expression);

                if (applyNode.Expression is IdentifierNode identifierNode)
                {
                    if (identifierNode.Location == IdentifierLocation.Unresolved)
                        throw new UnreachableException($"Analyzer :: Pass 3 :: Unresolved apply identifier location.");

                    if (identifierNode.Location == IdentifierLocation.Local)
                    {
                        // Call target is a local parameter.
                        // We have an "abstract" signature: concrete signature can only be built at runtime
                        var signatureString = ApplyNode.GetApplySignature(identifierNode.Value, applyNode.Arguments);
                        var signatureSymbol = Context.SymbolTable.GetOrCreate($"<abstract>{signatureString}");
                        applyNode.Signature = signatureSymbol;

                        applyNode.ResolveToParametric(identifierNode.Index!.Value);
                    }
                    else
                    {
                        // Call target is a local definition, global definition or "open" name.
                        var signatureString = ApplyNode.GetApplySignature(identifierNode.Value, applyNode.Arguments);
                        var signatureSymbol = Context.SymbolTable.GetOrCreate(signatureString);
                        applyNode.Signature = signatureSymbol;

                        // Look for target signature in the scope chain.
                        if (!CurrentScope.RecursiveLookup(signatureSymbol, out var _, out var targetScope, out var index))
                            applyNode.ResolveToDynamic();
                        else if (targetScope!.Kind == LexicalScopeKind.Instance)
                            applyNode.ResolveToLocal(index!.Value);
                        else if (targetScope!.Kind == LexicalScopeKind.Singleton)
                            applyNode.ResolveToGlobal(targetScope.GlobalFrameStart!.Value + index!.Value);
                        else
                            throw new UnreachableException($"Analyzer :: Pass 3 :: Unsupported scope kind {targetScope!.Kind}.");
                    }
                }
                else
                    // Expression is not an identifier. This case is not yet implemented (see also Interpreter.EvalApply).
                    throw new UnreachableException($"Analyzer :: Pass 3 :: Apply expression is not an identifier.");

                if (applyNode.Arguments != default)
                    foreach (var argument in applyNode.Arguments)
                        Pass3_Recurse(argument);
            }

            void Pass3_RecurseProgram(ProgramNode programNode)
            {
                // Push program scope.
                Scopes.Push((programNode.Scope.Alias, programNode.Scope));

                foreach (var instruction in programNode.Instructions)
                    Pass3_Recurse(instruction);

                // Pop program scope.
                Scopes.Pop();
            }

            void Pass3_RecurseModule(ModuleNode moduleNode)
            {
                // Push module scope.
                Scopes.Push((moduleNode.Scope.Alias, moduleNode.Scope));

                foreach (var instruction in moduleNode.Instructions)
                    Pass3_Recurse(instruction);

                // Pop module scope.
                Scopes.Pop();
            }

            void Pass3_RecurseDefinition(DefinitionNode definitionNode)
            {
                // Push local scope.
                Scopes.Push((definitionNode.Scope.Alias, definitionNode.Scope));

                if (definitionNode.Expression != default)
                    Pass3_Recurse(definitionNode.Expression);
            
                // Pop local scope.
                Scopes.Pop();
            }

            void Pass3_RecurseVar(VarNode varNode)
            {
                // Resolve var name and location for initial assignment (var x = initial).
                Pass3_Recurse(varNode.Name);
                
                if (varNode.RValue != default)
                    Pass3_Recurse(varNode.RValue);
            }

            void Pass3_RecurseAccess(AccessNode accessNode)
            {
                Pass3_Recurse(accessNode.Expression);

                // No need to resolve the access Member: we always treat it as a symbol.
                // Pass3_Recurse(accessNode.Member);
            }

            void Pass3_RecurseCast(CastNode castNode)
            {
                Pass3_Recurse(castNode.Expression);
                Pass3_Recurse(castNode.Type);
            }

            void Pass3_RecurseClose(CloseNode closeNode)
            {
                Pass3_Recurse(closeNode.Expression);

                if (closeNode.Bindings == default)
                    return;

                foreach (var (name, expression) in closeNode.Bindings)
                {
                    // Todo: logic for shielded identifiers
                    // in Interpreter.EvalClose can be moved here.
                    Pass3_Recurse(name); 
                    Pass3_Recurse(expression);
                }
            }

            void Pass3_RecurseConditional(ConditionalNode conditionalNode)
            {
                Pass3_Recurse(conditionalNode.If);
                Pass3_Recurse(conditionalNode.Then);

                if (conditionalNode.Else != default)
                    Pass3_Recurse(conditionalNode.Else);
            }

            void Pass3_RecurseIndex(IndexNode indexNode)
            {
                Pass3_Recurse(indexNode.Expression);
                Pass3_Recurse(indexNode.Accessor);
            }

            void Pass3_RecurseSequence(SequenceNode sequenceNode)
            {
                Pass3_Recurse(sequenceNode.Expression);

                if (sequenceNode.Continuation != default)
                    Pass3_Recurse(sequenceNode.Continuation);
            }

            void Pass3_RecurseSet(SetNode setNode)
            {
                Pass3_Recurse(setNode.LValue);
                Pass3_Recurse(setNode.RValue);
            }

            void Pass3_RecurseTuple(TupleNode tupleNode)
            {
                if (tupleNode.Items != default)
                    foreach (var item in tupleNode.Items)
                        Pass3_Recurse(item);
            }
        }

        #endregion

        #region Pass 4: Finalize

        /// <summary>
        /// This method is called at the end of the analysis phase, and in particular after identifier resolution.
        /// </summary>
        void Pass4_Finalize(ProgramNode programNode)
        {
            GlobalScope.AddRange(GlobalScopeExtension);
            GlobalFrame.Extend(GlobalScopeExtension);
            programNode.Analyzed = true;
        }

        #endregion
    }
}