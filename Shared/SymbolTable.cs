namespace Chi.Shared
{
    public class SymbolTable
    {
        static readonly Random Random = new();

        readonly Dictionary<int, string> CodeIdentifier = new();
        readonly Dictionary<string, int> IdentifierCode = new();
        
        public string Get(int code)
        {
            if (!CodeIdentifier.TryGetValue(code, out var identifier))
                throw new ArgumentException($"No identifier found for code {code}.");

            return identifier;
        }

        public int Get(string identifier)
        {
            if (IdentifierCode.TryGetValue(identifier, out var code))
                return code;

            // Random code generation.
            do { code = Random.Next(); }
            while (CodeIdentifier.ContainsKey(code));

            CodeIdentifier.Add(code, identifier);
            IdentifierCode.Add(identifier, code);

            return code;
        }
    }
}
