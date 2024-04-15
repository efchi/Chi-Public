namespace Chi.Parsing.Data
{
    /// <summary>
    /// Symbol Table for our frontend and runtime.
    /// The symbol table is used to store all the symbols (identifiers) found during parsing
    /// and more generally during the execution of one or multiple programs. In fact, the symbol
    /// table is shared across different execution, and its lifetime is managed by the Context.
    /// </summary>
    public class SymbolTable
    {
        readonly Dictionary<int, Symbol> SymbolCodes = new();
        readonly Dictionary<string, Symbol> SymbolStrings = new();

        public SymbolTable()
        {
            // Adding the <null> symbol.
            var code = 0;
            var identifier = "<null>";
            var symbol = new Symbol(code, identifier);
            SymbolCodes.Add(code, symbol);
            SymbolStrings.Add(identifier, symbol);
        }

        /// <summary>
        /// Get or create a symbol from a string.
        /// </summary>
        public Symbol GetOrCreate(string @string)
        {
            if (SymbolStrings.TryGetValue(@string, out var symbol))
                return symbol;

            var code = SymbolCodes.Count;
            symbol = new Symbol(code, @string);

            SymbolCodes.Add(code, symbol);
            SymbolStrings.Add(@string, symbol);

            return symbol;
        }

        /// <summary>
        /// Lookup a symbol by its code.
        /// </summary>
        public Symbol GetByCode(int code)
        {
            if (!SymbolCodes.TryGetValue(code, out var symbol))
                throw new ArgumentException($"No Symbol found for code {code}.");

            return symbol;
        }

        /// <summary>
        /// Lookup a symbol by its string.
        /// </summary>
        public Symbol GetByIdentifier(string @string)
        {
            if (!SymbolStrings.TryGetValue(@string, out var symbol))
                throw new ArgumentException($"No Symbol found for string {@string}.");

            return symbol;
        }
    }
}
