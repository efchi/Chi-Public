using Chi.Shared;
using Chi.Parsing.Syntax;
using System.Diagnostics;
using Chi.Infra;

namespace Chi.Runtime
{
    internal static class Startup
    {
        /// <summary>
        /// List of files and their corresponding ASTs.
        /// </summary>
        public static readonly List<(string filename, ProgramNode ast)> Files = new();

        /// <summary>
        /// Initializes the startup process by loading files in the .startup directory.
        /// </summary>
        public static void Initialize(SymbolTable symbolTable, bool startupVerbose)
        {
            const bool ignoreWhitespace = true;
            const bool ignoreComments = true;
            const bool print = false;
            const bool verbose = false;

            var stopwatch = Stopwatch.StartNew();
            var paths = Directory.GetFiles(Paths.StartupPath, $"*{Settings.LanguageExtension}");

            if (startupVerbose)
                Output.WriteLine($"Startup :: Preloading {paths.Length} files", ConsoleColor.Yellow);

            int passed = 0,
                failed = 0;

            foreach (var path in paths)
            {
                string? filename = default;
                try
                {
                    filename = Path.GetFileName(path);
                    
                    if (startupVerbose)
                        Output.WriteLine($"Startup :: Preloading {filename}", ConsoleColor.DarkYellow);

                    var source = File.ReadAllText(path);
                    var tokens = Workflow.Lex(source, ignoreWhitespace, ignoreComments, print, verbose);
                    tokens = Workflow.Postprocess(source, tokens, print, verbose);
                    var ast = Workflow.Parse(symbolTable, tokens.ToArray(), print);

                    Files.Add((Path.GetFileName(path), ast));
                    passed++;
                }
                catch (Exception ex)
                {
                    Output.WriteLine($"Startup :: Failed to preload {filename ?? path} :: {ex.Message}", ConsoleColor.Red);
                    failed++;
                }
            }
            stopwatch.Stop();

            if (startupVerbose)
            {
                Output.WriteLine($"Startup :: Ran in {stopwatch.Elapsed.TotalMilliseconds}ms", ConsoleColor.DarkGray);
                Output.WriteLine($"Startup :: {passed} OK, {failed} KO", ConsoleColor.Black, failed == 0 ? ConsoleColor.Green : ConsoleColor.Red);
                Output.WriteLine();
            }
        }
    }
}
