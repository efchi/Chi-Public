using Chi.Runtime.Values.Abstract;

namespace Chi.Runtime.Values
{
    /// <summary>
    /// Represents a program result as a list of values (similar to a tuple).
    /// </summary>
    public class Program : List<IValueNode>, IValueNode
    {
    }
}
