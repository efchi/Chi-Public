using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class WildcardNode : IExpressionNode
    {
        // No need to have multiple WildcardNode instances, so we implement it a singleton.
        public static readonly WildcardNode Instance = new();

        private WildcardNode() 
        {
        }
    }
}
