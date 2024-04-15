using Chi.Runtime.Values.Abstract;

namespace Chi.Runtime.Values
{
    public class Nil : IValueNode
    {
        // No need to have multiple Nil instances, so we implement it a singleton.
        public static readonly Nil Instance = new();

        private Nil()
        {
        }
    }
}
