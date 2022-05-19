using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.EBNFOperators
{
    public abstract class UnaryOperator : Operator
    {
        public Operator InnerOperator { get { return _operators[0]; } set { _operators[0] = value; } }

        public UnaryOperator()
        {
            _operators = new List<Operator>() { null };
        }
        public UnaryOperator(Operator inner)
        {
            _operators = new List<Operator>() { null };
            InnerOperator = inner;
        }

        public T Inner<T>() where T : Operator
        {
            return (T)InnerOperator;
        }
    }
}
