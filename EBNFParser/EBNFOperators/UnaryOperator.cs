using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.EBNFOperators
{
    public class UnaryOperator : Operator
    {
        private Operator _innerOperator;
        public Operator InnerOperator { get { return _innerOperator; } set { _innerOperator = value; } }

        public UnaryOperator() { }
        public UnaryOperator(Operator inner)
        {
            InnerOperator = inner;
        }

        public T Inner<T>() where T : Operator
        {
            return (T)InnerOperator;
        }
    }
}
