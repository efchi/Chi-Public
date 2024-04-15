using Chi.Lexing;
using Chi.Parsing;
using Chi.Parsing.Data;
using Chi.Parsing.Syntax;
using Chi.Runtime;
using Chi.Runtime.Abstract;
using Chi.Runtime.Data;
using Chi.Runtime.Values;
using Chi.Semanting;

namespace Chi.Infra
{
    /// <summary>
    /// A singleton class that holds the state of the Chi runtime.
    /// The Context contains the workflow modules instances (Lexer, Parser, Analyzer, Interpreter, REPL)
    /// along with the global symbol table, the global scope and the shared memory, plus some common data.
    /// We chose this pattern because it's a convenient place to perform critical load/reload operations.
    /// </summary>
    public static class Context
    {
        public static class Options
        {
            // Used to turn sequence optimization on and off for testing purposes (see Interpreter).
            public const bool OptimizeSequences = true;
        }

        // Workflow Modules
        public static Lexer Lexer { get; private set; }
        public static Postprocessor Postprocessor { get; private set; }
        public static Parser Parser { get; private set; }
        public static Analyzer Analyzer { get; private set; }
        public static IInterpreter Interpreter { get; private set; }
        public static REPL? REPL { get; private set; }

        // Runtime State
        public static bool IsREPL { get; private set; }
        public static SymbolTable SymbolTable { get; private set; } = new();
        public static LexicalScope GlobalScope { get; private set; }
        public static MemoryFrame GlobalFrame { get; private set; }

        // Common Data
        public static Symbol SymbolOK { get; private set; }
        public static Symbol SymbolKO { get; private set; }
        public static Symbol SymbolNull { get; private set; }
        public static Symbol SymbolGlobal { get; private set; }
        public static Symbol SymbolProgram { get; private set; }
        public static IdentifierNode IdentifierProgram { get; private set; }
        public static Open OpenOK { get; private set; }
        public static Open OpenKO { get; private set; }
        
        /// <summary>
        /// Resets the Context to its initial state.
        /// </summary>
        /// <param name="isRepl">Indicates whether Chi is running in REPL mode.</param>
        public static void Reload(bool isRepl)
        {
            // Workflow Modules

            Workflow.Reload();

            Lexer = new Lexer();
            Postprocessor = new Postprocessor();
            Parser = new Parser();
            Analyzer = new Analyzer();
            Interpreter = new Interpreter();

            IsREPL = isRepl;
            if (IsREPL) REPL = new REPL();

            // Symbol Table, Common Data

            SymbolTable = new SymbolTable();
            SymbolOK = SymbolTable.GetOrCreate("OK");
            SymbolKO = SymbolTable.GetOrCreate("KO");
            SymbolNull = SymbolTable.GetOrCreate("<null>");
            SymbolGlobal = SymbolTable.GetOrCreate("<global>");
            SymbolProgram = SymbolTable.GetOrCreate("<program>");
            IdentifierProgram = new IdentifierNode(SymbolProgram);
            OpenOK = new Open(SymbolOK);
            OpenKO = new Open(SymbolKO);

            // Runtime State

            // Creating the global scope.
            GlobalScope = new LexicalScope(SymbolGlobal, LexicalScopeKind.Singleton);

            // Registering primitives into the symbol table and the global scope.
            Primitives.Register(SymbolTable, GlobalScope);
            
            // Creating the static memory frame of our global scope.
            GlobalFrame = new(GlobalScope);
            
            // Initializing and running startup files.
            Startup.Run();

            // Cleanup after running the startup files.
            Workflow.Reload();
            Startup.Cleanup();
            GC.Collect();
        }
    }
}
