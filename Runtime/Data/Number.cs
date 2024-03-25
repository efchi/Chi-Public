using Chi.Runtime.Data.Abstract;

namespace Chi.Runtime.Data
{
    public class Number : IValueNode 
    {
        public readonly decimal Value; 
    
        public Number(decimal value) => 
            Value = value;
    }
}
