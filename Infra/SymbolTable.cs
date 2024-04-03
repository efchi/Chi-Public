namespace Chi.Infra
{
    public class SymbolTable
    {
        readonly Dictionary<int, Symbol> SymbolCodes = new();
        readonly Dictionary<string, Symbol> SymbolIdentifiers = new();

        public Symbol GetOrCreate(string identifier)
        {
            if (SymbolIdentifiers.TryGetValue(identifier, out var symbol))
                return symbol;

            // Symbol code 0 is reserved for the <null> symbol (default(Symbol)).
            var code = SymbolCodes.Count + 1;
            symbol = new Symbol(code, identifier);

            SymbolCodes.Add(code, symbol);
            SymbolIdentifiers.Add(identifier, symbol);

            return symbol;
        }

        public Symbol GetByCode(int code)
        {
            if (!SymbolCodes.TryGetValue(code, out var symbol))
                throw new ArgumentException($"No Symbol found for code {code}.");

            return symbol;
        }

        public Symbol GetByIdentifier(string identifier)
        {
            if (!SymbolIdentifiers.TryGetValue(identifier, out var symbol))
                throw new ArgumentException($"No Symbol found for identifier {identifier}.");

            return symbol;
        }
    }
}
