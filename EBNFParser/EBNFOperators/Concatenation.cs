using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBNFParser.EBNFOperators
{
    public class Concatenation : BinaryOperator
    {
        public Concatenation() { }
        public Concatenation(Operator left, Operator right) : base(left, right) { }
    }
}
