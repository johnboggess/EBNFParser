using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.EBNFOperators
{
    public class Exception : BinaryOperator
    {
        public Exception() { }
        public Exception(Operator left, Operator right) : base(left, right) { }
    }
}
