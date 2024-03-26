using Chi.Runtime;
using Chi.Shared;
using System.Reflection;

namespace Chi
{
    internal class Entry
    {
        // This is a signal to the main thread that the background thread has finished writing all the output.
        public static EventWaitHandle OutputWaitHandle { get; } = new(false, EventResetMode.ManualReset);

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                // Run in REPL mode.
                
                Output.WriteLine($"{Settings.LanguageName} version {Assembly.GetExecutingAssembly().GetName().Version}",
                    ConsoleColor.White, ConsoleColor.DarkBlue);

                // Startup.
                var symbols = new SymbolTable();
                Startup.Initialize(symbols, startupVerbose: true);
                
                // Create interpreter and loop.
                var interpreter = new Interpreter(symbols, repl: true);
                Output.WriteLine("Chi is Ready! Use /usage for more info", ConsoleColor.Yellow);
                Output.WriteLine();
                new REPL(interpreter).Loop();
            }
            else
            {
                // Run source in File mode.

                // Startup.
                var symbols = new SymbolTable();
                Startup.Initialize(symbols, startupVerbose: false);

                // Read and process file.
                var workingPath = Directory.GetCurrentDirectory();
                var filename = args[0];
                var filepath = Path.Combine(workingPath, filename);
                var source = File.ReadAllText(filepath);
                var tokens = Workflow.Lex(source, ignoreWhitespace: true, ignoreComments: true, print: false, verbose: false);
                tokens = Workflow.Postprocess(source, tokens, print: false, verbose: false);

                // Parse, create interpreter and run.
                var ast = Workflow.Parse(symbols, tokens.ToArray(), print: false);
                var interpreter = new Interpreter(symbols, repl: false);
                Workflow.Run(interpreter, ast, print: true, verbose: false);
                
                // Signal the Output thread that the program terminated.
                // The Output thread will write the remaining output and then signal back when finished.
                Output.SignalExit();

                // Wait until the Output thread signal.
                OutputWaitHandle.WaitOne();
            }
        }
    }
}
