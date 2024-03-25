using System.Diagnostics;
using Chi.Lexing;
using Chi.Runtime;
using Chi.Runtime.Data;
using Chi.Shared;
using Chi.Parsing.Syntax;
using Chi.Runtime.Abstract;

namespace Chi
{
    public class REPL
    {
        readonly IInterpreter Interpreter;
        string? CurrentSource;
        IList<Token>? CurrentTokens;
        ProgramNode? CurrentAST;

        public REPL(IInterpreter interpreter) =>
            Interpreter = interpreter;

        #region Command & Options

        bool IgnoreWhitespace = true;
        bool IgnoreComments = true;
        bool Verbose = false;

        const string CommandQuit = "/quit";
        const string CommandUsage = "/usage";
        const string CommandOption = "/option";
        const string CommandSource = "/source";
        const string CommandFile = "/file";
        const string CommandLex = "/lex";
        const string CommandPost = "/post";
        const string CommandParse = "/parse";
        const string CommandRun = "/run";
        const string CommandLexPost = "/lex-post";
        const string CommandLexPostParse = "/lex-post-parse";
        const string CommandLexPostParseRun = "/lex-post-parse-run";
        const string CommandReset = "/reset";
        const string CommandTest = "/test";

        #endregion

        #region Workflow Helpers

        void Lex(bool ignoreWhitespace, bool ignoreComments, bool print, bool verbose) =>
            CurrentTokens = Workflow.Lex(CurrentSource!, ignoreWhitespace, ignoreComments, print, verbose);

        void Post(bool print, bool verbose) =>
            CurrentTokens = Workflow.Postprocess(CurrentSource!, CurrentTokens!, print, verbose);

        void Parse(bool print) =>
            CurrentAST = Workflow.Parse(CurrentTokens?.ToArray()!, print);

        Program Interpret(bool print, bool verbose) =>
            Workflow.Run(Interpreter, CurrentAST!, print, verbose);

        void Reset()
        {
            CurrentSource = default;
            CurrentTokens = default;
            CurrentAST = default;
            Interpreter.Reset();
        }

        #endregion

        #region Read-Eval-Print Loop

        public void Loop()
        {
            while (true)
            {
                try
                {
                    // Prompt.
                    Output.Write($"{Settings.LanguageName} >", ConsoleColor.White, ConsoleColor.DarkBlue);
                    Output.Write(" ", ConsoleColor.White, ConsoleColor.Black);

                    var input = (Console.ReadLine() ?? string.Empty).Trim();

                    if (input == CommandQuit)
                    {
                        // Quit.
                        Output.WriteLine("Bye!");
                        return;
                    }
                    else if (input == CommandUsage)
                    {
                        // Show usage.
                        ShowUsage();
                    }
                    else if (input.StartsWith($"{CommandOption} "))
                    {
                        // Set Option.
                        SetOption(input);
                    }
                    else if (input.StartsWith($"{CommandSource} "))
                    {
                        // Use the specified source code for subsequent commands.
                        CurrentSource = input[$"{CommandSource} ".Length..];
                        CurrentTokens = default;
                        CurrentAST = default;
                    }
                    else if (input.StartsWith($"{CommandFile} "))
                    {
                        // Load source code from a file for subsequent commands.
                        var filename = input[$"{CommandFile} ".Length..].Trim();
                        var filepath = Path.Combine(Paths.SourcesPath, filename);
                        CurrentSource = File.ReadAllText(filepath);
                        CurrentTokens = default;
                        CurrentAST = default;
                    }
                    else if (input == CommandLex)
                    {
                        // Lex the loaded source code.

                        if (CurrentSource == default)
                            throw new UsageException("Load a program first");

                        Lex(IgnoreWhitespace, IgnoreComments, print: true, Verbose);
                    }
                    else if (input == CommandPost)
                    {
                        // Postprocess the current tokens.

                        if (CurrentSource == default)
                            throw new UsageException("Load a program first");
                        if (CurrentTokens == default)
                            throw new UsageException("Lex a program first");

                        Post(print: true, Verbose);
                    }
                    else if (input == CommandParse)
                    {
                        // Parse the current tokens.

                        if (CurrentSource == default)
                            throw new UsageException("Load a program first");
                        if (CurrentTokens == default)
                            throw new UsageException("Lex a program first");

                        Parse(print: true);
                    }
                    else if (input == CommandRun)
                    {
                        // Run the current AST.

                        if (CurrentSource == default)
                            throw new UsageException("Load a program first");
                        if (CurrentTokens == default)
                            throw new UsageException("Lex a program first");
                        if (CurrentAST == default)
                            throw new UsageException("Parse a program first");

                        Interpret(print: true, verbose: true);
                    }
                    else if (input.StartsWith($"{CommandRun} "))
                    {
                        // Run the specified file.
                        // Equivalent to /file + /lex-post-parse-run, but with no intermediate output.

                        var filename = input[$"{CommandRun} ".Length..].Trim();
                        var filepath = Path.Combine(Paths.SourcesPath, filename);
                        CurrentSource = File.ReadAllText(filepath);
                        Lex(IgnoreWhitespace, IgnoreComments, print: false, Verbose);
                        Post(print: false, Verbose);
                        Parse(print: false);
                        Interpret(print: true, verbose: false);
                    }
                    else if (input == CommandLexPost)
                    {
                        // Lex and Postprocess the current source code.

                        if (CurrentSource == default)
                            throw new UsageException("Load a program first");

                        Lex(IgnoreWhitespace, IgnoreComments, print: true, Verbose);
                        Post(print: true, Verbose);
                    }
                    else if (input == CommandLexPostParse)
                    {
                        // Lex, Postprocess and Parse the current source code.

                        if (CurrentSource == default)
                            throw new UsageException("Load a program first");

                        Lex(IgnoreWhitespace, IgnoreComments, print: true, Verbose);
                        Post(print: true, Verbose);
                        Parse(print: true);
                    }
                    else if (input == CommandLexPostParseRun)
                    {
                        // Lex, Postprocess, Parse and Run the current source code.

                        if (CurrentSource == default)
                            throw new UsageException("Load a program first");

                        Lex(IgnoreWhitespace, IgnoreComments, print: true, Verbose);
                        Post(print: true, Verbose);
                        Parse(print: true);
                        Interpret(print: true, verbose: true);
                    }
                    else if (input == CommandReset)
                    {
                        // Reset the interpreter state.
                        Reset();
                    }
                    else if (input == CommandTest)
                    {
                        // Run all language tests.
                        Test();
                    }
                    else if (input.StartsWith($"{CommandTest} "))
                    {
                        // Run the specified test file.
                        var filename = input[$"{CommandTest} ".Length..].Trim();
                        var filepath = Path.Combine(Paths.TestPath, filename);
                        Test(filepath);
                    }
                    else
                    {
                        // Run the specified source code.
                        CurrentSource = input;
                        Lex(ignoreWhitespace: true, ignoreComments: true, print: false, verbose: false);
                        Post(print: false, verbose: false);
                        Parse(print: false);
                        Interpret(print: true, verbose: false);
                    }
                }
                catch (UsageException ex)
                {
                    Output.WriteLine($"REPL :: Invalid Usage :: {ex.Message}", ConsoleColor.Red);
                    Output.WriteLine($"REPL :: Use {CommandUsage} for more info", ConsoleColor.Red);
                    Output.WriteLine();
                }
                catch (SyntaxException ex)
                {
                    Output.WriteLine($"REPL :: Parse Error :: {ex.Message}", ConsoleColor.Red);
                    Output.WriteLine();
                }
                catch(RuntimeException ex)
                {
                    Output.WriteLine($"REPL :: Runtime Error :: {ex.Message}", ConsoleColor.Red);
                    Output.WriteLine();
                }
                catch (Exception ex)
                {
                    Output.WriteLine($"REPL :: {ex.Message}", ConsoleColor.Red);
                    Output.WriteLine();
                }
            }
        }

        #endregion

        #region REPL Helpers

        static void ShowUsage()
        {
            Output.WriteLine("Usage :: Commands", ConsoleColor.Yellow);
            Output.WriteLine($"{CommandQuit}\t\t\t Quit the Chi REPL.", ConsoleColor.DarkYellow);
            Output.WriteLine($"{CommandUsage}\t\t\t Shows the Chi REPL usage.", ConsoleColor.DarkYellow);
            Output.WriteLine($"{CommandOption} <key> <value>\t Set Option. <key>: [verbose|whitespace|comments], <value>: [true|false].", ConsoleColor.DarkYellow);
            Output.WriteLine($"{CommandSource} <source>\t Use <source> for subsequent commands.", ConsoleColor.DarkYellow);
            Output.WriteLine($"{CommandFile} <filename>\t Load <filename> for subsequent commands.", ConsoleColor.DarkYellow);
            Output.WriteLine($"{CommandLex}\t\t\t Run Lexer on current source, producing some tokens.", ConsoleColor.DarkYellow);
            Output.WriteLine($"{CommandPost}\t\t\t Run Postprocessor on current tokens, producing some tokens.", ConsoleColor.DarkYellow);
            Output.WriteLine($"{CommandLexPost}\t\t Run Lexer and Postprocessor on current source, producing some tokens.", ConsoleColor.DarkYellow);
            Output.WriteLine($"{CommandParse}\t\t\t Run Parser on current tokens, producing an AST", ConsoleColor.DarkYellow);
            Output.WriteLine($"{CommandLexPostParse}\t\t Run Lexer, Postprocessor and Parser on current source, producing an AST.", ConsoleColor.DarkYellow);
            Output.WriteLine($"{CommandRun}\t\t\t Run Interpreter on current AST, producing a result value (accessible with _).", ConsoleColor.DarkYellow);
            Output.WriteLine($"{CommandLexPostParseRun}\t Run Interpreter on current source, producing a result value (accessible with _)", ConsoleColor.DarkYellow);
            Output.WriteLine($"{CommandReset}\t\t\t Reset the REPL and Interpreter state.", ConsoleColor.DarkYellow);
            Output.WriteLine($"{CommandTest}\t\t\t Run all language tests.", ConsoleColor.DarkYellow);
            Output.WriteLine();
        }

        void SetOption(string input) => _ = input switch
        {
            $"{CommandOption} verbose true" => Verbose = true,
            $"{CommandOption} verbose false" => Verbose = false,
            $"{CommandOption} whitespace true" => IgnoreWhitespace = false,
            $"{CommandOption} whitespace false" => IgnoreWhitespace = true,
            $"{CommandOption} comments true" => IgnoreComments = false,
            $"{CommandOption} comments false" => IgnoreComments = true,

            _ => throw new UsageException("Wrong arguments"),
        };

        void Test() => 
            Test(Directory.GetFiles(Paths.TestPath, $"*{Settings.LanguageExtension}").OrderBy(x => x).ToArray());

        void Test(string path) =>
            Test(new[] { Path.Combine(Paths.TestPath, path) });

        void Test(string[] paths)
        {
            Output.WriteLine($"Test :: File {paths.Length} test files", ConsoleColor.Yellow);
            
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
                    Output.WriteLine($"Test :: File {filename}", ConsoleColor.DarkYellow);
                    
                    CurrentSource = File.ReadAllText(path);
                    Lex(ignoreWhitespace: true, ignoreComments: true, print: false, verbose: false);
                    Post(print: false, verbose: false);
                    Parse(print: false);

                    stopwatch.Restart();
                    var value = Interpret(print: false, verbose: false);
                    stopwatch.Stop();
                    totalElapsed += stopwatch.Elapsed.TotalMilliseconds;

                    // A test construct is expected to return an Open OK / KO, and a test file can contain multiple tests.
                    // Thus, we consider a test file to be successful if and only if all of its tests are passed.
                    // This is basically the same semantics as the evaluation of TestNode, extended to ProgramNode.
                    // A test program shall not contain definitions, statements (eg. set) and "free" expressions.

                    var passed = value!.All(r => r is Open open && open.Value == "OK");

                    if (passed)
                    {
                        Output.WriteLine($"Test :: {filename} OK", ConsoleColor.Green);
                        passedCount++;
                    }
                    else
                    {
                        // The "KO" test (file _ko.chi) is always failed: don't count it.
                        var isKoFile = filename == "_ko.chi";

                        Output.WriteLine($"Test :: {filename} KO {(isKoFile ? "(skipped)" : "")}", ConsoleColor.Red);

                        if (!isKoFile)
                            failedCount++;
                    }
                }
                catch (Exception ex)
                {
                    Output.WriteLine($"Test :: {filename ?? path} KO with exception :: {ex.Message}", ConsoleColor.Red);
                    failedCount++;
                }
                finally
                {
                    // It's important to reset the interpreter state after each test.
                    Reset();
                }
            }
            stopwatch.Stop();
            Output.WriteLine($"Test :: Interpreted in {totalElapsed:F4}ms", ConsoleColor.DarkGray);
            Output.WriteLine($"Test :: {passedCount} OK, {failedCount} KO", ConsoleColor.Black, failedCount == 0 ? ConsoleColor.Green : ConsoleColor.Red);
            Output.WriteLine();
        }

        #endregion
    }
}
