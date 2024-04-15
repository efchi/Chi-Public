using Chi.Lexing;
using Chi.Parsing;
using Chi.Parsing.Data;
using Chi.Parsing.Syntax;
using Chi.Runtime;
using Chi.Runtime.Values;
using Chi.Runtime.Values.Abstract;
using Chi.Shared;
using System.Diagnostics;

namespace Chi.Infra
{
    /// <summary>
    /// This class is responsible for managing the language workflow in a step-by-step manner.
    /// </summary>
    internal static class Workflow
    {
        // I/O settings for Workflow methods, configurable by REPL using the /option command.
        public static bool IgnoreWhitespace = true;
        public static bool IgnoreComments = true;
        public static bool VerboseTokens = false;
        public static bool VerboseValues = true;

        // Intermediate Workflow results.
        public static string? CurrentSource { get; internal set; }
        public static IList<Token>? CurrentTokens { get; internal set; }
        public static ProgramNode? CurrentAST { get; internal set; }
        public static IValueNode CurrentResult { get; internal set; }

        static Workflow() =>
            Reload();

        #region Stateful Workflow Helpers

        internal static void Reload()
        {
            CurrentSource = default;
            CurrentTokens = default;
            CurrentAST = default;
            CurrentResult = Nil.Instance;
        }

        public static void Load(string source)
        {
            CurrentSource = source;
            CurrentTokens = default;
            CurrentAST = default;
        }

        public static void Lex(bool print)
        {
            if (CurrentSource == default)
                throw new UsageException("Workflow :: Lex :: Load a program first");

            CurrentTokens = Lex(CurrentSource, print);
        }

        public static void Postprocess(bool print)
        {
            if (CurrentSource == default)
                throw new UsageException("Workflow :: Postprocess :: Load a program first");

            if (CurrentTokens == default)
                throw new UsageException("Workflow :: Postprocess :: Lex a program first");

            CurrentTokens = Postprocess(CurrentSource!, CurrentTokens!, print);
        }

        public static void Parse(bool print)
        {
            if (CurrentSource == default)
                throw new UsageException("Workflow :: Parse :: Load a program first");

            if (CurrentTokens == default)
                throw new UsageException("Workflow :: Parse :: Lex a program first");

            CurrentAST = Parse(CurrentTokens!.ToArray(), print);
        }

        public static void Analyze(bool print)
        {
            if (CurrentSource == default)
                throw new UsageException("Workflow :: Analyze :: Load a program first");

            if (CurrentTokens == default)
                throw new UsageException("Workflow :: Analyze :: Lex a program first");

            if (CurrentAST == default)
                throw new UsageException("Workflow :: Analyze :: Parse a program first");

            Analyze(CurrentAST!, print);
            CurrentAST!.Analyzed = true;
        }

        public static void Run(bool verbose, bool printResult, bool printElapsed)
        {
            if (CurrentSource == default)
                throw new UsageException("Workflow :: Run :: Load a program first");

            if (CurrentTokens == default)
                throw new UsageException("Workflow :: Run :: Lex a program first");

            if (CurrentAST == default)
                throw new UsageException("Workflow :: Run :: Parse a program first");

            if (!CurrentAST.Analyzed)
                throw new UsageException("Workflow :: Run :: Analyze a program first");

            CurrentResult = Run(CurrentAST!, verbose, printResult, printElapsed);
        }

        #endregion

        #region Stateless Workflow Methods

        static readonly Stopwatch Stopwatch = new();

        private static IList<Token> Lex(string source, bool print)
        {
            if (print)
                Output.WriteLine("Workflow :: Lexing...", ConsoleColor.Yellow);

            Stopwatch.Restart();
            var tokens = Context.Lexer.Run(source, IgnoreWhitespace, IgnoreComments);
            Stopwatch.Stop();

            if (print)
            {
                Lexer.Print(tokens, verbose: VerboseTokens);
                Output.WriteLine($"Workflow :: Lex :: Done in {Stopwatch.Elapsed.TotalMilliseconds}ms", ConsoleColor.DarkGray);
            }

            return tokens;
        }

        private static IList<Token> Postprocess(string source, IList<Token> tokens, bool print)
        {
            if (print)
                Output.WriteLine("Workflow :: Postprocessing...", ConsoleColor.Yellow);

            Stopwatch.Restart();
            tokens = Context.Postprocessor.Run(source, tokens);
            Stopwatch.Stop();

            if (print)
            {
                Lexer.Print(tokens, verbose: VerboseTokens);
                Output.WriteLine($"Workflow :: Postprocess :: Done in {Stopwatch.Elapsed.TotalMilliseconds}ms", ConsoleColor.DarkGray);
            }

            return tokens;
        }

        private static ProgramNode Parse(Token[] tokens, bool print)
        {
            if (print)
                Output.WriteLine("Workflow :: Parsing...", ConsoleColor.Yellow);

            Stopwatch.Restart();
            var node = Context.Parser.Run(Context.SymbolTable, tokens.ToArray());
            Stopwatch.Stop();

            if (print)
            {
                Parser.Print(node);
                Output.WriteLine();
                Output.WriteLine($"Workflow :: Parse :: Done in {Stopwatch.Elapsed.TotalMilliseconds}ms", ConsoleColor.DarkGray);
            }

            return node;
        }

        private static void Analyze(ProgramNode node, bool print)
        {
            if (print)
                Output.WriteLine("Workflow :: Analyzing...", ConsoleColor.Yellow);

            Stopwatch.Restart();
            Context.Analyzer.Run(Context.SymbolTable, Context.GlobalScope, Context.GlobalFrame, node);
            Stopwatch.Stop();

            if (print)
            {
                Output.WriteLine();
                Output.WriteLine($"Workflow :: Analyze :: Done in {Stopwatch.Elapsed.TotalMilliseconds}ms", ConsoleColor.DarkGray);
            }
        }

        private static Program Run(ProgramNode node, bool verbose, bool printResult, bool printElapsed)
        {
            if (verbose)
                Output.WriteLine("Workflow :: Running...", ConsoleColor.Yellow);

            Stopwatch.Restart();
            var result = Context.Interpreter.Run(node);
            Stopwatch.Stop();

            if (printResult)
            {
                var serialized = Serializer.Serialize(Context.SymbolTable, result, verbose: VerboseValues);

                if (!string.IsNullOrEmpty(serialized))
                    Output.WriteLine(serialized, ConsoleColor.White);
            }

            if (printElapsed)
                Output.WriteLine($"Workflow :: Run :: Done in {Stopwatch.Elapsed.TotalMilliseconds}ms", ConsoleColor.DarkGray);

            return result;
        }

        #endregion

        #region Language Testing

        public static void Test() =>
            Test(Directory.GetFiles(Paths.TestPath, $"*{Settings.LanguageExtension}").OrderBy(x => x).ToArray());

        public static void Test(string path) =>
            Test(new[] { Path.Combine(Paths.TestPath, path) });

        /// <summary>
        /// This method is very important and used for testing the language features across changing implementations.
        /// </summary>
        public static void Test(string[] paths)
        {
            Output.WriteLine($"Workflow :: Test :: Testing {paths.Length} test files...", ConsoleColor.Yellow);

            int passedCount = 0,
                failedCount = 0;

            double totalElapsed = 0;
            var stopwatch = new Stopwatch();

            foreach (var path in paths)
            {
                string? filename = default;
                try
                {
                    filename = Path.GetFileName(path);
                    Output.WriteLine($"Workflow :: Testing {filename}", ConsoleColor.DarkYellow);

                    var source = File.ReadAllText(path);
                    Load(source);
                    Lex(print: false);
                    Postprocess(print: false);
                    Parse(print: false);
                    Analyze(print: false);

                    stopwatch.Restart();
                    Run(verbose: false, printResult: false, printElapsed: false);
                    stopwatch.Stop();
                    totalElapsed += stopwatch.Elapsed.TotalMilliseconds;

                    // A test construct is expected to return an Open OK / KO, and can contain multiple tests.
                    // Thus, we consider a test file to be successful if and only if all of its tests are passed.
                    // This is basically the same semantics as the evaluation of TestNode, extended to our ProgramNode.

                    var passed = IsTestPassed((Program)CurrentResult);

                    if (passed)
                    {
                        Output.WriteLine($"Workflow :: Testing {filename} OK", ConsoleColor.Green);
                        passedCount++;
                    }
                    else
                    {
                        // The "KO" test (file _ko.chi) is always failed: don't count it.
                        var isKoFile = filename == "_ko.chi";

                        Output.WriteLine($"Workflow :: Testing {filename} KO {(isKoFile ? "(skipped)" : "")}", ConsoleColor.Red);
                        if (!isKoFile) failedCount++;
                    }
                }
                catch (Exception ex)
                {
                    Output.WriteLine($"Workflow :: Testing {filename ?? path} KO with exception :: {ex.Message}", ConsoleColor.Red);
                    failedCount++;
                }
                finally
                {
                    // It's important to reset the Context state after each test,
                    // especially when exceptions are thrown, to avoid side effects.
                    Context.Reload(isRepl: true);
                }
            }
            stopwatch.Stop();
            Output.WriteLine($"Workflow :: Test :: Tested in {totalElapsed:F4}ms", ConsoleColor.DarkGray);
            Output.WriteLine($"Workflow :: Test :: {passedCount} OK, {failedCount} KO", ConsoleColor.Black, failedCount == 0 ? ConsoleColor.Green : ConsoleColor.Red);
            Output.WriteLine();
        }

        public static bool IsTestPassed(List<IValueNode> testResults) =>
            testResults.Any(v => IsValueOK(v)) &&
            testResults.All(v => v is Nil || v is Definition || IsValueOK(v));

        static bool IsValueOK(IValueNode value) =>
            value is Open open && Symbol.Equal(open.Value, Context.SymbolOK);

        #endregion
    }
}
