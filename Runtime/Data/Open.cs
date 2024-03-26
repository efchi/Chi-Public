using Chi.Runtime.Data.Abstract;

namespace Chi.Runtime.Data
{
    public class Open : IValueNode
    {
        public readonly int Symbol;

        public Open(int symbol) =>
            Symbol = symbol;
    }
}
