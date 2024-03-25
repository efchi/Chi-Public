using Chi.Runtime.Data.Abstract;

namespace Chi.Runtime.Data
{
    public class Open : IValueNode
    {
        public readonly string Value;

        public Open(string value) =>
            Value = value;
    }
}
