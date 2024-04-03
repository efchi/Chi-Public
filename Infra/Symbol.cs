namespace Chi.Infra
{
    public readonly struct Symbol
    {
        public readonly int Code;
        public readonly string Identifier;

        public Symbol(int code, string identifier) =>
            (Code, Identifier) = (code, identifier);

        public override string ToString() =>
            $"[Symbol #{Code} = {Identifier}]";
    }
}