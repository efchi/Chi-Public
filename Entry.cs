using Chi.Runtime;
using Chi.Shared;
using Chi.Infra;
using System.Reflection;
using Chi.Runtime.Abstract;

namespace Chi
{
    internal class Entry
    {
        // This is a signal to the main thread that the background thread has finished writing all the output.
        public static EventWaitHandle OutputWaitHandle { get; } = new(false, EventResetMode.ManualReset);

#pragma warning disable CS8618
        // Main interpreter and symbol table instances. We make them static for debug purposes.
        static IInterpreter Interpreter;
        static SymbolTable Symbols;
#pragma warning restore CS8618

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                // Run in REPL mode.
                
                Output.WriteLine($"{Settings.LanguageName} version {Assembly.GetExecutingAssembly().GetName().Version}",
                    ConsoleColor.White, ConsoleColor.DarkBlue);

                // Startup.
                Symbols = new SymbolTable();
                Startup.Initialize(Symbols, startupVerbose: true);
                
                // Create interpreter and loop.
                Interpreter = new Interpreter(Symbols, repl: true);
                Output.WriteLine("Chi is Ready! Use /usage for more info", ConsoleColor.Yellow);
                Output.WriteLine();
                new REPL(Interpreter).Loop();
            }
            else
            {
                // Run source in File mode.

                // Startup.
                Symbols = new SymbolTable();
                Startup.Initialize(Symbols, startupVerbose: false);

                // Read and process file.
                var workingPath = Directory.GetCurrentDirectory();
                var filename = args[0];
                var filepath = Path.Combine(workingPath, filename);
                var source = File.ReadAllText(filepath);
                var tokens = Workflow.Lex(source, ignoreWhitespace: true, ignoreComments: true, print: false, verbose: false);
                tokens = Workflow.Postprocess(source, tokens, print: false, verbose: false);

                // Parse, create interpreter and run.
                var ast = Workflow.Parse(Symbols, tokens.ToArray(), print: false);
                Interpreter = new Interpreter(Symbols, repl: false);
                Workflow.Run(Interpreter, ast, print: true, verbose: false);
                
                // Signal the Output thread that the program terminated.
                // The Output thread will write the remaining output and then signal back when finished.
                Output.SignalExit();

                // Wait until the Output thread signal.
                OutputWaitHandle.WaitOne();
            }
        }
    }
}
