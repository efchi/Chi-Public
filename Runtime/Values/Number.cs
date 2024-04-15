using Chi.Runtime.Values.Abstract;

namespace Chi.Runtime.Values
{
    public class Number : IValueNode 
    {
        public readonly decimal Value; 
    
        public Number(decimal value) => 
            Value = value;
    }
}
