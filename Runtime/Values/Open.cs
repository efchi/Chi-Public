using Chi.Parsing.Data;
using Chi.Runtime.Values.Abstract;

namespace Chi.Runtime.Values
{
    public class Open : IValueNode
    {
        public readonly Symbol Value;

        public Open(Symbol value) =>
            Value = value;
    }
}
