using Chi.Lexing;
using Chi.Parsing;
using Chi.Parsing.Syntax;
using Chi.Runtime.Abstract;
using Chi.Runtime.Data;
using Chi.Shared;
using System.Diagnostics;

namespace Chi.Runtime
{
    internal class Workflow
    {
        static readonly Stopwatch Stopwatch = new();

        public static IList<Token> Lex(string source, bool ignoreWhitespace, bool ignoreComments, bool print, bool verbose)
        {
            if (print)
                Output.WriteLine("Lexing...", ConsoleColor.Yellow);

            var lexer = new Lexer(source, ignoreWhitespace, ignoreComments);

            Stopwatch.Start();
            var tokens = lexer.Run();
            Stopwatch.Stop();

            if (print)
            {
                Lexer.Print(tokens, verbose, ignoreWhitespace, ignoreComments);
                Output.WriteLine($"Lex :: Done in {Stopwatch.Elapsed.TotalMilliseconds}ms", ConsoleColor.DarkGray);
            }

            Stopwatch.Reset();
            return tokens;
        }

        public static IList<Token> Postprocess(string source, IList<Token> tokens, bool print, bool verbose)
        {
            if (print)
                Output.WriteLine("Postprocessing...", ConsoleColor.Yellow);

            var postprocessor = new Postprocessor(source);

            Stopwatch.Start();
            tokens = postprocessor.Run(tokens);
            Stopwatch.Stop();

            if (print)
            {
                Lexer.Print(tokens, verbose, ignoreWhitespace: true, ignoreComments: true);
                Output.WriteLine($"Postprocess :: Done in {Stopwatch.Elapsed.TotalMilliseconds}ms", ConsoleColor.DarkGray);
            }

            Stopwatch.Reset();
            return tokens;
        }

        public static ProgramNode Parse(SymbolTable symbols, Token[] tokens, bool print)
        {
            if (print)
                Output.WriteLine("Parsing...", ConsoleColor.Yellow);

            var parser = new Parser(symbols, tokens.ToArray());

            Stopwatch.Start();
            var node = parser.Run();
            Stopwatch.Stop();

            if (print)
            {
                parser.Print(node);
                Output.WriteLine();
                Output.WriteLine($"Parse :: Done in {Stopwatch.Elapsed.TotalMilliseconds}ms", ConsoleColor.DarkGray);
            }

            Stopwatch.Reset();
            return node;
        }

        public static Program Run(IInterpreter interpreter, ProgramNode node, bool print, bool verbose)
        {
            if (verbose)
                Output.WriteLine("Running...", ConsoleColor.Yellow);

            Stopwatch.Start();
            var result = interpreter.Run(node);
            Stopwatch.Stop();

            if (print)
            {
                var serialized = Serializer.Serialize(interpreter.Symbols, result, verbose: true);

                if (!string.IsNullOrWhiteSpace(serialized))
                    Output.WriteLine(serialized, ConsoleColor.White);

                Output.WriteLine($"Run :: Done in {Stopwatch.Elapsed.TotalMilliseconds}ms", ConsoleColor.DarkGray);
            }

            Stopwatch.Reset();
            return result;
        }
    }
}
