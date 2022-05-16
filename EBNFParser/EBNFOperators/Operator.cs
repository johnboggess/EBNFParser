using EBNFParser.EBNFOperators;

namespace EBNFParser
{
    public abstract class Operator
    {
        protected List<Operator> _operators = new List<Operator>();
        public IReadOnlyCollection<Operator> Operators { get { return _operators.AsReadOnly(); } }
        public Operator() { }
    }
}
