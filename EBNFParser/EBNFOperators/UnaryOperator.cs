using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.EBNFOperators
{
    public class UnaryOperator : Operator
    {
        public Operator Inner;

        public UnaryOperator() { }
        public UnaryOperator(Operator inner)
        {
            Inner = inner;
        }
    }
}
