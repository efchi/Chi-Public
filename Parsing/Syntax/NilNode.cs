using Chi.Parsing.Syntax.Abstract;

namespace Chi.Parsing.Syntax
{
    public class NilNode :  IExpressionNode
    {
        // No need to have multiple NilNode instances, so we implement it a singleton.
        public static readonly NilNode Instance = new();

        private NilNode()
        {
        }
    }
}
