using Chi.Shared;
using Chi.Parsing.Syntax;
using System.Diagnostics;

namespace Chi.Infra
{
    /// <summary>
    /// This class is responsible for storing and running the startup files.
    /// </summary>
    internal static class Startup
    {
        /// <summary>
        /// List of startup files inside ./sources with their corresponding contents.
        /// </summary>
        static List<(string path, string source)>? Sources { get; set; }

        /// <summary>
        /// List of startup files inside ./sources with their corresponding ASTs.
        /// </summary>
        static List<(string filename, ProgramNode ast)>? ASTs { get; set; }

        static bool FirstRun = true;

        /// <summary>
        /// Run all startup files.
        /// This method should be called evey time the Context is reloaded.
        /// </summary>
        public static void Run()
        {
            if (Sources == default)
                Initialize();

            // Run in verbose mode: REPL mode and first run.
            var verbose = Context.IsREPL && FirstRun;
            FirstRun = false;

            // Unlike file sources, startup ASTs are never cached (symbol table may change).
            ASTs = new();

            var stopwatch = Stopwatch.StartNew();
            var paths = Directory.GetFiles(Paths.StartupPath, $"*{Settings.LanguageExtension}");

            if (verbose)
                Output.WriteLine($"Startup :: Preload :: Preloading {paths.Length} files", ConsoleColor.Yellow);

            int passed = 0,
                failed = 0;

            foreach (var (path, source) in Sources!)
            {
                string? filename = default;
                try
                {
                    filename = Path.GetFileName(path);

                    if (verbose)
                        Output.WriteLine($"Startup :: Preload :: Preloading {filename}", ConsoleColor.DarkYellow);

                    Workflow.Load(source);
                    Workflow.Lex(print: false);
                    Workflow.Postprocess(print: false);
                    Workflow.Parse(print: false);
                    Workflow.Analyze(print: false);

                    // Keeping AST for debugging purposes only.
                    var ast = Workflow.CurrentAST!;
                    ASTs!.Add((filename, ast));

                    Workflow.Run(verbose: false, printResult: false, printElapsed: false);
                    passed++;
                }
                catch (Exception ex)
                {
                    Output.WriteLine($"Startup :: Preload :: Failed to preload {filename ?? path} :: {ex.Message}", ConsoleColor.Red);
                    failed++;
                }
            }
            stopwatch.Stop();

            if (verbose)
            {
                Output.WriteLine($"Startup :: Preload :: Ran in {stopwatch.Elapsed.TotalMilliseconds}ms", ConsoleColor.DarkGray);
                Output.WriteLine($"Startup :: Preload :: {passed} OK, {failed} KO", ConsoleColor.Black, failed == 0 ? ConsoleColor.Green : ConsoleColor.Red);
                Output.WriteLine();
            }
        }

        /// <summary>
        /// Read and cache startup files, to improve performance during multiple tests.
        /// This step is performed only once: subsequent calls will be ignored.
        /// </summary>
        static void Initialize()
        {
            // Ensure that Initialize is called only once.
            if (Sources != default)
                return;

            Sources = new();

            var stopwatch = Stopwatch.StartNew();
            var paths = Directory.GetFiles(Paths.StartupPath, $"*{Settings.LanguageExtension}");

            if (Context.IsREPL)
                Output.WriteLine($"Startup :: Initialize :: Reading {paths.Length} source files", ConsoleColor.Yellow);

            int passed = 0,
                failed = 0;

            foreach (var path in paths)
            {
                string? filename = default;
                try
                {
                    filename = Path.GetFileName(path);

                    if (Context.IsREPL)
                        Output.WriteLine($"Startup :: Initialize :: Reading {filename}", ConsoleColor.DarkYellow);

                    var source = File.ReadAllText(path);
                    Sources!.Add((path, source));
                    passed++;
                }
                catch (Exception ex)
                {
                    Output.WriteLine($"Startup :: Initialize :: Failed to read source file {filename ?? path} :: {ex.Message}", ConsoleColor.Red);
                    failed++;
                }
            }
            stopwatch.Stop();

            if (Context.IsREPL)
            {
                Output.WriteLine($"Startup :: Initialize :: Ran in {stopwatch.Elapsed.TotalMilliseconds}ms", ConsoleColor.DarkGray);
                Output.WriteLine($"Startup :: Initialize :: {passed} OK, {failed} KO", ConsoleColor.Black, failed == 0 ? ConsoleColor.Green : ConsoleColor.Red);
                Output.WriteLine();
            }
        }

        public static void Cleanup()
        {            
            // Clear startup ASTs (eventually Context will invoke GC).
            // Sources are kept for future reloads. 
            ASTs = default;
        }
    }
}
