using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.EBNFOperators
{
    public class Exception : UnaryOperator
    {
        public Exception() { }
        public Exception(Operator inner) :base(inner) { }
    }
}
