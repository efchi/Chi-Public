using Chi.Infra;
using Chi.Runtime.Data.Abstract;

namespace Chi.Runtime.Data
{
    public class Open : IValueNode
    {
        public readonly Symbol Value;

        public Open(Symbol value) =>
            Value = value;
    }
}
