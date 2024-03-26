﻿using Chi.Lexing;
using Chi.Parsing.Syntax;
using Chi.Parsing.Syntax.Abstract;
using Chi.Shared;

namespace Chi.Parsing
{
    /// <summary>
    /// Implements a parser for the Chi language.
    /// </summary>
    /// <typeparam name="Language">The language type.</typeparam>
    /// <typeparam name="Token">The token type.</typeparam>
    /// <typeparam name="TokenType">The token type enumeration.</typeparam>
    public class Parser : Abstract.Parser<Language, Token, TokenType>
    {
        /// <summary>
        /// The current symbol table: a single lexical context shared across different evaluations.
        /// </summary>
        public readonly SymbolTable SymbolTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="source">The source tokens.</param>
        public Parser(SymbolTable symbolTable, Token[] source) : base(Language.Instance, source) =>
            SymbolTable = symbolTable;

        #region Grammar Helpers

        /// <summary>
        /// Gets a value indicating whether the current token group is a sequence disambiguator.
        /// </summary>
        bool IsDisambiguator =>
            Current.Type == TokenType.Colon && Next.Type == TokenType.Colon;

        /// <summary>
        /// The follow tokens of an expression.
        /// </summary>
        readonly TokenType[] FollowOfExpression = new TokenType[]
        {
            TokenType.Semicolon,    // ';' Program: next instruction. 
            TokenType.Comma,        // ',' Apply/Close/Tuple: next argument/item.
            TokenType.ParenClose,   // ')' Apply/Close: end of arguments.
            TokenType.BraceClose,   // '}' Tuple: end of items.
            TokenType.BrackClose,   // ']' Index: end of index.
            TokenType.Then,         // "then" Conditional: then branch.
            TokenType.Else,         // "else" Conditional: else branch.
            TokenType.With,         // "with" Close: end of expression.
            TokenType.Equal,        // '=' Set: end of L-Value.
            TokenType.EOF,          // Program termination.

            // TokenType.Colon,     // "::" Sequence disambiguator.
            // Colon is technically in the follow of Expression, but it shall not be
            // included here because it's manually handled by the expression parser
            // for the sequence disambiguation rule (see below).
        };

        #endregion

        #region Recursive Descent LL(k) Parser

        /// <summary>
        /// Runs the parser and returns the parsed program.
        /// </summary>
        /// <returns>The parsed program.</returns>
        public ProgramNode Run()
        {
            Reset();
            return ParseAxiom();
        }

        /// <summary>
        /// Parses the axiom rule.
        /// </summary>
        ProgramNode ParseAxiom()
        {
            var program = ParseProgram();
            Match(TokenType.EOF);
            return program;
        }

        /// <summary>
        /// Parses a program.
        /// </summary>
        /// <returns>The parsed program node.</returns>
        ProgramNode ParseProgram()
        {
            bool IsFollowOfProgram() =>
                // For now, TestNode has '}' in the follow of Program.
                Current.Type == TokenType.EOF || Current.Type == TokenType.BraceClose;

            var nodes = new List<ISyntaxNode>();

            while (!IsFollowOfProgram())
            {
                if (Current.Type == TokenType.Test)
                    nodes.Add(ParseTest());
                else if (Current.Type == TokenType.Def)
                    nodes.Add(ParseDefinition());
                else
                    nodes.Add(ParseExpression());

                if (!IsFollowOfProgram())
                    Match(TokenType.Semicolon);
            }

            return new ProgramNode(nodes);
        }

        /// <summary>
        /// Parses a test case.
        /// </summary>
        TestNode ParseTest()
        {
            Match(TokenType.Test);

            // Match Identifier.
            var token = Match(TokenType.Identifier);
            
            // Match '{'.
            Match(TokenType.BraceOpen);

            var program = ParseProgram();

            // Match '}'.
            Match(TokenType.BraceClose);

            var identifier = token.Text;
            var symbol = SymbolTable.Get(identifier);
            return new TestNode(symbol, program);
        }

        /// <summary>
        /// Parses a definition.
        /// </summary>
        /// <returns>The parsed definition node.</returns>
        DefinitionNode ParseDefinition()
        {
            Match(TokenType.Def);
            var (symbol, parameters) = ParseDefinitionSignature();

            IExpressionNode? expression = default;

            // Match definition expression (if any).
            if (Current.Type == TokenType.Equal)
            {
                // Match "=>".
                Match(TokenType.Equal);
                Match(TokenType.AngularClose);

                expression = ParseExpression();
            }

            return new DefinitionNode(symbol, parameters, expression);
        }

        /// <summary>
        /// Parses a definition signature.
        /// </summary>
        /// <returns>The parsed signature.</returns>
        (int identifier, List<int>? parameters) ParseDefinitionSignature()
        {
            // Match definition identifier.
            var token = Match(TokenType.Identifier);
            
            // Match definition parameters (zero or more).
            var parameters = ParseDefinitionParameters();

            var identifier = token.Text;
            var symbol = SymbolTable.Get(identifier);
            return (symbol, parameters);
        }

        /// <summary>
        /// Parses definition parameters.
        /// </summary>
        /// <returns>The parsed definition parameters.</returns>
        List<int>? ParseDefinitionParameters()
        {
            // No parameters (implicit). Terminal (Identifier) definition.
            if (Current.Type != TokenType.ParenOpen)
                return default;

            Match(TokenType.ParenOpen);

            var parameters = new List<int>();

            // No parameters (explicit). Non-terminal (Function) definition.
            if (Current.Type == TokenType.ParenClose)
            {
                Match(TokenType.ParenClose);
                return parameters;
            }

            do
            {
                // Match Identifier.
                var token = Match(TokenType.Identifier);
                var identifier = token.Text;
                var symbol = SymbolTable.Get(identifier);
                parameters.Add(symbol);

                // Match Comma, except for last parameter.
                if (Current.Type != TokenType.ParenClose)
                    Match(TokenType.Comma);
            }
            while (Current.Type != TokenType.ParenClose);

            Match(TokenType.ParenClose);
            return parameters;
        }

        /// <summary>
        /// Parses an expression.
        /// </summary>
        /// <returns>The parsed expression node.</returns>
        IExpressionNode ParseExpression()
        {
            IExpressionNode? expression;

            // Atoms rule.

            if (Current.Type == TokenType.Nil)
            {
                // Nil rule.
                Match(TokenType.Nil);
                expression = NilNode.Instance;
            }
            else if (Current.Type == TokenType.Integer)
            {
                // Integer number rule.
                var token = Match(TokenType.Integer);
                var text = token.Text;

                if (!int.TryParse(text, out var integerValue))
                    throw new LiteralException(TokenType.Integer, text);

                expression = new IntegerNode(integerValue);
            }
            else if (Current.Type == TokenType.Decimal)
            {
                // Decimal number rule.
                var token = Match(TokenType.Decimal);
                var text = token.Text;

                if (!decimal.TryParse(text, out var decimalValue))
                    throw new LiteralException(TokenType.Decimal, text);

                expression = new DecimalNode(decimalValue);
            }
            else if (Current.Type == TokenType.Underscore)
            {
                // Wildcard rule.
                Match(TokenType.Underscore);
                expression = WildcardNode.Instance;
            }
            else if (Current.Type == TokenType.Identifier)
            {
                // Identifier rule.
                var token = Match(TokenType.Identifier);
                var identifier = token.Text;
                var symbol = SymbolTable.Get(identifier);
                expression = new IdentifierNode(symbol);
            }
            else if (Current.Type == TokenType.ParenOpen)
            {
                // Precedence rule.
                Match(TokenType.ParenOpen);
                expression = ParseExpression();
                Match(TokenType.ParenClose);
            }
            else if (Current.Type == TokenType.If)
            {
                // If-Then-Else? rule.
                expression = ParseConditional();
            }
            else if (Current.Type == TokenType.Close)
            {
                // Close rule.
                expression = ParseClose();
            }
            else if (Current.Type == TokenType.BraceOpen)
            {
                // Tuple rule.
                expression = new TupleNode(ParseTupleItems());
            }
            else if (Current.Type == TokenType.Var)
            {
                // Var rule.
                Match(TokenType.Var);
                Match(TokenType.ParenOpen);

                var token = Match(TokenType.Identifier);
                var rValue = default(IExpressionNode?);

                // Match optional value assignment.
                if (Current.Type == TokenType.Equal)
                {
                    Match(TokenType.Equal);
                    rValue = ParseExpression();
                }

                Match(TokenType.ParenClose);

                var identifier = token.Text;
                var symbol = SymbolTable.Get(identifier);
                expression = new VarNode(symbol, rValue);
            }
            else if (Current.Type == TokenType.Set)
            {
                // Set rule.
                Match(TokenType.Set);
                Match(TokenType.ParenOpen);

                var lValue = ParseExpression();
                Match(TokenType.Equal);
                var rValue = ParseExpression();
                
                Match(TokenType.ParenClose);
                expression = new SetNode(lValue, rValue);
            }
            else
                throw Predict("Expression First (Continuation) or Follow (End)");

            // Compound continuation rule (access, apply, cast operations).

            while (Current.Type == TokenType.Dot || Current.Type == TokenType.ParenOpen || Current.Type == TokenType.Colon)
            {
                if (Current.Type == TokenType.Dot)
                {
                    // Access rule.
                    Match(TokenType.Dot);

                    if (Current.Type == TokenType.BrackOpen)
                    {
                        // Index access rule (x.[y]).
                        Match(TokenType.BrackOpen);
                        expression = new IndexNode(expression, ParseExpression());
                        Match(TokenType.BrackClose);
                    }
                    else
                    {
                        // Member access rule (x.y).
                        var token = Match(TokenType.Identifier);
                        var identifier = token.Text;
                        var symbol = SymbolTable.Get(identifier);
                        expression = new AccessNode(expression, symbol);
                    }
                }
                else if (Current.Type == TokenType.ParenOpen)
                {
                    // Apply rule.
                    expression = new ApplyNode(expression, ParseApplyArguments());
                }
                else if (Current.Type == TokenType.Colon)
                {
                    // Sequence disambiguator: not a cast.
                    if (Next.Type == TokenType.Colon)
                        break;

                    // Cast rule.
                    Match(TokenType.Colon);
                    expression = new CastNode(expression, ParseExpression());
                }
            }

            // Sequence rule.

            // If the next token is in follow of Expression (like ; ) } ,)
            // then we return the parsed expression with no continuation.
            if (FollowOfExpression.Contains(Current.Type))
                return expression;

            // Sequence disambiguator rule.
            // Eg: a (b) is an application; a :: (b) is a sequence.
            if (IsDisambiguator)
            {
                // Just match the colons and continue parsing.
                Match(TokenType.Colon);
                Match(TokenType.Colon);
            }

            // Parse the sequence continuation.
            var continuation = ParseExpression();
            return new SequenceNode(expression, continuation);
        }

        /// <summary>
        /// Parses apply arguments.
        /// </summary>
        /// <returns>The parsed apply arguments.</returns>
        IList<IExpressionNode>? ParseApplyArguments()
        {
            Match(TokenType.ParenOpen);

            // No arguments.
            if (Current.Type == TokenType.ParenClose)
            {
                Match(TokenType.ParenClose);
                return default;
            }

            var arguments = new List<IExpressionNode>();
            do
            {
                var expression = ParseExpression();
                arguments.Add(expression);

                if (Current.Type != TokenType.ParenClose)
                    Match(TokenType.Comma);
            }
            while (Current.Type != TokenType.ParenClose);

            Match(TokenType.ParenClose);
            return arguments;
        }

        /// <summary>
        /// Parses tuple items.
        /// </summary>
        /// <returns>The parsed tuple items.</returns>
        IList<IExpressionNode>? ParseTupleItems()
        {
            Match(TokenType.BraceOpen);

            // No items.
            if (Current.Type == TokenType.BraceClose)
            {
                Match(TokenType.BraceClose);
                return default;
            }

            var items = new List<IExpressionNode>();
            do
            {
                var expression = ParseExpression();
                items.Add(expression);

                if (Current.Type != TokenType.BraceClose)
                    Match(TokenType.Comma);
            }
            while (Current.Type != TokenType.BraceClose);

            Match(TokenType.BraceClose);
            return items;
        }

        /// <summary>
        /// Parses a conditional expression.
        /// </summary>
        /// <returns>The parsed conditional node.</returns>
        ConditionalNode ParseConditional()
        {
            Match(TokenType.If);
            var @if = ParseExpression();

            Match(TokenType.Then);
            var then = ParseExpression();

            var @else = default(IExpressionNode?);
            
            // Match optional else branch.
            if (Current.Type == TokenType.Else)
            {
                Match(TokenType.Else);
                @else = ParseExpression();
            }

            return new ConditionalNode(@if, then, @else);
        }

        /// <summary>
        /// Parses a close expression.
        /// </summary>
        /// <returns>The parsed close node.</returns>
        CloseNode ParseClose()
        {
            Match(TokenType.Close);

            var closedExpression = ParseExpression();

            Match(TokenType.With);
            Match(TokenType.ParenOpen);

            // No bindings.
            if (Current.Type == TokenType.ParenClose)
            {
                Match(TokenType.ParenClose);
                return new CloseNode(closedExpression, default);
            }

            var bindings = new List<(int symbol, IExpressionNode expression)>();
            do
            {
                // Shielded identifers are only supported for close parameters for now.
                // (see Interpreter.EvalClose for the actual implementation).

                bool shielded = false;
                if (Current.Type == TokenType.Dollar)
                {
                    Match(TokenType.Dollar);
                    shielded = true;
                }

                var token = Match(TokenType.Identifier);
                Match(TokenType.Equal);
                var argumentExpression = ParseExpression();

                var identifier = (shielded ? "$" : "") + token.Text;
                var symbol = SymbolTable.Get(identifier);
                bindings.Add((symbol, argumentExpression));

                if (Current.Type != TokenType.ParenClose)
                    Match(TokenType.Comma);
            }
            while (Current.Type != TokenType.ParenClose);

            Match(TokenType.ParenClose);
            return new CloseNode(closedExpression, bindings);
        }

        #endregion

        #region Print Helpers

        public void Print(ISyntaxNode node) =>
            PrintTree(node);

        private void PrintTree(ISyntaxNode node, bool isSequenceMiddle = false)
        {
            int i;
            switch (node)
            {
                case ProgramNode programNode:
                    Output.Write($"Program {{ ", ConsoleColor.Green);

                    i = 0;
                    foreach (var instruction in programNode.Instructions)
                    {
                        Output.Write($"Instruction #{i} {{ ", ConsoleColor.Green);
                        PrintTree(instruction);
                        Output.Write($" }} ", ConsoleColor.Green);

                        if (i < programNode.Instructions.Count - 1)
                            Output.Write($" ; ", ConsoleColor.Green);
                        i++;
                    }

                    Output.Write($" }} ", ConsoleColor.Green);
                    break;

                case TestNode testNode:
                    Output.Write($"Test {SymbolTable.Get(testNode.Symbol)} {{ ", ConsoleColor.Blue);
                    PrintTree(testNode.Program);
                    Output.Write($" }} ", ConsoleColor.Blue);
                    break;

                case WildcardNode:
                    Output.Write("[Wildcard]", ConsoleColor.Green);
                    break;

                case NilNode:
                    Output.Write("[Nil]", ConsoleColor.Blue);
                    break;

                case IntegerNode integerNode:
                    Output.Write($"[Integer {integerNode.Value}]", ConsoleColor.Cyan);
                    break;

                case DecimalNode decimalNode:
                    Output.Write($"[Decimal {decimalNode.Value}]", ConsoleColor.Cyan);
                    break;

                case IdentifierNode identifierNode:
                    Output.Write($"[Identifier {SymbolTable.Get(identifierNode.Value)}]", ConsoleColor.White);
                    break;

                case DefinitionNode definitionNode:
                    var parameterList = definitionNode.Parameters != default ? 
                        string.Join(",", definitionNode.Parameters.Select(p => SymbolTable.Get(p))) : 
                        string.Empty;
                    
                    Output.Write($"Definition {SymbolTable.Get(definitionNode.Symbol)}({parameterList}) {{ ", ConsoleColor.Blue);

                    if (definitionNode.Expression != default)
                        PrintTree(definitionNode.Expression);
                    else
                        Output.Write($"<No-Expression>", ConsoleColor.DarkBlue);

                    Output.Write($" }} ", ConsoleColor.Blue);
                    break;

                case SequenceNode sequenceNode:
                    if (!isSequenceMiddle)
                        Output.Write($"Sequence {{ ", ConsoleColor.Red);

                    PrintTree(sequenceNode.Expression, isSequenceMiddle = true);
                    Output.Write($" :: ", ConsoleColor.Red);

                    if (sequenceNode.Continuation != default)
                        PrintTree(sequenceNode.Continuation, isSequenceMiddle = true);
                    else 
                        Output.Write($"<No-Continuation>", ConsoleColor.DarkRed);

                    if (!isSequenceMiddle)
                        Output.Write($" }} ", ConsoleColor.Red);
                    break;

                case TupleNode tupleNode:
                    Output.Write($"Tuple {{ ", ConsoleColor.Yellow);

                    if (tupleNode.Items != default)
                    {
                        i = 0;
                        foreach (var item in tupleNode.Items)
                        {
                            Output.Write($"Item #{i} {{ ", ConsoleColor.Yellow);
                            PrintTree(item);
                            Output.Write($" }} ", ConsoleColor.Yellow);

                            if (i < tupleNode.Items.Count - 1)
                                Output.Write($" ; ", ConsoleColor.Yellow);
                            i++;
                        }
                    }
                    else
                        Output.Write($"<No-Items>", ConsoleColor.DarkYellow);

                    Output.Write($" }} ", ConsoleColor.Yellow);
                    break;

                case ApplyNode applyNode:
                    Output.Write($"Apply {{ ", ConsoleColor.Magenta);

                    Output.Write($"Expression {{ ", ConsoleColor.Magenta);
                    PrintTree(applyNode.Expression);
                    Output.Write($" }} ", ConsoleColor.Magenta);

                    Output.Write($"Arguments {{ ", ConsoleColor.Magenta);
                    if (applyNode.Arguments != default)
                    {
                        i = 0;
                        foreach (var argument in applyNode.Arguments)
                        {
                            Output.Write($"Argument #{i} {{ ", ConsoleColor.Magenta);
                            PrintTree(argument);
                            Output.Write($" }} ", ConsoleColor.Magenta);

                            if (i < applyNode.Arguments.Count - 1)
                                Output.Write($" , ", ConsoleColor.Magenta);
                            i++;
                        }
                    }
                    else
                        Output.Write($"<No-Arguments>", ConsoleColor.DarkMagenta);

                    Output.Write($" }} ", ConsoleColor.Magenta);
                    break;

                case CloseNode closeNode:
                    Output.Write($"Close {{ ", ConsoleColor.Magenta);

                    Output.Write($"Expression {{ ", ConsoleColor.Magenta);
                    PrintTree(closeNode.Expression);
                    Output.Write($" }} ", ConsoleColor.Magenta);

                    Output.Write($"Bindings {{ ", ConsoleColor.Magenta);

                    if (closeNode.Bindings != default)
                    {
                        i = 0;
                        foreach (var (identifier, expression) in closeNode.Bindings)
                        {
                            Output.Write($"Binding #{i}:{SymbolTable.Get(identifier)} {{ ", ConsoleColor.Magenta);
                            PrintTree(expression);
                            Output.Write($" }} ", ConsoleColor.Magenta);

                            if (i < closeNode.Bindings.Count - 1)
                                Output.Write($" , ", ConsoleColor.Magenta);
                            i++;
                        }
                    }
                    else
                        Output.Write($"<No-Bindings>", ConsoleColor.DarkMagenta);

                    Output.Write($" }} ", ConsoleColor.Magenta);
                    break;

                case ConditionalNode conditionalNode:
                    Output.Write($"Conditional {{ ", ConsoleColor.Blue);
                    Output.Write($"If {{ ", ConsoleColor.Blue);
                    PrintTree(conditionalNode.If);
                    Output.Write($" }} ", ConsoleColor.Blue);
                    Output.Write($"Then {{ ", ConsoleColor.Blue);
                    PrintTree(conditionalNode.Then);
                    Output.Write($" }} ", ConsoleColor.Blue);
                    if (conditionalNode.Else != default)
                    {
                        Output.Write($"Else {{ ", ConsoleColor.Blue);
                        PrintTree(conditionalNode.Else);
                        Output.Write($" }} ", ConsoleColor.Blue);
                    }
                    else
                        Output.Write($"<No-Else>", ConsoleColor.DarkBlue);
                    Output.Write($" }} ", ConsoleColor.Blue);
                    break;

                case AccessNode accessNode:
                    Output.Write($"Access {{ ", ConsoleColor.Magenta);
                    Output.Write($"Expression {{ ", ConsoleColor.Magenta);
                    PrintTree(accessNode.Expression);
                    Output.Write($" }} ", ConsoleColor.Magenta);
                    Output.Write($"Member: ", ConsoleColor.Magenta);
                    Output.Write($"[Identifier {SymbolTable.Get(accessNode.Symbol)}]", ConsoleColor.White);
                    Output.Write($" }} ", ConsoleColor.Magenta);
                    break;

                case IndexNode indexNode:
                    Output.Write($"Index {{ ", ConsoleColor.Magenta);
                    Output.Write($"Expression {{ ", ConsoleColor.Magenta);
                    PrintTree(indexNode.Expression);
                    Output.Write($" }} ", ConsoleColor.Magenta);
                    Output.Write($"Accessor {{ ", ConsoleColor.Magenta);
                    PrintTree(indexNode.Accessor);
                    Output.Write($" }} ", ConsoleColor.Magenta);
                    Output.Write($" }} ", ConsoleColor.Magenta);
                    break;

                case SetNode setNode:
                    Output.Write($"Set {{ ", ConsoleColor.Blue);
                    Output.Write($"L-Value {{ ", ConsoleColor.Blue);
                    PrintTree(setNode.LValue);
                    Output.Write($" }} ", ConsoleColor.Blue);
                    Output.Write($"R-Value {{ ", ConsoleColor.Blue);
                    PrintTree(setNode.RValue);
                    Output.Write($" }} ", ConsoleColor.Blue);
                    Output.Write($" }} ", ConsoleColor.Blue);
                    break;

                case CastNode castNode:
                    Output.Write($"Cast {{ ", ConsoleColor.Magenta);
                    Output.Write($"Expression {{ ", ConsoleColor.Magenta);
                    PrintTree(castNode.Expression);
                    Output.Write($" }} ", ConsoleColor.Magenta);
                    Output.Write($"Type {{ ", ConsoleColor.Magenta);
                    PrintTree(castNode.Type);
                    Output.Write($" }} ", ConsoleColor.Magenta);
                    Output.Write($" }} ", ConsoleColor.Magenta);
                    break;

                default:
                    throw new NotSupportedException($"{nameof(Output)}.{nameof(Parser)}: Unsupported node type {node.GetType().Name}.");
            }
        }

        #endregion
    }
}
