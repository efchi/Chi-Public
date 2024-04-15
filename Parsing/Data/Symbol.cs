namespace Chi.Parsing.Data
{
    /// <summary>
    /// This class represents a Symbol of the language, that is an identifier 
    /// stored in the symbol table with a unique code as a key.
    /// </summary>
    public class Symbol
    {
        public readonly int Code;
        public readonly string String;

        public Symbol(int code, string @string) =>
            (Code, String) = (code, @string);

        public override string ToString() =>
            $"(Symbol {String} #{Code})";

        public static bool Equal(Symbol a, Symbol b) =>
            ReferenceEquals(a, b) || a.Code == b.Code;

        #region Disabled Equality Methods

        public override bool Equals(object? obj) => 
            throw new NotSupportedException();
        
        public override int GetHashCode() => 
            throw new NotSupportedException();
        
        public static bool operator ==(Symbol left, Symbol right) => 
            throw new NotSupportedException();
        
        public static bool operator !=(Symbol left, Symbol right) => 
            throw new NotSupportedException();

        #endregion
    }
}