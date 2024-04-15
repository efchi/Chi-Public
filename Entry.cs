using Chi.Shared;
using Chi.Infra;
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
                Context.Reload(isRepl: true);
                
                Output.WriteLine($"{Settings.LanguageName} version {Assembly.GetExecutingAssembly().GetName().Version}",
                    ConsoleColor.White, ConsoleColor.DarkBlue);

                Output.WriteLine("Chi is Ready! Use /usage for more info", ConsoleColor.Yellow);
                Output.WriteLine();

                // REPL Loop.
                Context.REPL!.Loop();
            }
            else
            {
                // Run source in File mode.
                Context.Reload(isRepl: false);

                // Read file.
                var workingPath = Directory.GetCurrentDirectory();
                var filename = args[0];
                var filepath = Path.Combine(workingPath, filename);
                var source = File.ReadAllText(filepath);

                // Process file.

                Workflow.Load(source);
                Workflow.Lex(print: false);
                Workflow.Postprocess(print: false);
                Workflow.Parse(print: false);
                Workflow.Analyze(print: false);
                Workflow.Run(verbose: false, printResult: true, printElapsed: false);
                
                // Signal the Output thread that the program terminated.
                // The Output thread will write the remaining output and then signal back when finished.
                Output.SignalExit();

                // Wait until the Output thread signal.
                OutputWaitHandle.WaitOne();
            }
        }
    }
}
