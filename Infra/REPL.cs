using Chi.Shared;

namespace Chi.Infra
{
    /// <summary>
    /// This class is responsible for managing user input and the Read-Eval-Print Loop (REPL).
    /// </summary>
    public class REPL
    {
        const string CommandQuit = "/quit";
        const string CommandUsage = "/usage";
        const string CommandOption = "/option";
        const string CommandSource = "/source";
        const string CommandFile = "/file";
        const string CommandLex = "/lex";
        const string CommandPost = "/post";
        const string CommandParse = "/parse";
        const string CommandAnalyze = "/analyze";
        const string CommandRun = "/run";
        const string CommandLexPost = "/lex /post";
        const string CommandLexPostParse = "/lex /post /parse";
        const string CommandLexPostParseAnalyze = "/lex /post /parse /analyze";
        const string CommandLexPostParseAnalyzeRun = "/lex /post /parse /analyze /run";
        const string CommandReload = "/reload";
        const string CommandTest = "/test";

        #region Read-Eval-Print Loop

        public void Loop()
        {
            while (true)
            {
                try
                {
                    Output.Write($"{Settings.LanguageName} >", ConsoleColor.White, ConsoleColor.DarkBlue);
                    Output.Write(" ", ConsoleColor.White, ConsoleColor.Black);
                    
                    // Read prompt.
                    var input = (Console.ReadLine() ?? string.Empty).Trim();

                    if (input == CommandQuit)
                    {
                        Output.WriteLine("Bye!");
                        return;
                    }
                    else if (input == CommandUsage)
                    {
                        ShowUsage();
                    }
                    else if (input.StartsWith($"{CommandOption} "))
                    {
                        SetOption(input);
                    }
                    else if (input.StartsWith($"{CommandSource} "))
                    {
                        // Use the specified source code for subsequent commands.

                        var source = input[$"{CommandSource} ".Length..];
                        Workflow.Load(source);
                    }
                    else if (input.StartsWith($"{CommandFile} "))
                    {
                        // Load source code from a file for subsequent commands.

                        var filename = input[$"{CommandFile} ".Length..].Trim();
                        var filepath = Path.Combine(Paths.SourcesPath, filename);
                        var source = File.ReadAllText(filepath);   
                        Workflow.Load(source);
                    }
                    else if (input == CommandLex)
                    {
                        // Lex the loaded source code.
                        Workflow.Lex(print: true);
                    }
                    else if (input == CommandPost)
                    {
                        // Postprocess the current tokens.
                        Workflow.Postprocess(print: true);
                    }
                    else if (input == CommandParse)
                    {
                        // Parse the current tokens.
                        Workflow.Parse(print: true);
                    }
                    else if (input == CommandAnalyze)
                    {
                        // Analyze the current AST.
                        Workflow.Analyze(print: true);
                    }
                    else if (input == CommandRun)
                    {
                        // Run the current AST.
                        Workflow.Run(verbose: true, printResult: true, printElapsed: true);
                    }
                    else if (input == CommandLexPost)
                    {
                        // Lex and Postprocess the current source code.
                        Workflow.Lex(print: true);
                        Workflow.Postprocess(print: true);
                    }
                    else if (input == CommandLexPostParse)
                    {
                        // Lex, Postprocess and Parse the current source code.
                        Workflow.Lex(print: true);
                        Workflow.Postprocess(print: true);
                        Workflow.Parse(print: true);
                    }
                    else if (input == CommandLexPostParseAnalyze)
                    {
                        // Lex, Postprocess, Parse and Analyze the current source code.
                        Workflow.Lex(print: true);
                        Workflow.Postprocess(print: true);
                        Workflow.Parse(print: true);
                        Workflow.Analyze(print: true);
                    }
                    else if (input == CommandLexPostParseAnalyzeRun)
                    {
                        // Lex, Postprocess, Parse, Analyze and Run the current source code.
                        Workflow.Lex(print: true);
                        Workflow.Postprocess(print: true);
                        Workflow.Parse(print: true);
                        Workflow.Analyze(print: true);
                        Workflow.Run(verbose: true, printResult: true, printElapsed: true);
                    }
                    else if (input.StartsWith($"{CommandRun} "))
                    {
                        // Run the specified file.
                        // Equivalent to /file + /lex /post /parse /analyze /run, but with no intermediate output.

                        var filename = input[$"{CommandRun} ".Length..].Trim();
                        var filepath = Path.Combine(Paths.SourcesPath, filename);
                        var source = File.ReadAllText(filepath);

                        Workflow.Load(source);
                        Workflow.Lex(print: false);
                        Workflow.Postprocess(print: false);
                        Workflow.Parse(print: false);
                        Workflow.Analyze(print: false);
                        Workflow.Run(verbose: true, printResult: true, printElapsed: true);
                    }
                    else if (input.StartsWith(CommandTest))
                    {
                        if (input != CommandTest)
                        {
                            // Run the specified test file ("/test file.chi").

                            var filename = input[$"{CommandTest} ".Length..].Trim();
                            var filepath = Path.Combine(Paths.TestPath, filename);
                            Workflow.Test(filepath);
                        }
                        else
                            // Run all language tests.
                            Workflow.Test();
                    }
                    else if (input == CommandReload)
                    {
                        // Reset the runtime state.
                        Context.Reload(isRepl: true);
                    }
                    else
                    {
                        // Run the specified source code.
                        Workflow.Load(input);
                        Workflow.Lex(print: false);
                        Workflow.Postprocess(print: false);
                        Workflow.Parse(print: false);
                        Workflow.Analyze(print: false);
                        Workflow.Run(verbose: false, printResult: true, printElapsed: true);
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
                    Output.WriteLine($"REPL :: Syntax Error :: {ex.Message}", ConsoleColor.Red);
                    Output.WriteLine();
                }
                catch (RuntimeException ex)
                {
                    Output.WriteLine($"REPL :: Runtime Error :: {ex.Message}", ConsoleColor.Red);
                    Output.WriteLine();
                }
                catch (Exception ex)
                {
                    Output.WriteLine($"REPL :: Unmanaged Error :: {ex.Message}", ConsoleColor.Red);
                    Output.WriteLine();
                }
            }
        }

        #endregion

        #region REPL Command Helpers

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
            Output.WriteLine($"{CommandAnalyze}\t\t Run Analyzer on current AST, populating the current Symbol Table and Global Scope.", ConsoleColor.DarkYellow);
            Output.WriteLine($"{CommandLexPostParse}\t\t Run Lexer, Postprocessor and Parser on current source, producing an AST.", ConsoleColor.DarkYellow);
            Output.WriteLine($"{CommandLexPostParseAnalyze}\t Run Lexer, Postprocessor, Parser and Analyzer on current source, producing an AST.", ConsoleColor.DarkYellow);
            Output.WriteLine($"{CommandRun}\t\t\t Run Interpreter on current AST, producing a result value (accessible with _).", ConsoleColor.DarkYellow);
            Output.WriteLine($"{CommandLexPostParseAnalyzeRun}\t Run Interpreter on current source, producing a result value (accessible with _)", ConsoleColor.DarkYellow);
            Output.WriteLine($"{CommandReload}\t\t\t Reset the REPL and Interpreter state.", ConsoleColor.DarkYellow);
            Output.WriteLine($"{CommandTest}\t\t\t Run all language tests.", ConsoleColor.DarkYellow);
            Output.WriteLine();
        }

        void SetOption(string input) => _ = input switch
        {
            $"{CommandOption} verbose-tokens true" => Workflow.VerboseTokens = true,
            $"{CommandOption} verbose-tokens false" => Workflow.VerboseTokens = false,
            $"{CommandOption} verbose-values true" => Workflow.VerboseValues = true,
            $"{CommandOption} verbose-values false" => Workflow.VerboseValues = false,
            $"{CommandOption} whitespace true" => Workflow.IgnoreWhitespace = false,
            $"{CommandOption} whitespace false" => Workflow.IgnoreWhitespace = true,
            $"{CommandOption} comments true" => Workflow.IgnoreComments = false,
            $"{CommandOption} comments false" => Workflow.IgnoreComments = true,

            _ => throw new UsageException("Wrong arguments"),
        };

        #endregion
    }
}
