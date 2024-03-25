using Chi.Runtime.Data.Abstract;

namespace Chi.Runtime.Data
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
